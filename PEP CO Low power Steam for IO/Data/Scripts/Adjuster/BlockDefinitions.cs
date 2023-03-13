using System.Collections.Generic;
using static ModAdjuster.DefinitionStructure;
using static ModAdjuster.DefinitionStructure.BlockDef;
using static ModAdjuster.DefinitionStructure.BlockDef.BlockAction.BlockMod;

namespace ModAdjuster
{
    public class BlockDefinitions
    {
        public string AdminComponent = "MyObjectBuilder_Component/admin_Fluxkondensator_Pepco"; // Component to insert into disabled blocks to prevent building from projection
        public List<string> DisabledBlocks = new List<string>() // List of blocks to disable
        {
            "MyObjectBuilder_WindTurbine/LargeWindTurbine",
            "MyObjectBuilder_OxygenGenerator/CoalFurnace",
            "MyObjectBuilder_OxygenFarm/SolarConcentrator",
            "MyObjectBuilder_WindTurbine/LargeBlockWindTurbine",
        };

        public List<BlockDef> Definitions = new List<BlockDef>()
        {
            new BlockDef()
            {
                BlockName = "HydrogenEngine/SteamTurbineMirrored",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "Pressurized Steam Turbine for generating electricity. Must be connected to a source of Steam. Consumes 100 Steam/sec. Max Output: 100MW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 100f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 50
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "HydrogenEngine/SteamTurbine",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "Pressurized Steam Turbine for generating electricity. Must be connected to a source of Steam. Consumes 100 Steam/sec. Max Output: 100MW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 100f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 50
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "SolarPanel/LargeFullSpectrumSolarPanel",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "High-tech panel capable of generating power from all electromagnetic wavelengths at high efficiency. Max Output: 1MW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 1.5f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 10
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "SolarPanel/SmallFullSpectrumSolarPanel",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "High-tech panel capable of generating power from all electromagnetic wavelengths at high efficiency. Max Output: 300kW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 0.3f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 5
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "SolarPanel/LargeBlockSolarPanel",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "Basic Solar panel capable of generating power. Max Output: 500kW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 0.5f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 10
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "SolarPanel/SmallBlockSolarPanel",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "Basic Solar panel capable of generating power. Max Output: 150kW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 0.15f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 5
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "WindTurbine/SergWindTurbinePart",
                BlockActions = new[]
                {
                    
                    new BlockAction
                    {
                        Action = ChangeBlockName, // Change Display Name of the block
                        NewText = "Vertical Wind Turbine"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "Basic Solar panel capable of generating power. Nominal Max Output: 1.5MW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 1.5f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 25
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "WindTurbine/SergWindTurbine",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockName, // Change Display Name of the block
                        NewText = "Vertical Wind Turbine Top"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "Basic Solar panel capable of generating power. Nominal Max Output: 1.5MW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 1.5f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 25
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "WindTurbine/Serg_10MW_Wind_Turbine",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockName, // Change Display Name of the block
                        NewText = "Large Vertical Wind Turbine"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "Basic Solar panel capable of generating power. Nominal Max Output: 3MW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 3f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 50
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "WindTurbine/Serg_25MW_Wind_Turbine",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockName, // Change Display Name of the block
                        NewText = "XL Wind Turbine"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Change the Description of the block
                        NewText = "Extra Large Wind Turbine capable of generating power. Nominal Max Output: 10MW"
                    },
                    new BlockAction
                    {
                        Action = ChangeMaxPowerOutput, // Sets <MaxPowerOutput> for any reactor, hydrogen engine, solar panel, wind turbine, or battery
                        Value = 10f
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 100
                    },

                }

            },
            new BlockDef()
            {
                BlockName = "HydrogenEngine/FusionReactor",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 100
                    },

                }

            },

        };

    }
}
