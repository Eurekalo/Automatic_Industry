// Copyright (c) 2026 AutoMachine Rebuilt contributors. Licensed under the MIT License.

namespace AutoMachineRebuilt
{
    /// <summary>
    /// Translatable strings of the mod. The nested class names form the
    /// translation keys (for example
    /// <c>STRINGS.AUTOMACHINEREBUILT.CATEGORY.FABRICATORS</c>) and are
    /// exported to the <c>translations</c> folder as PO files.
    ///
    /// Building titles are intentionally short placeholders: at game start
    /// they are replaced by the vanilla building names so the options screen
    /// always matches the player's game language
    /// (see <see cref="Localization.BuildingNameBinder"/>).
    /// </summary>
    public static class STRINGS
    {
        public static class AUTOMACHINEREBUILT
        {
            public static class CATEGORY
            {
                public static LocString FABRICATORS = "Fabricators";
                public static LocString SPECIAL = "Special buildings";
                public static LocString RANCHING = "Ranching";
                public static LocString GENERAL = "General";
                public static LocString MANUAL = "Manual buildings";
                public static LocString RESEARCH = "Research buildings";
                public static LocString ROOM = "Room bound buildings";
                public static LocString PROGRESSBARS = "Progress Bars";
                public static LocString AUTOSWEEPER = "Auto-Sweeper";
                public static LocString LOGGING = "Diagnostics & Logging";
            }

            public static class OPTION
            {
                public static LocString AUTOSWEEPERHARVEST = "Auto-Sweeper: Harvest Plants & Crops";
                public static LocString AUTOSWEEPERRESPECTHARVESTDESIGNATION = "Auto-Sweeper: Respect 'Do Not Harvest' Setting";
                public static LocString AUTOSWEEPERCANCELDUPEHARVESTCHORE = "Auto-Sweeper: Cancel Duplicant Harvest Chores";
                public static LocString AUTOSWEEPERBOTTLERDIRECTPICKUP = "Auto-Sweeper: Direct Bottler / Canister Pickup";
                public static LocString AUTOSWEEPERSCANINTERVAL = "Auto-Sweeper: Harvest Scan Interval (s)";

                public static LocString LANGUAGE = "Options language";
                public static LocString BILINGUAL = "Bilingual labels";
                public static LocString OILREFINERYEFFICIENCY = "Oil Refinery ratio";
                public static LocString RANCHINTERVAL = "Ranching interval (s)";
                public static LocString VERBOSELOGGING = "Verbose logging";
                public static LocString ENABLEDIAGNOSTICLOGGING = "Enable diagnostic logging";
                public static LocString LOGBUILDINGAUTOMATION = "Log building automation events";
                public static LocString LOGBUILDINGINTERACTIONS = "Log building interactions";
                public static LocString LOGPROBLEMATICBUILDINGS = "Log problematic building states";
                public static LocString GEYSERSTUDY = "Geyser study";
                public static LocString IGNOREROOMPREFIX = "Ignore room";
                public static LocString IGNORECROPDEMAND = "Farm Station: produce without demand";
                public static LocString IGNOREPOWERDEMAND = "Power Control Station: produce without demand";
                public static LocString SWEEPERRESEARCH = "Auto-Sweeper delivery to research buildings";
                public static LocString EFFICIENCYVANILLA = "Vanilla 50%";
                public static LocString EFFICIENCYFULL = "Full 100%";

                public static class PROGRESSBAR
                {
                    public static LocString RESEARCH = "Research Stations: Progress Bar";
                    public static LocString GEOTUNER = "Geotuner: Data/Content Consumption Progress Bar";
                    public static LocString BOTTLER = "Bottle / Canister Filler: Storage Fill Progress Bar";
                    public static LocString FABRICATORS = "Fabricators & Cooking Stations: Work Progress Bar";
                    public static LocString TELESCOPES = "Telescopes & Enclosed Telescopes: Progress Bar";
                    public static LocString SPICEGRINDER = "Spice Grinder: Spicing Progress Bar";
                    public static LocString GLEANER = "Gleaner (Milk Separator): Storage Progress Bar";
                    public static LocString POWERCONTROLSTATION = "Power Control Station: Microchip Crafting Progress Bar";
                    public static LocString GEYSERTUNING = "Geyser Tuning: Amplification Count Progress Bar";
                }

