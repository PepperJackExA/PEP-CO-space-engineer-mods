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
            "MyObjectBuilder_WindTurbine/AntennaCorner",
        };

        public List<BlockDef> Definitions = new List<BlockDef>()
        {
            // List of blocks to modify. Can be as many or few as desired
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/LargeBlockRadioAntennaDish", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockName, // Change Display Name of the block
                        NewText = "Large Antenna Dish"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 50K",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 50
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent, // Replaces the component at the given index with a new component
                        Index = 8,
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 8,
                        Count = 100 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 7,
                        Count = 100 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 6,
                        Count = 50 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 5,
                        Count = 100 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 4,
                        Count = 500 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 3,
                        Count = 250 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 2,
                        Count = 30 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 50 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 100 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 7
                    },



                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/LargeBlockRadioAntenna", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockName, // Change Display Name of the block
                        NewText = "Large Antenna Tower"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 10K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 100 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 50 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
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
                BlockName = "MyObjectBuilder_RadioAntenna/OmnidirectionalAntenna", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockName, // Change Display Name of the block
                        NewText = "Omni Antenna Tower"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 10K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 15 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 15 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 8 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 15 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 75 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 38 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 8 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 15 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
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
                BlockName = "MyObjectBuilder_RadioAntenna/AntennaCube", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 5K",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 50 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 25 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
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
                BlockName = "MyObjectBuilder_RadioAntenna/Antenna45Corner", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 5K",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 50 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 25 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
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
                BlockName = "MyObjectBuilder_RadioAntenna/AntennaSlope", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 5K",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 50 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 25 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
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
                BlockName = "MyObjectBuilder_RadioAntenna/AntennaCorner", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 5K",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 50 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 25 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
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
                BlockName = "MyObjectBuilder_RadioAntenna/LBShortRadioAntenna", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 2K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 2 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 8 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 2
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/SBAngledRadioAntenna", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 2K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 2 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 8 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 2
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/SBLongRadioAntenna", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 2K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 2 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 8 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 4 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 2
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/SmallBlockRadioAntenna", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 500m",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 6,
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 5,
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 2 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 1
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/OmnidirectionalAntennaSmall", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block
                    new BlockAction
                    {
                        Action = ChangeBlockDescription, // Inserts the given number of the given component at the given index of a block's component list
                        NewText = "Antenna Range = 200m",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 4,
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 8 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 3
                    },
                    new BlockAction
                    {
                        Action = ChangePCU, // Sets block PCU
                        Value = 1
                    },

                }
            },
        };

    }
}