// Thanks to Chris Nolet for his QuickOutline project
// Source Repository: https://github.com/chrisnolet/QuickOutline

using System.Collections.Generic;
using System.Linq;
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

        private Renderer[] renderers;
        private Material maskMaterial;
        private Material fillMaterial;

        public void EnableOutline()
        {
            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials.ToList();
                materials.Add(maskMaterial);
                materials.Add(fillMaterial);
                renderer.materials = [.. materials];
            }
        }

        public void DisableOutline()
        {
            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials.ToList();
                materials.Remove(maskMaterial);
                materials.Remove(fillMaterial);
                renderer.materials = [.. materials];
            }
        }

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();

            maskMaterial = new Material(Plugin.LootItemMaskShader);
            fillMaterial = new Material(Plugin.LootItemFillShader);

            maskMaterial.SetFloat(zTestId, (float)CompareFunction.Always);
            fillMaterial.SetFloat(zTestId, (float)CompareFunction.LessEqual);

            LoadSmoothNormals();

            Plugin.Configuration.SettingChanged += UpdateOutlineSettings;

            UpdateOutlineSettings();
        }

        private void OnDestroy()
        {
            Plugin.Configuration.SettingChanged -= UpdateOutlineSettings;
            Destroy(maskMaterial);
            Destroy(fillMaterial);
        }

        // Retrieve or generate smooth normals
        private void LoadSmoothNormals()
        {
            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                // Skip if mesh is unreadable or smooth normals have already been adopted
                if (!meshFilter.sharedMesh.isReadable || !RegisteredMeshes.Add(meshFilter.sharedMesh))
                {
                    continue;
                }

                // Retrieve or generate smooth normals
                var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

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

        private List<Vector3> SmoothNormals(Mesh mesh)
        {
            // Group vertices by location
            var groups = mesh.vertices
                .Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index))
                .GroupBy(pair => pair.Key);

            // Copy normals to a new list
            var smoothNormals = new List<Vector3>(mesh.normals);

            // Average normals for grouped vertices
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
            fillMaterial.SetColor(colorId, Plugin.InteractableOutlineColour.Value);
            fillMaterial.SetFloat(widthId, Plugin.InteractableOutlineWidth.Value);
        }

        private void UpdateOutlineSettings(object sender, BepInEx.Configuration.SettingChangedEventArgs e)
        {
            if (e.ChangedSetting.Definition.Section == Plugin.SECTION_INTERACTABLES)
            {
                UpdateOutlineSettings();
            }
        }
    }
}