                public static LocString HIDEWORLDICONSKILL = "Hide skill requirement world icon";
                public static LocString HIDEWORLDICONROOM = "Hide room requirement world icon";
                public static LocString LIQUIDRESERVOIRDUPLICANTFETCH = "Liquid Reservoir Fetch (Duplicants)";
                public static LocString LIQUIDRESERVOIRAUTOSWEEPERFETCH = "Liquid Reservoir Convey (Auto-Sweeper)";
                public static LocString ENABLEPERBUILDINGCUSTOMIZATION = "Enable Per-Building Customization";
                public static LocString TOGGLEMODE = "Automation Toggle Mode";
                public static LocString TOGGLEMODE_DUPE = "Duplicant Wrench";
                public static LocString TOGGLEMODE_INSTANT = "Instant (No Chore)";
                public static LocString TOGGLEMODE_SMART = "Smart Hybrid";

                public static class IGNORETOOCOLD
                {
                    public static LocString ICECOOLEDFAN = "Ice-E Fan: Ignore Too Cold";
                }
            }

            public static class UI
            {
                public static class USERMENUACTIONS
                {
                    public static LocString ENABLE_AUTOMATION = "Enable Automation";
                    public static LocString ENABLE_AUTOMATION_TOOLTIP = "Upgrade this machine to operate unattended without duplicants.";
                    public static LocString DISABLE_AUTOMATION = "Revert to Manual";
                    public static LocString DISABLE_AUTOMATION_TOOLTIP = "Revert this machine back to vanilla manual duplicant operation.";
                    public static LocString CANCEL_TOGGLE = "Cancel Upgrade";
                    public static LocString CANCEL_TOGGLE_TOOLTIP = "Cancel the pending automation upgrade task.";
                    public static LocString RESET_OVERRIDE = "Follow Global Setting";
                    public static LocString RESET_OVERRIDE_TOOLTIP = "Clear individual customization on this machine and sync with global mod settings.";
                    public static LocString MOD_SOURCE_TAG = "\n\n<color=#4BC5FF><b>[Mod: Automatic Industry]</b></color>";
                }

                public static class STATUSITEMS
                {
                    public static LocString AUTOMATION_ENABLED = "Automation Active";
                    public static LocString AUTOMATION_ENABLED_TOOLTIP = "This building is operating automatically without duplicant labor.";
                    public static LocString AUTOMATION_MANUAL = "Manual Operation";
                    public static LocString AUTOMATION_MANUAL_TOOLTIP = "This building is operating under vanilla manual duplicant control.";
                    public static LocString AUTOMATION_PENDING = "Pending Automation Upgrade";
                    public static LocString AUTOMATION_PENDING_TOOLTIP = "Awaiting a duplicant with a wrench to perform the automation modification.";
                }
            }

            public static class TOOLTIP
            {
                public static class IGNORETOOCOLD
                {
                    public static LocString ICECOOLEDFAN = "Allows the Ice-E Fan to continue cooling even when the ambient temperature is below the vanilla 5°C threshold.";
                }
                public static LocString ENABLEDIAGNOSTICLOGGING = "Enables high-level diagnostic log messages for mod loading and lifecycle events.";
                public static LocString LOGBUILDINGAUTOMATION = "Logs informational messages when automated buildings begin or complete work cycles.";
                public static LocString LOGBUILDINGINTERACTIONS = "Logs when materials are delivered, chores are suppressed, or workers are evicted.";
                public static LocString LOGPROBLEMATICBUILDINGS = "Logs detailed diagnostics for complex state machine buildings (Geotuner, Ranching Stations, Oil Refinery, Biobot Maker).";
                public static class PROGRESSBAR
                {
                    public static LocString RESEARCH = "Displays the floating research progress bar while research buildings are automatically operating.";
                    public static LocString GEOTUNER = "Displays a floating progress bar showing the Geotuner's data depletion (100% to 0%) during broadcast, or material delivery progress.";
                    public static LocString BOTTLER = "Displays a floating progress bar reflecting internal storage fill percentage.";
                    public static LocString FABRICATORS = "Displays a floating progress bar for unattended fabricators and cooking stations.";
                    public static LocString TELESCOPES = "Displays a floating progress bar while telescopes are analyzing space targets.";
                    public static LocString SPICEGRINDER = "Displays a floating progress bar while food is being automatically spiced.";
                    public static LocString GLEANER = "Displays a floating progress bar reflecting solid output accumulation.";
                    public static LocString POWERCONTROLSTATION = "Displays a floating progress bar while microchips are being fabricated.";
                    public static LocString GEYSERTUNING = "Displays a floating progress bar (0%-100%) on tuned Geysers indicating how many Geotuners are currently tuning them (20% per Geotuner).";
                }

