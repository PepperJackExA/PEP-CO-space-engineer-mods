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
            // Pipeline - IO compatability Mod
            new BlockDef()
            {
                BlockName = "CargoContainer/Pipeline_Cargo",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "Component/SteelPlate",
                        Index = 5,
                        Count = 150
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "Component/LargeTube",
                        Index = 2,
                        Count = 30
                    },
                    new BlockAction
                    {
                        Action = ChangeComponentCount,
                        Index = 3,
                        Count = 30
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "Component/AdvancedComputer",
                        Index = 5,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "Component/Electromagnet",
                        Index = 3,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "Component/GoldWire",
                        Index = 3,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "Component/SteelPlate",
                        Index = 0,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 7
                    },
                },

            },

        };

    }
}
