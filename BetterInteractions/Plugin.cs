#if DEBUG
using Arys.BetterInteractions.Helper.Debug;
#endif
using Arys.BetterInteractions.Patches;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Arys.BetterInteractions
{
    [BepInPlugin("com.Arys.BetterInteractions", "Arys' Better Interactions", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
#if DEBUG
        internal const string SECTION_DEBUG = "Debugging";
#endif
        internal const string SECTION_INTERACTABLES = "Interactables";

        internal static string Directory;
        internal static ManualLogSource LogSource;
        internal static ConfigFile Configuration;

        internal static Shader LootItemMaskShader;
        internal static Shader LootItemFillShader;

        internal static Collider[] CachedDetectedColliders = null;
        internal static BetterInteractionsOutline CachedOutlineComponent = null;

#if DEBUG
        // Debug
        internal static ConfigEntry<GizmoMode> DebugRaycast;
#endif
        // Item interactions
        internal static ConfigEntry<float> InteractableSphereRadius;
        // Outline
        internal static ConfigEntry<Color> InteractableOutlineColour;
        internal static ConfigEntry<float> InteractableOutlineWidth;
        // Outline toggles
        internal static ConfigEntry<bool> LootItemOutlineEnabled;
        internal static ConfigEntry<bool> LootContainerOutlineEnabled;
        internal static ConfigEntry<bool> SwitchOutlineEnabled;
        internal static ConfigEntry<bool> DoorOutlineEnabled;

        private void Awake()
        {
            Configuration = Config;
            LogSource = Logger;
            Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string shadersPath = $"{Directory}\\arys_betterinteractions_shaders";
            LootItemMaskShader = LoadShader("assets/shaders/betterinteractions_outlinemask.shader", shadersPath);
            LootItemFillShader = LoadShader("assets/shaders/betterinteractions_outlinefill.shader", shadersPath);

#if DEBUG
            DebugRaycast = Config.Bind(
                SECTION_DEBUG,
                "Debug Raycasts",
                GizmoMode.Off
            );
#endif

            InteractableSphereRadius = Config.Bind(
                SECTION_INTERACTABLES,
                "Sphere Detection Radius",
                0.2f,
                new ConfigDescription(
                    "The radius of the sphere which detects interactables",
                    new AcceptableValueRange<float>(0.05f, 0.2f)
                )
            );

            InteractableOutlineColour = Config.Bind(
                SECTION_INTERACTABLES,
                "Outline Color",
                Color.white,
                new ConfigDescription("The outline color of the interactable you're looking at")
            );

            InteractableOutlineWidth = Config.Bind(
                SECTION_INTERACTABLES,
                "Outline Width",
                2f,
                new ConfigDescription(
                    "The outline width of the interactable you're looking at",
                    new AcceptableValueRange<float>(0.1f, 2f)
                )
            );

            LootItemOutlineEnabled = Config.Bind(
                SECTION_INTERACTABLES,
                "Enabled for Loot Items",
                true,
                new ConfigDescription("Loot Items are loose loot you can find in the world")
            );

            LootContainerOutlineEnabled = Config.Bind(
                SECTION_INTERACTABLES,
                "Enabled for Loot Containers",
                true,
                new ConfigDescription("Loot Containers are any lootable containers")
            );

            SwitchOutlineEnabled = Config.Bind(
                SECTION_INTERACTABLES,
                "Enabled for Switches",
                true,
                new ConfigDescription("Switches are any switches/levers you can flip, like the power switch on Reserve")
            );

            DoorOutlineEnabled = Config.Bind(
                SECTION_INTERACTABLES,
                "Enabled for Doors",
                true,
                new ConfigDescription("Self-explanatory; Doors are doors, they scare DrakiaXYZ")
            );

            new GameWorldRegisterLootPatch().Enable();
            new PlayerInteractionRaycastPatch().Enable();
            new PlayerDisposePatch().Enable();
        }

        private static Shader LoadShader(string shaderName, string bundlePath)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            Shader shader = assetBundle.LoadAsset<Shader>(shaderName);
            assetBundle.Unload(false);
            return shader;
        }
    }
}