                public static LocString HIDEWORLDICONSKILL = "Hides the red skill requirement warning icon floating beneath buildings in the world view, while keeping the details in the selection side screen.";
                public static LocString HIDEWORLDICONROOM = "Hides the red room requirement warning icon floating beneath buildings in the world view, while keeping the details in the selection side screen.";
                public static LocString LIQUIDRESERVOIRDUPLICANTFETCH = "Allows Duplicants to fetch liquid directly from the Liquid Reservoir.";
                public static LocString LIQUIDRESERVOIRAUTOSWEEPERFETCH = "Allows Auto-Sweepers (Solid Transfer Arm) to fetch liquid from the Liquid Reservoir and convey it.";

                public static class IGNOREDEMAND
                {
                    public static LocString POWERCONTROLSTATION = "Lets the Power Control Station keep producing Microchips even when no generator currently asks for them.";
                }

                public static LocString AUTOSWEEPERHARVEST = "Enables Auto-Sweepers (Solid Transfer Arms) to automatically harvest ready crops and plants within reach.";
                public static LocString AUTOSWEEPERRESPECTHARVESTDESIGNATION = "When enabled, Auto-Sweepers will only harvest plants that are marked for harvest, protecting wild decorative plants and Bonbon/Arbor trees designated for nectar.";
                public static LocString AUTOSWEEPERCANCELDUPEHARVESTCHORE = "Cancels active Duplicant harvest errands when an Auto-Sweeper harvests a plant, preventing duplicants from walking over to harvest empty air.";
                public static LocString AUTOSWEEPERBOTTLERDIRECTPICKUP = "Allows Auto-Sweepers to reach directly into Liquid Bottlers and Gas Canister Fillers without dropping bottles on the floor.";
                public static LocString AUTOSWEEPERSCANINTERVAL = "Controls the interval (in seconds) at which Auto-Sweepers scan for harvestable plants in range.";

                public static LocString LANGUAGE =
                    "Language of this options screen. Auto follows the game language.";

                public static LocString BILINGUAL =
                    "Show the English text next to the translated text.";

                public static LocString FABRICATOR =
                    "Runs without a Duplicant as long as power and ingredients are available.\n" +
                    "Ingredients still have to be delivered as usual.";

                public static LocString SMOKER =
                    "Runs without a Duplicant and empties its finished products automatically.";

                public static LocString OILREFINERY =
                    "Runs without a Duplicant while crude oil is available and the surrounding gas pressure is not too high.";

                public static LocString OILREFINERYEFFICIENCY =
                    "Vanilla: 10 kg/s crude oil produces 5 kg/s petroleum and 90 g/s natural gas.\n" +
                    "Full: 10 kg/s crude oil produces 10 kg/s petroleum and 180 g/s natural gas (legacy Auto Machine behaviour).\n" +
                    "Changing this affects newly loaded games only.";

                public static LocString OILWELLCAP =
                    "Releases the accumulated pressure automatically once the depressurize threshold on the building is reached.";

                public static LocString GEOTUNER =
                    "Performs the scientist interaction automatically once the required resource has been delivered.";

                public static LocString RANCHSTATION =
                    "Tends eligible critters automatically at a fixed interval. Room and power requirements still apply.";

                public static LocString RANCHINTERVAL =
                    "How often an automated ranching station tends one critter.";

                public static LocString AUTOWORK =
                    "Performs the vanilla work of this building without a Duplicant once every\n" +
                    "vanilla requirement (power, supplies, state) is satisfied.";

                public static LocString AUTOWORKROOM =
                    "Performs the vanilla work without a Duplicant. The vanilla room requirement\n" +
                    "stays active unless the matching \"Ignore room\" option is enabled.";

                public static LocString IGNORECROPDEMAND =
                    "Lets the Farm Station keep producing Micronutrient Fertilizer even when " +
                    "no plant currently asks for it.";

                public static LocString SWEEPERRESEARCH =
                    "Switches the delivery task of the research buildings to a machine delivery, " +
                    "so an Auto-Sweeper can supply their water and Data Banks. Requires a restart.";

                public static LocString MISSIONCONTROL =
                    "Boosts a rocket automatically once the vanilla station reports a rocket in " +
                    "range. Room and power requirements still apply.";

                public static LocString IGNOREROOM =
                    "Downgrades the mandatory room requirement of this building to a recommendation.";

                public static LocString GEYSERSTUDY =
                    "Studies geysers and other studyable features automatically.";

                public static LocString VERBOSELOGGING =
                    "Writes detailed automation messages to Player.log. Use it when reporting an issue.";
            }

