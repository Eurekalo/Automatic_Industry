// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

using PeterHan.PLib.Options;

namespace AutoMachineRebuilt.Config
{
    /// <summary>
    /// Conversion ratio used by the Oil Refinery.
    /// </summary>
    public enum OilRefineryEfficiency
    {
        /// <summary>
        /// Vanilla behaviour: 10 kg/s crude oil produces 5 kg/s petroleum and
        /// 90 g/s natural gas (50 % mass efficiency).
        /// </summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.EFFICIENCYVANILLA")]
        Vanilla50,

        /// <summary>
        /// Legacy "Auto Machine" behaviour: 10 kg/s crude oil produces
        /// 10 kg/s petroleum and 180 g/s natural gas (100 % mass efficiency).
        /// </summary>
        [Option("STRINGS.AUTOMACHINEREBUILT.OPTION.EFFICIENCYFULL")]
        Full100
    }
}
