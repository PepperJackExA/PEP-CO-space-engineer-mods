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
                BlockName = "MyProgrammableBlock/LargeMicroPB", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {                    
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 8,
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
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/AdvancedComputer",
                        Index = 0,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Display",
                        Index = 0,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/LargeTube",
                        Index = 0,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyProgrammableBlock/MicroPB", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 8,
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
                        Count = 1 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/AdvancedComputer",
                        Index = 0,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Display",
                        Index = 0,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/LargeTube",
                        Index = 0,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 6
                    },
                }
            },
        };

    }
}
