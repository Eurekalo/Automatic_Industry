// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

namespace AutoMachineRebuilt.Automation
{
    /// <summary>
    /// Static description of one building that is automated through the
    /// generic <see cref="AutoMachineRebuilt.Components.AutoWorkableController"/>.
    ///
    /// Every entry is resolved softly by type name so that a building that
    /// belongs to a DLC the player does not own is simply skipped instead of
    /// throwing a <see cref="System.TypeLoadException"/> while patching.
    /// </summary>
    internal sealed class AutomationEntry
    {
        /// <summary>Creates an immutable registry entry.</summary>
        /// <param name="optionKey">
        /// Short upper case key. It identifies the option toggle, the option
        /// string (<c>STRINGS.AUTOMACHINEREBUILT.BUILDING.&lt;key&gt;</c>) and
        /// the driver instance attached to the prefab.
        /// </param>
        /// <param name="configTypeName">Vanilla building config class name.</param>
        /// <param name="roomRequired">
        /// Whether the vanilla building enforces a room requirement that the
        /// player may optionally waive.
        /// </param>
        internal AutomationEntry(string optionKey, string configTypeName, bool roomRequired)
            : this(optionKey, configTypeName, roomRequired, AutomationMechanism.Workable)
        {
        }

        /// <summary>Creates an immutable registry entry for one mechanism.</summary>
        /// <param name="optionKey">Option and string key of the building.</param>
        /// <param name="configTypeName">Vanilla building config class name.</param>
        /// <param name="roomRequired">Whether a room waiver toggle exists.</param>
        /// <param name="mechanism">Vanilla mechanism used by the building.</param>
        internal AutomationEntry(string optionKey, string configTypeName, bool roomRequired,
                                 AutomationMechanism mechanism)
        {
            OptionKey = optionKey;
            ConfigTypeName = configTypeName;
            RoomRequired = roomRequired;
            Mechanism = mechanism;
        }

        /// <summary>Vanilla mechanism the building uses for its manual work.</summary>
        internal AutomationMechanism Mechanism { get; private set; }

        /// <summary>Option and string key of the building.</summary>
        internal string OptionKey { get; private set; }

        /// <summary>Name of the vanilla <c>IBuildingConfig</c> class.</summary>
        internal string ConfigTypeName { get; private set; }

        /// <summary>Whether a "ignore room requirement" toggle exists.</summary>
        internal bool RoomRequired { get; private set; }
    }
}
