using System.Collections.Generic;
using static ModAdjuster.DefinitionStructure;
using static ModAdjuster.DefinitionStructure.BlockDef;
using static ModAdjuster.DefinitionStructure.BlockDef.BlockAction.BlockMod;


namespace ModAdjuster
{
    public class BlockDefinitions
    {

        // Engines
        internal const int enginepower1 = 2;
        internal const int enginepower2 = 2 * enginepower1;
        internal const int enginepower3 = 2 * enginepower2;
        internal const int enginepower4 = 2 * enginepower3;

        internal const int enginefuelmultiplier1 = 2;
        internal const int enginefuelmultiplier2 = 2 * enginefuelmultiplier1;
        internal const int enginefuelmultiplier3 = 2 * enginefuelmultiplier2;
        internal const int enginefuelmultiplier4 = 2 * enginefuelmultiplier3;

        internal const int enginefuelcapacity1 = 2;
        internal const int enginefuelcapacity2 = 2 * enginefuelcapacity1;
        internal const int enginefuelcapacity3 = 2 * enginefuelcapacity2;
        internal const int enginefuelcapacity4 = 2 * enginefuelcapacity3;

        // Wind turbines 
        internal const int windturbinepower1 = 2;
        internal const int windturbinepower2 = 2 * windturbinepower1;
        internal const int windturbinepower3 = 2 * windturbinepower2;
        internal const int windturbinepower4 = 2 * windturbinepower3;

        // Solar Panels
        internal const int SolarPower1 = 2;
        internal const int SolarPower2 = 2 * SolarPower1;
        internal const int SolarPower3 = 2 * SolarPower2;
        internal const int SolarPower4 = 2 * SolarPower3;

        // Steam turbines Power
        internal const int SteamPower1 = 2;
        internal const int SteamPower2 = 2 * SteamPower1;
        internal const int SteamPower3 = 2 * SteamPower2;
        internal const int SteamPower4 = 2 * SteamPower3;

        internal const int steamfuelmultiplier1 = 2;
        internal const int steamfuelmultiplier2 = 2 * steamfuelmultiplier1;
        internal const int steamfuelmultiplier3 = 2 * steamfuelmultiplier2;
        internal const int steamfuelmultiplier4 = 2 * steamfuelmultiplier3;

        internal const int steamfuelcapacity1 = 2;
        internal const int steamfuelcapacity2 = 2 * steamfuelcapacity1;
        internal const int steamfuelcapacity3 = 2 * steamfuelcapacity2;
        internal const int steamfuelcapacity4 = 2 * steamfuelcapacity3;

        // Steam production
        internal const int SteamProduction1 = 2;
        internal const int SteamProduction2 = 2 * SteamProduction1;
        internal const int SteamProduction3 = 2 * SteamProduction2;
        internal const int SteamProduction4 = 2 * SteamProduction3;

        // Coal Consumption
        internal const int SteamConsumption1 = 2;
        internal const int SteamConsumption2 = 2 * SteamConsumption1;
        internal const int SteamConsumption3 = 2 * SteamConsumption2;
        internal const int SteamConsumption4 = 2 * SteamConsumption3;

        // Batteries
        internal const int BatteriesMaxStorage1 = 2;
        internal const int BatteriesMaxStorage2 = 2 * BatteriesMaxStorage1;
        internal const int BatteriesMaxStorage3 = 2 * BatteriesMaxStorage2;
        internal const int BatteriesMaxStorage4 = 2 * BatteriesMaxStorage3;

        public List<BlockDef> Definitions = new List<BlockDef>()
        
        {

// Engines            
        
            new BlockDef()
            {
                BlockName = "HydrogenEngine/LargeHydrogenEngine",
                BlockActions = new[]
                {
                    // These modifications are specific to power production blocks
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Max Power Output: " + enginepower1 + "MW",
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput,
                        Value = enginepower1
                    },
                    new BlockAction
                    {
                        Action = ChangeFuelCapacity,
                        Value = 1f
                    },
                    new BlockAction
                    {
                        Action = ChangeFuelMultiplier,
                        Value = 1f
                    },
                    

                }
            },
            
// WIND TURBINES
            new BlockDef()
            {
                BlockName = "WindTurbine/LargeBlockWindTurbine",
                BlockActions = new[]
                {
                    // These modifications are specific to power production blocks
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Chamge Description of Wind Turbine!"
                    },                    
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput,
                        Value = 0.5f
                    },
                    new BlockAction
                    {
                        Action = ChangeOptimalWindSpeed,
                        Value = 100f
                    },
                     
                }
            },
// BATTERIES
            new BlockDef()
            {
                BlockName = "BatteryBlock/LargeBlockBatteryBlock",
                BlockActions = new[]
                {
                    // These modifications are specific to power production blocks
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Chamge Description of LargeBlockBatteryBlock!"
                    },
                    new BlockAction
                    {
                        Action = ChangeBatteryPowerStorage,
                        Value = 1f
                    },
                    new BlockAction
                    {
                        Action = ChangeBatteryMaxoutput,
                        Value = 0.200f
                    },
                    new BlockAction
                    {
                        Action = ChangeBatteryMaxRequiredInput,
                        Value = 0.100f
                    }

                }
            },
        };

    }
}
