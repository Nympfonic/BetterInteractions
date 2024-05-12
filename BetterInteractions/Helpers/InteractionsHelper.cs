using Arys.BetterInteractions.Components;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Arys.BetterInteractions.Helpers
{
    internal static class InteractionsHelper
    {
        internal static BetterInteractionsOutline GetOrAddOutline(this InteractableObject interactable)
        {
            BetterInteractionsOutline component;

            if (interactable is Corpse corpse)
            {
                Transform playerMeshTransform = corpse.GetPlayerBody().GetMeshTransform();

                //for (int i = playerMeshTransform.childCount - 1; i >= 0; i--)
                //{
                //    playerMeshTransform.GetChild(i).GetAddComponent<BetterInteractionsOutline>();
                //}

                component = playerMeshTransform.GetAddComponent<BetterInteractionsOutline>();
            }
            else
            {
                if (interactable.transform.parent != null)
                {
                    component = interactable.transform.parent.GetAddComponent<BetterInteractionsOutline>();
                }
                else
                {
                    component = interactable.GetAddComponent<BetterInteractionsOutline>();
                }
            }

            return component;
        }

        internal static bool IsOutlineEnabled(this InteractableObject interactable)
        {
            return (BetterInteractionsPlugin.LootItemOutlineEnabled.Value && interactable is LootItem)
                || (BetterInteractionsPlugin.LootContainerOutlineEnabled.Value && (interactable is LootableContainer || interactable is Trunk))
                || (BetterInteractionsPlugin.SwitchOutlineEnabled.Value && interactable is Switch)
                || (BetterInteractionsPlugin.DoorOutlineEnabled.Value && interactable is Door);
        }

        internal static void ExecuteWorldInteraction(this Player player, WorldInteractiveObject interactiveObject, InteractionResult interactionResult)
        {
            _executeWorldInteractionMethod.Invoke(player, [ interactiveObject, interactionResult ]);
        }

        internal static T GetAddComponent<T>(this Component component)
            where T : Component
        {
            if (component.TryGetComponent<T>(out var targetComponent))
            {
                return targetComponent;
            }
            else
            {
                return component.gameObject.AddComponent<T>();
            }
        }

        internal static PlayerBody GetPlayerBody(this Corpse corpse)
        {
            return _corpsePlayerBodyField.GetValue(corpse) as PlayerBody;
        }

        internal static Transform GetMeshTransform(this PlayerBody playerBody)
        {
            return _playerBodyMeshTransformField.GetValue(playerBody) as Transform;
        }

        private static readonly MethodInfo _executeWorldInteractionMethod = AccessTools.FirstMethod(typeof(Player), mi =>
        {
            ParameterInfo[] parameters = mi.GetParameters();

            return parameters.Length == 2
                && parameters[0].ParameterType == typeof(WorldInteractiveObject)
                && parameters[1].ParameterType == typeof(InteractionResult);
        });

        private static readonly FieldInfo _corpsePlayerBodyField = AccessTools.Field(typeof(Corpse), "PlayerBody");
        private static readonly FieldInfo _playerBodyMeshTransformField = AccessTools.Field(typeof(PlayerBody), "_meshTransform");
    }
}
