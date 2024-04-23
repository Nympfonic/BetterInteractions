using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace Arys.BetterInteractions.Patches
{
    internal class PlayerDisposePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Dispose));
        }

        [PatchPrefix]
        private static void PatchPrefix(Player __instance)
        {
            if (__instance.IsYourPlayer)
            {
                Plugin.CachedOutlineComponent = null;
                Plugin.CachedDetectedColliders = null;
            }
        }
    }
}
