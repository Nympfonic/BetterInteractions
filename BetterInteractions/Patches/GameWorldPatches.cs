using Aki.Reflection.Patching;
using Arys.BetterInteractions.Components;
using Arys.BetterInteractions.Helper;
using Arys.BetterInteractions.Helper.Debug;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Arys.BetterInteractions.Patches
{
    internal class GameWorldPatches
    {
        internal class InitManager : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
            }

            [PatchPostfix]
            private static void PatchPostfix()
            {
                new GameObject("BetterInteractions Controller").AddComponent<BetterInteractionsManager>();
            }
        }

        internal class AddOutlines : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(
                    typeof(GameWorld),
                    nameof(GameWorld.RegisterLoot),
                    generics: [ typeof(InteractableObject) ]
                );
            }

            [PatchPostfix]
            private static void PatchPostfix(object loot)
            {
                // Add BetterInteractionsOutline component to mod-enabled interactive objects
                if (InteractionsHelper.IsEnabledInteractable(loot as InteractableObject))
                {
                    (loot as InteractableObject).gameObject.AddComponent<BetterInteractionsOutline>();
                }
            }
        }

        internal class AddPhysicsToDoors : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(typeof(GameWorld), nameof(GameWorld.RegisterWorldInteractionObject));
            }

            [PatchPostfix]
            private static void PatchPostfix(WorldInteractiveObject worldInteractiveObject)
            {
                if (
                    worldInteractiveObject is Door door
                    && worldInteractiveObject is not DoorSwitch
                    && worldInteractiveObject is not SlidingDoor
                )
                {
                    var component = door.gameObject.AddComponent<BetterInteractionsPhysicsDoor>();
                    BetterInteractionsManager.Instance.CachedPhysicsDoors.Add(component);
                }
            }
        }

        internal class ClearStatics : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(typeof(GameWorld), nameof(GameWorld.Dispose));
            }

            [PatchPrefix]
            private static void PrefixPatch()
            {
                BetterInteractionsOutline.RegisteredMeshes.Clear();
                GizmoHelper.DestroyGizmo();

                if (BetterInteractionsManager.Instance != null)
                {
                    Object.Destroy(BetterInteractionsManager.Instance);
                }
            }
        }
    }
}
