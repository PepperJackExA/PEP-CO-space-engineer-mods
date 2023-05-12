using System.Collections.Generic;
using static ModAdjuster.DefinitionStructure;
using static ModAdjuster.DefinitionStructure.BlockDef;
using static ModAdjuster.DefinitionStructure.BlockDef.BlockAction.BlockMod;
using static ModAdjuster.DefinitionStructure.GasDef;
using static ModAdjuster.DefinitionStructure.GasDef.GasAction.GasMod;


namespace ModAdjuster
{
    public class GasDefinitions
    {
        public List<GasDef> Definitions = new List<GasDef>()
        {
            new GasDef()
            {
                GasName = "GasProperties/Hydrogen",
                GasActions = new[]
                {
                    new GasAction
                    {
                        Action = ChangeDisplayName,
                        NewText = "TestName"

                    },
                    new GasAction
                    {
                        Action = ChangeEnergyDensity,
                        Value = 1000f
                    },
                }
            }
        };
    }
    public class BlockDefinitions
    {

        // Engines
        internal const int LargeHydrogenEngine_MaxPowerOutput = 1;
        internal const int LargeHydrogenEngine_FuelCapacity = 1;
        internal const int LargeHydrogenEngine_FuelMultiplier = 1;

        // Thrust
        internal const int LargeBlockSmallHydrogenThrust_ThrustForce = 10000;
        internal const decimal LargeBlockSmallHydrogenThrust_ThrustPowerConsumption = 1.25M;
        internal const int LargeBlockSmallHydrogenThrust_ThrustFuelEfficiency = 1;


        public List<BlockDef> Definitions = new List<BlockDef>()

        {
                   
            //HydrogenEngine
            new BlockDef()
            {
                BlockName = "HydrogenEngine/LargeHydrogenEngine",
                BlockActions = new[]
                {
                    // These modifications are specific to power production blocks
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                         NewText = "Max Power Output: " + LargeHydrogenEngine_MaxPowerOutput + " Newton" + "\n" +
                                  "Fuel Capacity: " + LargeHydrogenEngine_FuelCapacity + "\n" +
                                  "Fuel Efficiency: " + LargeHydrogenEngine_FuelMultiplier,
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput,
                        Value = LargeHydrogenEngine_MaxPowerOutput
                    },
                    new BlockAction
                    {
                        Action = ChangeFuelCapacity,
                        Value = LargeHydrogenEngine_FuelCapacity,
                    },
                    new BlockAction
                    {
                        Action = ChangeFuelMultiplier,
                        Value = LargeHydrogenEngine_FuelMultiplier,
                    },


                }
            },
            //LBSmallHydrogenThrust
            new BlockDef()
            {
                BlockName = "Thrust/LargeBlockSmallHydrogenThrust",
                BlockActions = new[]                     
                {
                    // These modifications are specific to power production blocks
                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Test Thurster"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Thrust Force: " + LargeBlockSmallHydrogenThrust_ThrustForce + " Newton" + "\n" +
                                  "Thrust Power Consumption: " + LargeBlockSmallHydrogenThrust_ThrustPowerConsumption + "\n" +
                                  "Fuel Efficiency: " + LargeBlockSmallHydrogenThrust_ThrustFuelEfficiency,
                                              
                    },
                    new BlockAction
                    {
                        Action = ChangeThrustForce,
                        Value = LargeBlockSmallHydrogenThrust_ThrustForce
                    },
                    new BlockAction
                    {
                        Action = ChangeThrustPowerConsumption,
                        Value = (float)LargeBlockSmallHydrogenThrust_ThrustPowerConsumption,
                    },
                    new BlockAction
                    {
                        Action = ChangeThrustFuelEfficiency,
                        Value = LargeBlockSmallHydrogenThrust_ThrustFuelEfficiency,
                    },


                }
            },

        };

    }
}
