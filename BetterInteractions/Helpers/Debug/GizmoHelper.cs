using UnityEngine;

namespace Arys.BetterInteractions.Helpers.Debug
{
    internal enum GizmoMode
    {
        Off,
        RaycastHit,
        OverlapSphere
    }

    internal static class GizmoHelper
    {
        private static GameObject _instance;

        internal static void DrawGizmo(Vector3 position, float scale)
        {
            if (_instance is null)
            {
                _instance = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                Collider collider = _instance.GetComponent<Collider>();
                collider.enabled = false;

                Material material = _instance.GetComponent<Renderer>().material;
                material.color = Color.red;
            }

            if (scale <= 0f)
            {
                scale = 0.01f;
            }

            _instance.transform.localScale = scale * Vector3.one;
            _instance.transform.position = position;
        }

        internal static void DestroyGizmo()
        {
            if (_instance is not null)
            {
                Object.Destroy(_instance);
            }
            _instance = null;
        }
    }
}
