using Aki.Reflection.Patching;
using Arys.BetterInteractions.Helper;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Arys.BetterInteractions.Patches
{
    internal class GetActionsClassPatches
    {
        internal class AddPeekAction : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.FirstMethod(typeof(GetActionsClass), IsTargetMethod);
            }

            private bool IsTargetMethod(MethodInfo mi)
            {
                ParameterInfo[] parameters = mi.GetParameters();

                return mi.ReturnType == typeof(ActionsReturnClass)
                    && parameters.Length == 2
                    && parameters[0].ParameterType == typeof(GamePlayerOwner)
                    && parameters[1].ParameterType == typeof(Door);
            }

            [PatchPostfix]
            private static void PatchPostfix(GamePlayerOwner owner, Door door, ref ActionsReturnClass __result)
            {
                List<ActionsTypesClass> actions = __result.Actions;

                // If door is shut, add Peek action
                if (door.DoorState == EDoorState.Shut)
                {
                    // For compatibility sake, if there is already a Peek action added by some other mod, then don't add Peek action
                    if (!actions.Exists(x => x.Name.ToLower().Contains("peek")))
                    {
                        int openActionId = actions.FindIndex(x => x.Name == "OpenDoor");
                        if (openActionId == -1)
                        {
                            BetterInteractionsPlugin.LogSource.LogError($"{nameof(openActionId)} is somehow -1 when it shouldn't be");
                        }

                        // Insert Peek action after OpenDoor action
                        actions.Insert(
                            openActionId + 1,
                            new ActionsTypesClass
                            {
                                Name = "Peek",
                                Action = () => OpenDoor(owner, door, PeekDoor),
                                Disabled = !door.Operatable
                            }
                        );
                    }
                }
                // If door is open but not fully, add Open Door action
                else if (door.DoorState == EDoorState.Open
                    && door.CurrentAngle != door.OpenAngle
                    && door.CurrentAngle != door.CloseAngle)
                {
                    actions.Insert(
                        0,
                        new ActionsTypesClass
                        {
                            Name = "OpenDoor",
                            Action = () => OpenDoor(owner, door, OpenDoorFully),
                            Disabled = !door.Operatable
                        }
                    );
                }
            }

            private static void OpenDoor(GamePlayerOwner owner, Door door, Action<GamePlayerOwner, Door, InteractionResult> action)
            {
                owner.Player.MovementContext.ResetCanUsePropState();

                var gstruct = Door.Interact(owner.Player, EInteractionType.Open);
                if (gstruct.Succeeded)
                {
                    action(owner, door, gstruct.Value);
                }
            }

            private static void PeekDoor(GamePlayerOwner owner, Door door, InteractionResult interactionResult)
            {
                // Change door open angle
                float prevAngle = door.OpenAngle;
                door.OpenAngle = Mathf.Sign(prevAngle) * 25f;
                // Execute interaction
                owner.Player.ExecuteWorldInteraction(door, interactionResult);
                // Reset open angle
                door.OpenAngle = prevAngle;
            }

            private static void OpenDoorFully(GamePlayerOwner owner, Door door, InteractionResult interactionResult)
            {
                // Door is already open, don't play door opening sound
                AudioClip[] openSounds = door.OpenSound;
                door.OpenSound = [];
                // Execute interaction
                owner.Player.ExecuteWorldInteraction(door, interactionResult);
                // Reset open sounds
                door.OpenSound = openSounds;
            }
        }
    }
}
