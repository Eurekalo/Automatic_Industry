// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

namespace AutoMachineRebuilt.Automation
{
    /// <summary>
    /// Vanilla mechanism a building uses to do its manual work.
    ///
    /// The automation never treats two different mechanisms the same way: a
    /// generic "tick every workable" driver was the root cause of the crashes
    /// reported for 2.0.7 (telescopes without a target, manual generators
    /// dereferencing the worker). Each value below maps to exactly one
    /// controller with hand audited entry and exit conditions.
    /// </summary>
    internal enum AutomationMechanism
    {
        /// <summary>
        /// Plain <see cref="Workable"/>: advance the vanilla work and complete
        /// it. Used only for workables whose tick and completion were audited
        /// as safe without a Duplicant.
        /// </summary>
        Workable,

        /// <summary>Telescope family; requires a valid analysis target.</summary>
        Telescope,

        /// <summary>Mission Control family; requires an assigned spacecraft.</summary>
        MissionControl,

        /// <summary>
        /// Research buildings; progress is produced by the element converter
        /// while the building is active, not by the work tick.
        /// </summary>
        Research,

        /// <summary>Manual Generator; driven by the circuit battery demand.</summary>
        ManualPower,

        /// <summary>
        /// Tinker stations (Farm Station, Power Station); the vanilla
        /// completion reads Duplicant attributes and is replaced by a safe
        /// equivalent without the Duplicant bonus.
        /// </summary>
        Tinker,

        /// <summary>
        /// Complex fabricators: the base game already runs them unattended
        /// once <c>duplicantOperated</c> is cleared, so the controller only
        /// flips that flag and restores it when the option is switched off.
        /// </summary>
        Fabricator,

        /// <summary>
        /// Buildings that produce on their own but need a Duplicant to carry
        /// the finished product out (Smoker, Dehydrator, Desalinator, Ice
        /// Liquefier, Gleaner). The controller performs exactly the drop the
        /// vanilla empty chore performs.
        /// </summary>
        Release,

        /// <summary>
        /// Buildings automated purely via state machine / chore creation hooks
        /// without requiring a ticking controller component (e.g. Compost, Gleaner).
        /// </summary>
        ChoreHook,

        /// <summary>
        /// Spice Grinder: a standalone <see cref="GameStateMachine"/> with its
        /// own <c>SpiceGrinderWorkable</c>. Not a ComplexFabricator — the
        /// controller drives the vanilla state machine directly.
        /// </summary>
        SpiceGrinder,

        /// <summary>
        /// Ice-E Fan: continuous cooling station driven directly without Duplicants.
        /// </summary>
        IceCooledFan,

        /// <summary>
        /// Materials Study Terminal: consumes Radbolts to conduct nuclear research.
        /// </summary>
        NuclearResearch,

        /// <summary>
        /// Botanical Analyzer: identifies mutant plant seeds with sample validation.
        /// </summary>
        GeneticAnalysis
    }
}
