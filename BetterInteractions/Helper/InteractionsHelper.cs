using EFT.Interactive;
using UnityEngine;

namespace Arys.BetterInteractions.Helper
{
    internal static class InteractionsHelper
    {
        internal static bool IsEnabledInteractable(InteractableObject interactable)
        {
            return (Plugin.LootItemOutlineEnabled.Value && interactable is LootItem)
                || (Plugin.LootContainerOutlineEnabled.Value && interactable is LootableContainer)
                || (Plugin.SwitchOutlineEnabled.Value && interactable is Switch)
                || (Plugin.DoorOutlineEnabled.Value && interactable is Door);
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
    }
}
