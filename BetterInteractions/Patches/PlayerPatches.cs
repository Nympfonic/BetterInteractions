using Aki.Reflection.Patching;
using Arys.BetterInteractions.Commands;
using Arys.BetterInteractions.Components;
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
    internal class PlayerPatches
    {
        internal class CustomInteractionCheck : ModulePatch
        {
            private readonly static int _gameWorldLayer1 = (int)AccessTools.DeclaredField(typeof(GameWorld), "int_1").GetValue(null);
            private readonly static int _gameWorldLayer3 = (int)AccessTools.DeclaredField(typeof(GameWorld), "int_3").GetValue(null);

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(typeof(Player), nameof(Player.InteractionRaycast));
            }

            [PatchPostfix]
            private static void PatchPostfix(Player __instance)
            {
                if (!__instance.IsYourPlayer || BetterInteractionsPlugin.OutlineController == null)
                {
                    return;
                }

                InteractableObject interactable = __instance.InteractableObject;

                // Disable cached interactable outline if no results
                if (interactable == null)
                {
                    BetterInteractionsPlugin.OutlineController.UndoCommand();
                }
                // Already have a result so we only need to enable outline
                else if (interactable.IsOutlineEnabled())
                {
                    // TODO:
                    // Adding the outline component to a Corpse can end up applying the outline effect to other interactables
                    // that are child game objects of the Corpse which can lead to catastrophic results.
                    // Fix!!

                    BetterInteractionsOutline component = interactable.GetOrAddOutline();
                    var command = new ToggleOutlineCommand(component);
                    BetterInteractionsPlugin.OutlineController.AddCommand(command);

                    //return;
                }

                //Ray ray = __instance.InteractionRay;

                // Do custom raycast check if no results
//                if (
//                    Physics.Raycast(ray, out RaycastHit hit, __instance.RayLength, _gameWorldLayer1)
//                    && Physics.OverlapSphereNonAlloc(
//                        hit.point,
//                        Plugin.InteractableSphereRadius.Value,
//                        Plugin.CachedDetectedColliders,
//                        _gameWorldLayer1
//                    ) > 0
//                )
//                {
//                    InteractableObject nearestInteractable = null;

//                    for (int i = 0; i < Plugin.CachedDetectedColliders.Length; i++)
//                    {
//                        Collider detectedCollider = Plugin.CachedDetectedColliders[i];

//                        if (detectedCollider == null)
//                        {
//                            continue;
//                        }

//                        InteractableObject detectedInteractable = detectedCollider.GetComponentInParent<InteractableObject>();

//                        if (detectedInteractable == null || !InteractionsHelper.IsEnabledInteractable(detectedInteractable))
//                        {
//                            continue;
//                        }

//                        if (nearestInteractable == null)
//                        {
//                            nearestInteractable = detectedInteractable;
//                            continue;
//                        }

//                        // if detectedInteractable is closer than nearestInteractable, update nearestInteractable
//                        if ((detectedInteractable.transform.position - hit.point).sqrMagnitude < (nearestInteractable.transform.position - hit.point).sqrMagnitude)
//                        {
//                            nearestInteractable = detectedInteractable;
//                        }
//                    }

//                    if (nearestInteractable == null || Physics.Linecast(ray.origin, nearestInteractable.transform.position, _gameWorldLayer3))
//                    {
//                        return;
//                    }

//                    Plugin.CachedOutlineComponent = nearestInteractable.GetAddComponent<BetterInteractionsOutline>();
//                    Plugin.CachedOutlineComponent.ToggleOutline(true);
//                }

//#if DEBUG
//                switch (Plugin.DebugRaycast.Value)
//                {
//                    case GizmoMode.RaycastHit:
//                        GizmoHelper.DrawGizmo(hit.point, 0.01f);
//                        break;
//                    case GizmoMode.OverlapSphere:
//                        GizmoHelper.DrawGizmo(hit.point, Plugin.InteractableSphereRadius.Value);
//                        break;
//                }
//#endif
            }
        }
    }
}
