using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;

namespace Arys.BetterInteractions.Patches
{
    internal class GameWorldRegisterLootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(GameWorld), nameof(GameWorld.RegisterLoot), generics: [typeof(InteractableObject)]);
        }

        [PatchPostfix]
        private static void PatchPostfix(object loot)
        {
            // Add BetterInteractionsOutline component to LootItem
            if (loot is LootItem lootItem)
            {
                lootItem.gameObject.AddComponent<BetterInteractionsOutline>();
            }
        }
    }
}
