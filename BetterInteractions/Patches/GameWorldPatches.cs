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
                    var rigidbody = door.gameObject.AddComponent<Rigidbody>();
                    // Rigidbody needs to be added to Tarkov's managed rigidbodies otherwise they will not work
                    PhysicsHelper.SupportRigidbody(rigidbody, 0f);

                    door.gameObject.AddComponent<HingeJoint>();
                    door.gameObject.AddComponent<BetterInteractionsPhysicsDoor>();
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
