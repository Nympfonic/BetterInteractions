using Aki.Reflection.Utils;
using EFT.Interactive;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Arys.BetterInteractions.Helper
{
    internal static class PhysicsHelper
    {
        internal static void SupportRigidbody(Rigidbody rigidbody, float quality = 1f, object visibilityChecker = null)
        {
            _supportRigidbodyMethod.Invoke(null, [ rigidbody, quality, visibilityChecker ]);
        }

        internal static void UnsupportRigidbody(Rigidbody rigidbody)
        {
            _unsupportRigidbodyMethod.Invoke(null, [ rigidbody ]);
        }

        internal static bool IsPhysicsEnabledDoor(this WorldInteractiveObject interactable)
        {
            return BetterInteractionsPlugin.DoorPhysicsEnabled.Value
                && interactable is Door
                && interactable is not SlidingDoor
                && interactable is not DoorSwitch
                && interactable is not KeycardDoor;
        }

        private static readonly Type _physicsManagerType = PatchConstants.EftTypes.First(type => AccessTools.DeclaredMethod(type, "IgnoreCollision") is not null);
        private static readonly Type _physicsRigidBodyHandlerType = AccessTools.FirstInner(_physicsManagerType, type => AccessTools.DeclaredMethod(type, "SupportRigidbody") is not null);

        private static readonly MethodInfo _supportRigidbodyMethod = AccessTools.DeclaredMethod(_physicsRigidBodyHandlerType, "SupportRigidbody");
        private static readonly MethodInfo _unsupportRigidbodyMethod = AccessTools.DeclaredMethod(_physicsRigidBodyHandlerType, "UnsupportRigidbody");
    }
}
