using Aki.Reflection.Patching;
using Arys.BetterInteractions.Helper.Debug;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace Arys.BetterInteractions.Patches
{
    internal class GameWorldDisposePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(GameWorld), nameof(GameWorld.Dispose));
        }

        [PatchPostfix]
        private static void PostfixPatch()
        {
            BetterInteractionsOutline.RegisteredMeshes.Clear();
            GizmoHelper.DestroyGizmo();
        }
    }
}
