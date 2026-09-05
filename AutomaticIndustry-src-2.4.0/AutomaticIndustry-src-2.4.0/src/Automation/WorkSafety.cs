// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AutoMachineRebuilt.Automation
{
    /// <summary>
    /// Result of the static audit of every <see cref="Workable"/> shipped with
    /// the game.
    ///
    /// The lists were produced by decompiling the current game assembly and
    /// checking whether <c>OnWorkTick</c> or <c>OnCompleteWork</c> dereference
    /// their <c>WorkerBase</c> argument. A workable listed here throws a
    /// <see cref="NullReferenceException"/> when it is driven without a
    /// Duplicant, so the automation must either use a timer instead of the
    /// vanilla tick, or refuse to complete the work at all and leave the job
    /// to a specialised controller.
    ///
    /// Derived types inherit the behaviour of their base class, therefore the
    /// lookups walk the whole inheritance chain.
    /// </summary>
    internal static class WorkSafety
    {
        /// <summary>Workables whose tick reads the worker.</summary>
        private static readonly HashSet<string> UnsafeTick = new HashSet<string>(StringComparer.Ordinal)
        {
            "AstronautTrainingCenter",
            "Clinic",
            "CommandModuleWorkable",
            "ComplexFabricatorWorkable",
            "Edible",
            "EnterableDock",
            "ExitableDock",
            "ManualGenerator",
            "MissionControlClusterWorkable",
            "MissionControlWorkable",
            "NewWorker",
            "RelaxationPoint",
            "Repairable",
            "ResearchCenter",
            "Shower",
            "Sleepable",
            "SocialGatheringPointWorkable",
            "SpiceGrinderWorkable",
            "WatchRoboDancerWorkable",
            "Work",
            "WorkerGunkRemover",
            "WorkerOilRefiller",
            "WorkerRecharger"
        };

        /// <summary>Workables whose completion reads the worker.</summary>
        private static readonly HashSet<string> UnsafeComplete = new HashSet<string>(StringComparer.Ordinal)
        {
            "ArcadeMachineWorkable",
            "Artable",
            "BeachChairWorkable",
            "Bottler",
            "BuildingHP",
            "Clinic",
            "Constructable",
            "Deconstructable",
            "DehydratedFoodPackage",
            "EnterableDock",
            "EspressoMachineWorkable",
            "ExitableDock",
            "GeneShuffler",
            "GetBalloonWorkable",
            "Harvestable",
            "HotTubWorkable",
            "IceKettleWorkable",
            "JuicerWorkable",
            "MassageTable",
            "MechanicalSurfboardWorkable",
            "MedicinalPillWorkable",
            "MessStation",
            "NewWorker",
            "OilChangerWorkableUse",
            "PartyPointWorkable",
            "PhonoboxWorkable",
            "Pickupable",
            "ResetSkillsStation",
            "ReturnSuitWorkable",
            "RoleStation",
            "SaunaWorkable",
            "Shower",
            "SocialGatheringPointWorkable",
            "SodaFountainWorkable",
            "SpiceGrinderWorkable",
            "StorageTileSwitchItemWorkable",
            "TelephoneCallerWorkable",
            "Tinkerable",
            "Toggleable",
            "ToiletWorkableUse",
            "VerticalWindTunnelWorkable",
            "WatchRoboDancerWorkable",
            "Work"
        };

        /// <summary>Whether the vanilla tick may be called with no worker.</summary>
        /// <param name="workable">Workable the automation wants to drive.</param>
        internal static bool CanTickWithoutWorker(Workable workable)
        {
            return workable != null && !Matches(workable.GetType(), UnsafeTick);
        }

        /// <summary>Whether the vanilla completion may run with no worker.</summary>
        /// <param name="workable">Workable the automation wants to complete.</param>
        internal static bool CanCompleteWithoutWorker(Workable workable)
        {
            return workable != null && !Matches(workable.GetType(), UnsafeComplete);
        }

        /// <summary>Checks a type and all of its base types against a list.</summary>
        /// <param name="type">Runtime type of the workable.</param>
        /// <param name="names">Audited type names.</param>
        private static bool Matches(Type type, HashSet<string> names)
        {
            for (Type current = type; current != null && current != typeof(object);
                 current = current.BaseType)
            {
                if (names.Contains(current.Name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
