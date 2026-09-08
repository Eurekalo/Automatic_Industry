// Copyright (c) 2026 Automatic Industry contributors. Licensed under the MIT License.
// Original mod concept: "AutoMachine" (Steam Workshop id 2992024030).

using System.Collections.Generic;
using AutoMachineRebuilt.Components;
using AutoMachineRebuilt.Config;
using AutoMachineRebuilt.Util;
using HarmonyLib;
using UnityEngine;

namespace AutoMachineRebuilt.Patches
{
    /// <summary>
    /// Hooks vanilla decision points where buildings request Duplicant intervention
    /// (emptying solid output or flipping compost), replacing the Duplicant chore
    /// with direct state machine transitions and safe dropped item placement.
    ///
    /// When options are disabled, all patches return true and let vanilla
    /// generate normal Duplicant chores without any alteration.
    /// </summary>
    internal static class AutoEmptyTriggerPatches
    {
        /// <summary>
        /// Intercepts Gleaner (MilkSeparator) idle check for emptying.
        /// When automated, directly transitions the machine to emptyComplete,
        /// dropping its solid products (Caviar / Brackwax) without entering the full state.
        /// </summary>
        [HarmonyPatch(typeof(MilkSeparator), "RequiresEmptying")]
        internal static class MilkSeparatorRequiresEmptyingPatch
        {
            internal static void Postfix(MilkSeparator.Instance smi, ref bool __result)
            {
                if (!__result || smi == null || !AutoMachineOptions.IsEnabledFor(smi.gameObject, "MILKFATSEPARATOR"))
                {
                    return;
                }

                SafeInvoke.Try("automated Gleaner emptying", delegate
                {
                    smi.GoTo(smi.sm.operational.emptyComplete);
                    Log.Verbose("Automated Gleaner emptying: transitioned from idle to emptyComplete.");
                });

                // The machine is transitioning directly to emptyComplete, do not enter full state.
                __result = false;
            }
        }

        /// <summary>
        /// Safety net if Gleaner enters full state (e.g. from an existing save game).
        /// Transitions to emptyComplete and returns true so GameStateMachine.SetupChore
        /// receives a non-null Chore without throwing NullReferenceException.
        /// </summary>
        [HarmonyPatch(typeof(MilkSeparator), "CreateEmptyChore")]
        internal static class MilkSeparatorCreateEmptyChorePatch
        {
            internal static bool Prefix(MilkSeparator.Instance smi, ref Chore __result)
            {
                if (smi == null || !AutoMachineOptions.IsEnabledFor(smi.gameObject, "MILKFATSEPARATOR"))
                {
                    return true; // 100% vanilla when disabled!
                }

                SafeInvoke.Try("automated Gleaner emptying", delegate
                {
                    smi.GoTo(smi.sm.operational.emptyComplete);
                    Log.Verbose("Automated Gleaner emptying: transitioned full machine to emptyComplete.");
                });

                // Return true so vanilla creates a valid Chore object for SetupChore,
                // which gets immediately cleaned up as the state machine transitions to emptyComplete.
                return true;
            }
        }

        /// <summary>
        /// Replaces Gleaner's DropMilkFat to ensure dropped solid items (Caviar / Brackwax)
        /// are placed directly in the open air cell above the foundation tile,
        /// make them instantly visible and pickable without entombment or save/reload issues.
        /// </summary>
        [HarmonyPatch(typeof(MilkSeparator.Instance), "DropMilkFat")]
        internal static class MilkSeparatorDropMilkFatPatch
        {
            internal static bool Prefix(MilkSeparator.Instance __instance)
            {
                if (__instance == null || !AutoMachineOptions.IsEnabledFor(__instance.gameObject, "MILKFATSEPARATOR"))
                {
                    return true; // 100% vanilla when disabled!
                }

                SafeInvoke.Try("automated Gleaner DropMilkFat", delegate
                {
                    Storage storage = __instance.GetComponent<Storage>();
                    List<GameObject> list = new List<GameObject>();
                    if (storage != null)
                    {
                        storage.Drop(__instance.def.MILK_FAT_TAG, list);
                        storage.Drop(__instance.def.CAVIAR_TAG, list);
                    }

                    Vector3 buildingPos = __instance.transform.GetPosition();
                    int originCell = Grid.PosToCell(buildingPos);
                    int targetCell = originCell;

                    if (Grid.IsValidCell(originCell) && Grid.Solid[originCell])
                    {
                        int aboveCell = Grid.CellAbove(originCell);
                        if (Grid.IsValidCell(aboveCell) && !Grid.Solid[aboveCell])
                        {
                            targetCell = aboveCell;
                        }
                    }

                    Vector3 spawnPos = Grid.CellToPosCBC(targetCell, Grid.SceneLayer.Ore);

                    for (int i = 0; i < list.Count; i++)
                    {
                        GameObject item = list[i];
                        if (item == null) continue;

                        item.transform.SetPosition(spawnPos);

                        KBatchedAnimController kbac = item.GetComponent<KBatchedAnimController>();
                        if (kbac != null)
                        {
                            kbac.SetVisiblity(true);
                        }

                        Pickupable pickupable = item.GetComponent<Pickupable>();
                        if (pickupable != null)
                        {
                            pickupable.RemoveTag(GameTags.LiquidSource);
                            pickupable.targetWorkable = pickupable;
                        }
                    }

                    HardThresholdRelease.DropRemainingSolids(__instance.gameObject);
                    __instance.RefreshMeters();
                });

                return false;
            }
        }

