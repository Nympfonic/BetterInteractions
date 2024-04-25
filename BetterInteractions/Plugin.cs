#if DEBUG
using Arys.BetterInteractions.Components;
using Arys.BetterInteractions.Helper.Debug;
#endif
using Arys.BetterInteractions.Patches;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
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
        internal const string SECTION_DOORS = "Door Physics";

        internal static string Directory;
        internal static ManualLogSource LogSource;
        internal static ConfigFile Configuration;

        internal static Shader LootItemMaskShader;
        internal static Shader LootItemFillShader;

        internal static BetterInteractionsOutline CachedOutlineComponent = null;
        internal static readonly Collider[] CachedDetectedColliders = new Collider[30];
        internal static readonly HashSet<BetterInteractionsPhysicsDoor> CachedPhysicsDoors = [];

        private readonly GameWorldPatches.AddPhysicsToDoors _addPhysicsToDoors = new();

#if DEBUG
        // Debug
        internal static ConfigEntry<GizmoMode> DebugRaycast;
#endif
        // Interactions
        internal static ConfigEntry<float> InteractableSphereRadius;
        // Outline
        internal static ConfigEntry<Color> InteractableOutlineColour;
        internal static ConfigEntry<float> InteractableOutlineWidth;
        // Outline toggles
        internal static ConfigEntry<bool> LootItemOutlineEnabled;
        internal static ConfigEntry<bool> LootContainerOutlineEnabled;
        internal static ConfigEntry<bool> SwitchOutlineEnabled;
        internal static ConfigEntry<bool> DoorOutlineEnabled;

        // Door Physics
        internal static ConfigEntry<bool> DoorPhysicsEnabled;

        private void Awake()
        {
            Configuration = Config;
            LogSource = Logger;
            Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string shadersPath = Path.Combine(Directory, "arys_betterinteractions_shaders");
            LootItemMaskShader = LoadShader("assets/shaders/betterinteractions_outlinemask.shader", shadersPath);
            LootItemFillShader = LoadShader("assets/shaders/betterinteractions_outlinefill.shader", shadersPath);

            InitConfigBindings();

            new GameWorldPatches.AddOutlines().Enable();
            _addPhysicsToDoors.Enable();
            new GameWorldPatches.ClearStatics().Enable();
            new PlayerPatches.CustomInteractionCheck().Enable();
            new GetActionsClassPatches.AddPeekAction().Enable();

            DoorPhysicsEnabled.SettingChanged += DoorPhysicsSettingChanged;
        }

        private void DoorPhysicsSettingChanged(object sender, EventArgs e)
        {
            if (Singleton<GameWorld>.Instantiated)
            {
                return;
            }

            if (DoorPhysicsEnabled.Value)
            {
                _addPhysicsToDoors.Enable();
            }
            else
            {
                _addPhysicsToDoors.Disable();
            }
        }

        private void InitConfigBindings()
        {
#if DEBUG
            DebugRaycast = Config.Bind(
                SECTION_DEBUG,
                "Debug Raycasts",
                GizmoMode.Off,
                new ConfigDescription(
                    "",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );
#endif

#region Interactables
            InteractableSphereRadius = Config.Bind(
                SECTION_INTERACTABLES,
                "Sphere Detection Radius",
                0.2f,
                new ConfigDescription(
                    "The radius of the sphere which detects interactables",
                    new AcceptableValueRange<float>(0.05f, 0.5f),
                    new ConfigurationManagerAttributes { Order = 1 }
                )
            );

            InteractableOutlineColour = Config.Bind(
                SECTION_INTERACTABLES,
                "Outline Color",
                Color.white,
                new ConfigDescription(
                    "The outline color of the interactable you're looking at",
                    null,
                    new ConfigurationManagerAttributes { Order = 2 }
                )
            );

            InteractableOutlineWidth = Config.Bind(
                SECTION_INTERACTABLES,
                "Outline Width",
                2f,
                new ConfigDescription(
                    "The outline width of the interactable you're looking at",
                    new AcceptableValueRange<float>(0.1f, 2f),
                    new ConfigurationManagerAttributes { Order = 3 }
                )
            );

            LootItemOutlineEnabled = Config.Bind(
                SECTION_INTERACTABLES,
                "Enabled for Loot Items",
                true,
                new ConfigDescription(
                    "Loot Items are loose loot and corpses",
                    null,
                    new ConfigurationManagerAttributes { Order = 4 }
                )
            );

            LootContainerOutlineEnabled = Config.Bind(
                SECTION_INTERACTABLES,
                "Enabled for Loot Containers",
                true,
                new ConfigDescription(
                    "Loot Containers are any lootable containers and car trunks",
                    null,
                    new ConfigurationManagerAttributes { Order = 5 }
                )
            );

            SwitchOutlineEnabled = Config.Bind(
                SECTION_INTERACTABLES,
                "Enabled for Switches",
                true,
                new ConfigDescription(
                    "Switches are any switches/levers you can flip, like the power switch on Reserve",
                    null,
                    new ConfigurationManagerAttributes { Order = 6 }
                )
            );

            DoorOutlineEnabled = Config.Bind(
                SECTION_INTERACTABLES,
                "Enabled for Doors",
                true,
                new ConfigDescription(
                    "Self-explanatory; Doors are doors, they scare DrakiaXYZ",
                    null,
                    new ConfigurationManagerAttributes { Order = 7 }
                )
            );
            #endregion

#region Doors
            DoorPhysicsEnabled = Config.Bind(
                SECTION_DOORS,
                "Enable Door Physics",
                true,
                new ConfigDescription(
                    "Door physics allows you to open doors",
                    null,
                    new ConfigurationManagerAttributes { Order = 1 }
                )
            );
#endregion
        }

        // Shaders are assets so they will persist through scene changes
        private static Shader LoadShader(string shaderName, string bundlePath)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            Shader shader = assetBundle.LoadAsset<Shader>(shaderName);
            assetBundle.Unload(false);
            return shader;
        }
    }
}
