using System.Collections.Generic;
using static ModAdjuster.DefinitionStructure;
using static ModAdjuster.DefinitionStructure.BlockDef;
using static ModAdjuster.DefinitionStructure.BlockDef.BlockAction.BlockMod;

namespace ModAdjuster
{
    public class BlockDefinitions
    {
        public string AdminComponent = "Component/SomeComponment"; // Component to insert into disabled blocks to prevent building from projection
        public List<string> DisabledBlocks = new List<string>() // List of blocks to disable
        {

        };

        public List<BlockDef> Definitions = new List<BlockDef>()
        {
            // List of blocks to modify. Can be as many or few as desired
            new BlockDef()
            {
                BlockName = "MotorAdvancedStator/AttachmentBase", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    new BlockAction
                    {
                        Action = ChangeSafetyDetach,
                        Value = 250,
                    },
                    new BlockAction
                    {
                        Action = ChangeSafetyDetachMax,
                        Value = 250,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent, 
                        Index = 7,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 6,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 5,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 4,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 3,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 2,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 1,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Electromagnet",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 15
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 40
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 25
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 5
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MotorAdvancedStator/AttachmentBaseSmall", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    new BlockAction
                    {
                        Action = ChangeSafetyDetach,
                        Value = 250,
                    },
                    new BlockAction
                    {
                        Action = ChangeSafetyDetachMax,
                        Value = 250,
                    },new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 7,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 6,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 5,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 4,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 3,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 2,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 1,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Electromagnet",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 15
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 40
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 25
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 5
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MotorAdvancedStator/AttachmentBaseTall", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    new BlockAction
                    {
                        Action = ChangeSafetyDetach,
                        Value = 250,
                    },
                    new BlockAction
                    {
                        Action = ChangeSafetyDetachMax,
                        Value = 250,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 7,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 6,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 5,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 4,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 3,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 2,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 1,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Electromagnet",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 15
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 40
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 5
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MotorAdvancedRotor/AttachmentTop", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 7,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 6,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 5,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 4,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 3,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 2,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 1,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 30 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/LargeTube",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 1
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MotorAdvancedRotor/AttachmentTopLarge", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 7,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 6,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 5,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 4,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 3,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 2,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 1,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent, // Replaces the component at the given index with a new component
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 30 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/LargeTube",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 1
                    },
                }
            },


        };

    }
}
