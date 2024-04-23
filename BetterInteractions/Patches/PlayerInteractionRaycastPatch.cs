using Aki.Reflection.Patching;
using Arys.BetterInteractions.Helper;
#if DEBUG
using Arys.BetterInteractions.Helper.Debug;
#endif
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Arys.BetterInteractions.Patches
{
    internal class PlayerInteractionRaycastPatch : ModulePatch
    {
        private static int _gameWorldLayer1;
        private static int _gameWorldLayer3;

        protected override MethodBase GetTargetMethod()
        {
            _gameWorldLayer1 = (int)AccessTools.DeclaredField(typeof(GameWorld), "int_1").GetValue(null);
            _gameWorldLayer3 = (int)AccessTools.DeclaredField(typeof(GameWorld), "int_3").GetValue(null);

            return AccessTools.DeclaredMethod(typeof(Player), nameof(Player.InteractionRaycast));
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {
            if (!__instance.IsYourPlayer)
            {
                return;
            }

            InteractableObject interactable = __instance.InteractableObject;

            // Already have a result so we only need to enable outline
            if (interactable is not null && InteractionsHelper.IsEnabledInteractable(interactable))
            {
                var component = interactable.GetAddComponent<BetterInteractionsOutline>();

                // Disable cached component outline if it is not the current one
                if (Plugin.CachedOutlineComponent is not null && Plugin.CachedOutlineComponent != component)
                {
                    Plugin.CachedOutlineComponent.DisableOutline();
                }

                Plugin.CachedOutlineComponent = component;
                Plugin.CachedOutlineComponent.EnableOutline();

                return;
            }

            // Disable cached interactable outline if no results
            if (interactable is null && Plugin.CachedOutlineComponent is not null)
            {
                Plugin.CachedOutlineComponent.DisableOutline();
                Plugin.CachedOutlineComponent = null;
            }

            Ray ray = __instance.InteractionRay;

            Plugin.CachedDetectedColliders ??= new Collider[30];

            // Do custom raycast check if no results
            if (
                Physics.Raycast(ray, out RaycastHit hit, __instance.RayLength, _gameWorldLayer1)
                && Physics.OverlapSphereNonAlloc(
                    hit.point,
                    Plugin.InteractableSphereRadius.Value,
                    Plugin.CachedDetectedColliders,
                    _gameWorldLayer1
                ) > 0
            )
            {
                InteractableObject nearestInteractable = null;

                for (int i = 0; i < Plugin.CachedDetectedColliders.Length; i++)
                {
                    Collider detectedCollider = Plugin.CachedDetectedColliders[i];

                    if (detectedCollider is null)
                    {
                        continue;
                    }

                    InteractableObject detectedInteractable = detectedCollider.GetComponentInParent<InteractableObject>();

                    if (detectedInteractable is null || !InteractionsHelper.IsEnabledInteractable(detectedInteractable))
                    {
                        continue;
                    }

                    if (nearestInteractable is null)
                    {
                        nearestInteractable = detectedInteractable;
                        continue;
                    }

                    // if detectedInteractable is closer than nearestInteractable, update nearestInteractable
                    if ((detectedInteractable.transform.position - hit.point).sqrMagnitude < (nearestInteractable.transform.position - hit.point).sqrMagnitude)
                    {
                        nearestInteractable = detectedInteractable;
                    }
                }

                if (nearestInteractable is null || Physics.Linecast(ray.origin, nearestInteractable.transform.position, _gameWorldLayer3))
                {
                    return;
                }

                Plugin.CachedOutlineComponent = nearestInteractable.GetAddComponent<BetterInteractionsOutline>();
                Plugin.CachedOutlineComponent.EnableOutline();
            }

#if DEBUG
            switch (Plugin.DebugRaycast.Value)
            {
                case GizmoMode.RaycastHit:
                    GizmoHelper.DrawGizmo(hit.point, 0.01f);
                    break;
                case GizmoMode.OverlapSphere:
                    GizmoHelper.DrawGizmo(hit.point, Plugin.InteractableSphereRadius.Value);
                    break;
            }
#endif
        }
    }
}
