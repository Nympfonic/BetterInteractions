using Aki.Reflection.Patching;
using Arys.BetterInteractions.Components;
using Arys.BetterInteractions.Controllers;
using Arys.BetterInteractions.Helpers;
using Arys.BetterInteractions.Helpers.Debug;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;

namespace Arys.BetterInteractions.Patches
{
    internal class GameWorldPatches
    {
        // GameWorld.RegisterLoot
        internal class AddOutlineToRegisteredLootable : ModulePatch
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
                // When players/bots are killed, they are turned into corpses and are registered through this patched method

                var interactable = loot as InteractableObject;

                if (interactable.IsOutlineEnabled())
                {
                    interactable.GetOrAddOutline();
                }
            }
        }

        // GameWorld.OnGameStarted
        internal class InitialiseComponents : ModulePatch
        {
            private static readonly FieldInfo _worldInteractiveObjectsField = AccessTools.DeclaredField(typeof(World), "worldInteractiveObject_0");
            private static readonly FieldInfo _handleField = AccessTools.DeclaredField(typeof(WorldInteractiveObject), "_handle");

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.DeclaredMethod(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
            }

            [PatchPostfix]
            private static void PatchPostfix(GameWorld __instance)
            {
                BetterInteractionsPlugin.OutlineController = new OutlineController();

                var allWorldInteractives = _worldInteractiveObjectsField.GetValue(__instance.World_0) as WorldInteractiveObject[];

                for (int i = allWorldInteractives.Length - 1; i >= 0; i--)
                {
                    WorldInteractiveObject interactive = allWorldInteractives[i];

                    // Add outline to all baked world interactive objects
                    if (interactive.transform.parent != null)
                    {
                        interactive.transform.parent.gameObject.AddComponent<BetterInteractionsOutline>();
                    }
                    else
                    {
                        interactive.gameObject.AddComponent<BetterInteractionsOutline>();
                    }

                    if (interactive.IsPhysicsEnabledDoor())
                    {
                        // Add physics to normal doors only
                        var component = interactive.gameObject.AddComponent<BetterInteractionsPhysicsDoor>();
                        BetterInteractionsPlugin.CachedPhysicsDoors.Add(component);
                    }
                    
                    // Door handles are not attached to Door game objects so we get the reference via the _handle field
                    var handle = _handleField.GetValue(interactive) as DoorHandle;
                    if (handle != null)
                    {
                        // TODO:
                        // Adding the outline component to the handle doesn't solve the issue of still needing to be toggled
                        // Toggle outline if the parent door outline is also toggled

                        // Add outline to door handles
                        handle.gameObject.AddComponent<BetterInteractionsOutline>();
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
                BetterInteractionsPlugin.OutlineController.ClearCommandList();
                BetterInteractionsPlugin.OutlineController = null;
                BetterInteractionsPlugin.CachedPhysicsDoors.Clear();
                BetterInteractionsOutline.RegisteredMeshes.Clear();
                GizmoHelper.DestroyGizmo();
            }
        }
    }
}
