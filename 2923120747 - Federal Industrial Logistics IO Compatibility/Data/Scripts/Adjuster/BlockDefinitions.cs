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
                BlockName = "CargoContainer/Industrial Storage", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Component = "Component/InteriorPlate",
                        Index = 0,
                        Count = 360 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 120
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 48
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Display",
                        Index = 0,
                        Count = 6
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 120
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 120
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/MetalGrid",
                        Index = 0,
                        Count = 144
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 360
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/InteriorPlate",
                        Index = 0,
                        Count = 1440
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
                BlockName = "CargoContainer/Stone Storage", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Count = 360 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 120
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 48
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 120
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 120
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 360
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 1000
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 4
                    },
                }
            },
            //Small Grid
            new BlockDef()
            {
                BlockName = "CargoContainer/Industrial Storage SG", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Component = "Component/InteriorPlate",
                        Index = 0,
                        Count = 132 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Display",
                        Index = 0,
                        Count = 6
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 48
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 40
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/MetalGrid",
                        Index = 0,
                        Count = 60
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 150
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/InteriorPlate",
                        Index = 0,
                        Count = 264
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
                BlockName = "CargoContainer/Stone Storage SG", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Count = 150 // This field is optional. If not specified or set to 0, the component count will stay the same
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
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 48
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/CopperWire",
                        Index = 0,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Computer",
                        Index = 0,
                        Count = 40
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 150
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 150
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex, // Sets the critical component which must be welded up for the block to function
                        Index = 5
                    },
                }
            },
            //Conveyor addons 
            new BlockDef()
            {
                BlockName = "Conveyor/Small Ship Conveyor 1-5", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
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
                        Count = 20 // This field is optional. If not specified or set to 0, the component count will stay the same
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Motor",
                        Index = 0,
                        Count = 6
                    },
                    new BlockAction
                    {
                        Action = InsertComponent, // Inserts the given number of the given component at the given index of a block's component list
                        Component = "Component/Construction",
                        Index = 0,
                        Count = 14
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
                        Index = 2
                    },
                }
            },
        };

    }
}
