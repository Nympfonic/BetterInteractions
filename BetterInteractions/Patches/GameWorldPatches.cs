using Aki.Reflection.Patching;
using Arys.BetterInteractions.Components;
using Arys.BetterInteractions.Helper;
using Arys.BetterInteractions.Helper.Debug;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;

namespace Arys.BetterInteractions.Patches
{
    internal class GameWorldPatches
    {
        // GameWorld.RegisterLoot
        internal class AddOutlines : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(
                    typeof(GameWorld),
                    nameof(GameWorld.RegisterLoot),
                    generics: [typeof(InteractableObject)]
                );
            }

            [PatchPostfix]
            private static void PatchPostfix(object loot)
            {
                // Add BetterInteractionsOutline component to mod-enabled lootable objects
                if (InteractionsHelper.IsEnabledInteractable(loot as InteractableObject))
                {
                    (loot as InteractableObject).gameObject.AddComponent<BetterInteractionsOutline>();
                }
            }
        }

        // GameWorld.OnGameStarted
        internal class AddPhysicsToDoors : ModulePatch
        {
            private static readonly FieldInfo _worldInteractiveObjectsField = AccessTools.DeclaredField(typeof(GameWorld), "worldInteractiveObject_0");

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
            }

            [PatchPostfix]
            private static void PatchPostfix(GameWorld __instance)
            {
                var allWorldInteractives = _worldInteractiveObjectsField.GetValue(__instance) as WorldInteractiveObject[];

                for (int i = allWorldInteractives.Length - 1; i >= 0; i--)
                {
                    var interactive = allWorldInteractives[i];

                    if (
                        interactive is Door door
                        && interactive is not DoorSwitch
                        && interactive is not SlidingDoor
                    )
                    {
                        var component = door.gameObject.AddComponent<BetterInteractionsPhysicsDoor>();
                        Plugin.CachedPhysicsDoors.Add(component);
                    }
                }
            }
        }

        // GameWorld.Dispose
        internal class ClearStatics : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(typeof(GameWorld), nameof(GameWorld.Dispose));
            }

            [PatchPrefix]
            private static void PrefixPatch()
            {
                Plugin.CachedOutlineComponent = null;
                Plugin.CachedPhysicsDoors.Clear();
                BetterInteractionsOutline.RegisteredMeshes.Clear();
                GizmoHelper.DestroyGizmo();
            }
        }
    }
}