        /// <summary>
        /// Prevents Duplicants from accepting or being summoned for the Compost flip chore
        /// when automated. Adds an Always-False precondition to the chore returned by CreateFlipChore.
        /// </summary>
        [HarmonyPatch(typeof(Compost.States), "CreateFlipChore")]
        internal static class CompostCreateFlipChorePatch
        {
            private static readonly Chore.Precondition DisabledByModPrecondition = new Chore.Precondition
            {
                id = "DisabledByAutoMachine",
                description = "Automated by AutoMachine",
                fn = (ref Chore.Precondition.Context context, object data) => false
            };

            internal static void Postfix(Compost.StatesInstance smi, ref Chore __result)
            {
                if (__result == null || smi == null || !AutoMachineOptions.IsEnabledFor(smi.gameObject, "COMPOST"))
                {
                    return; // 100% vanilla when disabled!
                }

                __result.AddPrecondition(DisabledByModPrecondition, null);
            }
        }

        /// <summary>
        /// Synchronizes Compost visual state with its contents on spawn/load.
        /// Ensures full compost piles never appear as empty boxes ("off" animation)
        /// when loaded from save files.
        /// </summary>
        [HarmonyPatch(typeof(Compost), "OnSpawn")]
        internal static class CompostOnSpawnPatch
        {
            internal static void Postfix(Compost __instance)
            {
                if (__instance == null || __instance.smi == null)
                {
                    return;
                }

                SafeInvoke.Try("synchronizing compost on spawn", delegate
                {
                    if (!__instance.smi.IsEmpty())
                    {
                        var kbac = __instance.GetComponent<KBatchedAnimController>();
                        if (__instance.smi.CanStartConverting())
                        {
                            if (kbac != null && (kbac.currentAnim == "off" || kbac.currentAnim == null))
                            {
                                kbac.Play("on", KAnim.PlayMode.Once);
                            }

                            StateMachine.BaseState curState = StateMachineUtil.CurrentState(__instance.smi);
                            if (curState == __instance.smi.sm.empty)
                            {
                                __instance.smi.GoTo(__instance.smi.sm.inert);
                            }
                        }
                        else
                        {
                            if (kbac != null && (kbac.currentAnim == "off" || kbac.currentAnim == null))
                            {
                                kbac.Play("idle_half", KAnim.PlayMode.Once);
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Ensures that when the composting state is entered, the visual anim
        /// is displaying the compost pile ("on") instead of an empty box ("off").
        /// </summary>
        [HarmonyPatch(typeof(Compost.States), "InitializeStates")]
        internal static class CompostInitializeStatesPatch
        {
            internal static void Postfix(Compost.States __instance)
            {
                __instance.composting.Enter("EnsureCompostingVisual", smi =>
                {
                    var kbac = smi.GetComponent<KBatchedAnimController>();
                    if (kbac != null && (kbac.currentAnim == "off" || kbac.currentAnim == "idle_half"))
                    {
                        kbac.Play("on", KAnim.PlayMode.Once);
                    }
                });
            }
        }

        /// <summary>
        /// Replaces the Desalinator's emptying chore with the automated
        /// release of its stored solids.
        /// </summary>
        [HarmonyPatch(typeof(Desalinator.StatesInstance), "CreateEmptyChore")]
        internal static class DesalinatorCreateEmptyChorePatch
        {
            /// <summary>Empties the building instead of asking a Duplicant.</summary>
            /// <param name="__instance">Desalinator state machine instance.</param>
            /// <returns><c>false</c> when the vanilla chore must be skipped.</returns>
            internal static bool Prefix(Desalinator.StatesInstance __instance)
            {
                if (__instance == null || !AutoMachineOptions.IsEnabledFor(__instance.gameObject, "DESALINATOR"))
                {
                    return true;
                }

                SafeInvoke.Try("automated Desalinator emptying", delegate
                {
                    int dropped = HardThresholdRelease.ForceReleaseDesalinator(__instance);
                    Log.Verbose("Automated Desalinator emptying at the vanilla trigger: " +
                                dropped + " item(s).");
                });

                return false;
            }
        }

        /// <summary>
        /// Fixes vanilla Desalinator state machine deadlock where on.waiting only listens to OnStorageChange.
        /// When existing liquid mass is already in storage (e.g. 20kg pipe buffer filled), entering on.waiting
        /// immediately transitions to on.working_pre, preventing the building from freezing.
        /// </summary>
        [HarmonyPatch(typeof(Desalinator.States), "InitializeStates")]
        internal static class DesalinatorStatesInitializeStatesPatch
        {
            internal static void Postfix(Desalinator.States __instance)
            {
                if (__instance == null || __instance.on == null || __instance.on.waiting == null)
                {
                    return;
                }

                __instance.on.waiting.Enter("CheckExistingMassOnWaiting", delegate(Desalinator.StatesInstance smi)
                {
                    if (smi != null && smi.master != null && HasConvertableMass(smi.gameObject))
                    {
                        smi.GoTo(__instance.on.working_pre);
                    }
                });
            }

            private static bool HasConvertableMass(GameObject go)
            {
                if (go == null) return false;
                ElementConverter[] converters = go.GetComponents<ElementConverter>();
                if (converters == null) return false;
                for (int i = 0; i < converters.Length; i++)
                {
                    if (converters[i] != null && converters[i].HasEnoughMassToStartConverting())
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Prevents Duplicants from accepting or being summoned for the Ice-E Fan use chore
        /// when automated. Adds an Always-False precondition to the chore returned by CreateUseChore.
        /// </summary>
        [HarmonyPatch(typeof(IceCooledFan.States), "CreateUseChore")]
        internal static class IceCooledFanCreateUseChorePatch
        {
            private static readonly Chore.Precondition DisabledByModPrecondition = new Chore.Precondition
            {
                id = "DisabledByAutoMachine",
                description = "Automated by AutoMachine",
                fn = (ref Chore.Precondition.Context context, object data) => false
            };

            internal static void Postfix(IceCooledFan.StatesInstance smi, ref Chore __result)
            {
                if (__result == null || smi == null || !AutoMachineOptions.IsEnabledFor(smi.gameObject, "ICECOOLEDFAN"))
                {
                    return;
                }

                __result.AddPrecondition(DisabledByModPrecondition, null);
            }
        }

        /// <summary>
        /// Prevents Duplicants from accepting or being summoned for the Manual Generator
        /// operate chore when automated. Adds an Always-False precondition to the chore.
        /// </summary>
        [HarmonyPatch(typeof(ManualGenerator), "EnergySim200ms")]
        internal static class ManualGeneratorEnergySim200msPatch
        {
            private static readonly Chore.Precondition DisabledByModPrecondition = new Chore.Precondition
            {
                id = "DisabledByAutoMachine",
                description = "Automated by AutoMachine",
                fn = (ref Chore.Precondition.Context context, object data) => false
            };

            internal static void Postfix(ManualGenerator __instance)
            {
                if (__instance == null || !AutoMachineOptions.IsEnabledFor(__instance.gameObject, "MANUALGENERATOR"))
                {
                    return; // 100% vanilla when disabled!
                }

                Chore chore = StateMachineUtil.Field(__instance, "chore") as Chore;
                if (chore != null)
                {
                    chore.AddPrecondition(DisabledByModPrecondition, null);
                }
            }
        }

        /// <summary>
        /// Enables Auto-Sweepers (Solid Transfer Arm) to convey bottled water,
        /// bottled liquids/gases, canisters, medicine, and elemental bottles, allowing direct automated supply
        /// from Bottle Fillers / Pitcher Pumps / floor bottles to the Super Computer (Advanced Research Center).
        /// System, minion, and living creature tags are explicitly excluded.
        /// </summary>
        [HarmonyPatch(typeof(Assets), "IsTagSolidTransferArmConveyable")]
        internal static class AssetsIsTagSolidTransferArmConveyablePatch
        {
            private static readonly HashSet<Tag> NonConveyableTags = new HashSet<Tag>
            {
                GameTags.BaseMinion,
                new Tag("Minion"),
                new Tag("BionicMinion"),
                GameTags.Creature,
                GameTags.CreatureBrain,
                GameTags.Dead,
                GameTags.Robot,
                GameTags.GeyserFeature
            };

            private static readonly HashSet<Tag> AllowedExtendedTags = new HashSet<Tag>
            {
                GameTags.Water,
                GameTags.AnyWater,
                GameTags.DirtyWater,
                GameTags.Liquid,
                GameTags.LiquidSource,
                GameTags.Gas,
                GameTags.GasSource,
                GameTags.Medicine
            };

            internal static void Postfix(Tag tag, ref bool __result)
            {
                if (__result || !tag.IsValid)
                {
                    return;
                }

                var options = AutoMachineOptions.Instance;
                if (options != null && !options.EnableAllAutomation &&
                    !options.UnmannedAdvancedResearchCenter &&
                    !options.UnmannedNuclearResearchCenter &&
                    !options.LiquidReservoirAutoSweeperFetch)
                {
                    return;
                }

                if (NonConveyableTags.Contains(tag))
                {
                    return;
                }

                if (AllowedExtendedTags.Contains(tag))
                {
                    __result = true;
                    return;
                }

                // Check if the tag represents an element (e.g. bottled water, bottled ethanol, oxygen canister)
                Element element = ElementLoader.GetElement(tag);
                if (element != null && (element.IsLiquid || element.IsGas))
                {
                    __result = true;
                }
            }
        }
    }
}