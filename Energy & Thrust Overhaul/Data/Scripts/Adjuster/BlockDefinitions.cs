using System.Collections.Generic;
using static ModAdjuster.DefinitionStructure;
using static ModAdjuster.DefinitionStructure.BlockDef;
using static ModAdjuster.DefinitionStructure.BlockDef.BlockAction.BlockMod;

namespace ModAdjuster
{
    public class BlockDefinitions
    {
       
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
                        NewText = "Chamge Description of HydrogenEngine!"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "TEST Hydrogen Engine!"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput,
                        Value = 0.177f
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
                        Action = ChangeBlockName,
                        NewText = "TEST Wind Turbine!"
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
                        Action = ChangeBlockName,
                        NewText = "NameChanged LargeBlockBatteryBlock"
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
