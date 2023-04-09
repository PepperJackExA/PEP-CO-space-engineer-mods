﻿using System.Collections.Generic;
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
                BlockName = "Searchlight/SmallSearchlight", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Component = "Component/BulletproofGlass",
                        Index = 0,
                        Count = 2 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Lightbulb",
                        Index = 0,
                        Count = 3
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 5
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
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/LargeTube",
                        Index = 0,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 3
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
                BlockName = "Searchlight/LargeSearchlight", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Component = "Component/BulletproofGlass",
                        Index = 0,
                        Count = 10 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Lightbulb",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 5
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
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 4
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
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 5
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
                BlockName = "ReflectorLight/OffsetSpotlight", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Component = "Component/BulletproofGlass",
                        Index = 0,
                        Count = 2 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Lightbulb",
                        Index = 0,
                        Count = 2
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
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 3
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "ReflectorLight/LargeBlockFrontLight", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Component = "Component/BulletproofGlass",
                        Index = 0,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Lightbulb",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 15
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 3
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "ReflectorLight/LargeBlockFrontLight", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Component = "Component/BulletproofGlass",
                        Index = 0,
                        Count = 5 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Lightbulb",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 15
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 3
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "ReflectorLight/SmallBlockFrontLight", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Component = "Component/BulletproofGlass",
                        Index = 0,
                        Count = 2 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Lightbulb",
                        Index = 0,
                        Count = 2
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
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 3
                    },
                }
            },


        };

    }
}