            /// <summary>
            /// Fallback building titles. Overwritten at runtime with the
            /// vanilla building names of the active game language.
            /// </summary>
            public static class BUILDING
            {
                public static LocString COOKINGSTATION = "Electric Grill";
                public static LocString GOURMETCOOKINGSTATION = "Gas Range";
                public static LocString MICROBEMUSHER = "Microbe Musher";
                public static LocString DEEPFRYER = "Deep Fryer";
                public static LocString MILKPRESS = "Plant Pulverizer";
                public static LocString SMOKER = "Smoker";
                public static LocString ROCKCRUSHER = "Rock Crusher";
                public static LocString METALREFINERY = "Metal Refinery";
                public static LocString GLASSFORGE = "Glass Forge";
                public static LocString SUPERMATERIALREFINERY = "Molecular Forge";
                public static LocString SUITFABRICATOR = "Exosuit Forge";
                public static LocString CLOTHINGFABRICATOR = "Textile Loom";
                public static LocString CLOTHINGALTERATIONSTATION = "Clothing Refashionator";
                public static LocString CRAFTINGTABLE = "Sculpting Block";
                public static LocString ADVANCEDCRAFTINGTABLE = "Soldering Station";
                public static LocString SLUDGEPRESS = "Sludge Press";
                public static LocString DIAMONDPRESS = "Diamond Press";
                public static LocString CHEMICALREFINERY = "Emulsifier";
                public static LocString MISSILEFABRICATOR = "Blastshot Maker";
                public static LocString DATAMINER = "Data Miner";
                public static LocString OILREFINERY = "Oil Refinery";
                public static LocString OILWELLCAP = "Oil Well";
                public static LocString GEOTUNER = "Geotuner";
                public static LocString RANCHSTATION = "Grooming Station";
                public static LocString SHEARINGSTATION = "Shearing Station";
                public static LocString MILKINGSTATION = "Milking Station";
                public static LocString UNDERWATERRANCHSTATION = "Aquatic Grooming Station";
                public static LocString UNDERWATERSHEARINGSTATION = "Aquatic Shearing Station";
                public static LocString UNDERWATERMILKINGSTATION = "Aquatic Milking Station";
                public static LocString MANUALGENERATOR = "Manual Generator";
                public static LocString TELESCOPE = "Telescope";
                public static LocString CLUSTERTELESCOPE = "Telescope";
                public static LocString CLUSTERTELESCOPEENCLOSED = "Enclosed Telescope";
                public static LocString MANUALHIGHENERGYPARTICLESPAWNER = "Manual Radbolt Generator";
                public static LocString RESETSKILLSSTATION = "Skill Scrubber";
                public static LocString APOTHECARY = "Apothecary";
                public static LocString ADVANCEDAPOTHECARY = "Nuclear Apothecary";
                public static LocString SUSHIBAR = "Sushi Bar";
                public static LocString ICEKETTLE = "Ice Liquefier";
                public static LocString CAMPFIRE = "Wood Heater";
                public static LocString ICECOOLEDFAN = "Ice-E Fan";
                public static LocString COMPOST = "Compost";
                public static LocString FOODDEHYDRATOR = "Dehydrator";
                public static LocString RESEARCHCENTER = "Research Station";
                public static LocString ADVANCEDRESEARCHCENTER = "Super Computer";
                public static LocString COSMICRESEARCHCENTER = "Virtual Planetarium";
                public static LocString DLC1COSMICRESEARCHCENTER = "Virtual Planetarium";
                public static LocString NUCLEARRESEARCHCENTER = "Materials Study Terminal";
                public static LocString ORBITALRESEARCHCENTER = "Orbital Data Collection Lab";
                public static LocString GENETICANALYSISSTATION = "Botanical Analyzer";
                public static LocString MORBROVERMAKER = "Biobot Builder";
                public static LocString MISSIONCONTROL = "Mission Control Station";
                public static LocString MISSIONCONTROLCLUSTER = "Mission Control Station";
                public static LocString FARMSTATION = "Farm Station";
                public static LocString SPICEGRINDER = "Spice Grinder";
                public static LocString POWERCONTROLSTATION = "Power Control Station";
                public static LocString FABRICATEDWOODMAKER = "Plywood Press";
                public static LocString LIQUIDBOTTLER = "Bottle Filler";
                public static LocString GASBOTTLER = "Canister Filler";
                public static LocString LIQUIDPUMPINGSTATION = "Pitcher Pump";
            }
        }
    }
}
