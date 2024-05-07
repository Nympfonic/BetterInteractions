// Thanks to Chris Nolet for his QuickOutline project
// Source Repository: https://github.com/chrisnolet/QuickOutline

using BepInEx.Configuration;
using EFT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Arys.BetterInteractions.Components
{
    [DisallowMultipleComponent]
    public class BetterInteractionsOutline : MonoBehaviour
    {
        internal static readonly HashSet<Mesh> RegisteredMeshes = [];

        private static readonly int zTestId = Shader.PropertyToID("_ZTest");
        private static readonly int colorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int widthId = Shader.PropertyToID("_OutlineWidth");

        private Renderer[] _renderers;
        private Material _maskMaterial;
        private Material _fillMaterial;
        private Coroutine _initialisationCoroutine;
        private bool _isInitialised = false;

        public void ToggleOutline(bool enabled)
        {
            void action()
            {
                foreach (var renderer in _renderers)
                {
                    var materials = renderer.sharedMaterials.ToList();
                    if (enabled)
                    {
                        materials.Add(_maskMaterial);
                        materials.Add(_fillMaterial);
                    }
                    else
                    {
                        materials.Remove(_maskMaterial);
                        materials.Remove(_fillMaterial);
                    }
                    renderer.materials = [.. materials];
                }
            }

            if (IsInitialised())
            {
                action();
            }
            else
            {
                _initialisationCoroutine = StaticManager.BeginCoroutine(DoAfterInitialisation(action));
            }
        }

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();

            _maskMaterial = Plugin.OutlineMaskMaterial;
            _fillMaterial = Plugin.OutlineFillMaterial;

            _maskMaterial.SetFloat(zTestId, (float)CompareFunction.Always);
            _fillMaterial.SetFloat(zTestId, (float)CompareFunction.LessEqual);
        }

        private async void Start()
        {
            await LoadSmoothNormalsAsync();

            Plugin.Configuration.SettingChanged += UpdateOutlineSettings;

            UpdateOutlineSettings();

            _isInitialised = true;
        }

        private void OnDestroy()
        {
            StaticManager.KillCoroutine(ref _initialisationCoroutine);
            Plugin.Configuration.SettingChanged -= UpdateOutlineSettings;
            Destroy(_maskMaterial);
            Destroy(_fillMaterial);
        }

        private bool IsInitialised()
        {
            return _isInitialised;
        }

        private IEnumerator DoAfterInitialisation(Action action)
        {
            yield return new WaitUntil(IsInitialised);

            action();
        }

        // Retrieve or generate smooth normals
        private async Task LoadSmoothNormalsAsync()
        {
            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                // Skip if mesh is unreadable or smooth normals have already been adopted
                if (!meshFilter.sharedMesh.isReadable || !RegisteredMeshes.Add(meshFilter.sharedMesh))
                {
                    continue;
                }

                // Retrieve or generate smooth normals
                var smoothNormals = await SmoothNormalsAsync(meshFilter.sharedMesh);

                // Store smooth normals in UV3
                meshFilter.sharedMesh.SetUVs(3, smoothNormals);

                // Combine submeshes
                var renderer = meshFilter.GetComponent<Renderer>();

                if (renderer != null)
                {
                    CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
                }
            }

            // Clear UV3 on skinned mesh renderers
            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                // Skip if mesh is unreadable or UV3 has already been reset
                if (!skinnedMeshRenderer.sharedMesh.isReadable || !RegisteredMeshes.Add(skinnedMeshRenderer.sharedMesh))
                {
                    continue;
                }

                // Clear UV3
                skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

                // Combine submeshes
                CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
            }
        }

        private async Task<List<Vector3>> SmoothNormalsAsync(Mesh mesh)
        {
            // Group vertices by location
            var groups = mesh.vertices
                .Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index))
                .GroupBy(pair => pair.Key);

            // Copy normals to a new list
            var smoothNormals = new List<Vector3>(mesh.normals);

            // Average normals for grouped vertices
            await Task.Run(() =>
            {
                foreach (var group in groups)
                {
                    // Skip single vertices
                    if (group.Count() == 1)
                    {
                        continue;
                    }

                    // Calculate the average normal
                    var smoothNormal = Vector3.zero;

                    foreach (var pair in group)
                    {
                        smoothNormal += smoothNormals[pair.Value];
                    }

                    smoothNormal.Normalize();

                    // Assign smooth normal to each vertex
                    foreach (var pair in group)
                    {
                        smoothNormals[pair.Value] = smoothNormal;
                    }
                }
            });

            return smoothNormals;
        }

        private void CombineSubmeshes(Mesh mesh, Material[] materials)
        {
            // Skip meshes with a single submesh
            if (mesh.subMeshCount == 1)
            {
                return;
            }

            // Skip if submesh count exceeds material count
            if (mesh.subMeshCount > materials.Length)
            {
                return;
            }

            // Append combined submesh
            mesh.subMeshCount++;
            mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
        }

        private void UpdateOutlineSettings()
        {
            _fillMaterial.SetColor(colorId, Plugin.OutlineColour.Value);
            _fillMaterial.SetFloat(widthId, Plugin.OutlineWidth.Value);
        }

        private void UpdateOutlineSettings(object sender, SettingChangedEventArgs e)
        {
            if (e.ChangedSetting.Definition.Section == Plugin.SECTION_INTERACTABLES)
            {
                UpdateOutlineSettings();
            }
        }
    }
}
