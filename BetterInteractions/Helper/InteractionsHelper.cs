using EFT;
using EFT.Interactive;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace Arys.BetterInteractions.Helper
{
    internal static class InteractionsHelper
    {
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

        internal static T GetAddComponent<T>(this MonoBehaviour monoBehaviour)
            where T : Component
        {
            if (monoBehaviour.TryGetComponent<T>(out var component))
            {
                return component;
            }
            else
            {
                return monoBehaviour.gameObject.AddComponent<T>();
            }
        }

        private static readonly MethodInfo _executeWorldInteractionMethod = AccessTools.FirstMethod(typeof(Player), mi =>
        {
            ParameterInfo[] parameters = mi.GetParameters();

            return parameters.Length == 2
                && parameters[0].ParameterType == typeof(WorldInteractiveObject)
                && parameters[1].ParameterType == typeof(InteractionResult);
        });
    }
}
