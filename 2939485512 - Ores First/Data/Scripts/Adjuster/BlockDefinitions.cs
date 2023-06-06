using System.Collections.Generic;
using static PepCo_OresFirst.DefinitionStructure;
using static PepCo_OresFirst.DefinitionStructure.BlockDef;
using static PepCo_OresFirst.DefinitionStructure.BlockDef.BlockAction.BlockMod;

namespace PepCo_OresFirst
{
    public class BlockDefinitions
    {
        public string AdminComponent = "MyObjectBuilder_Component/SomeComponment"; // Component to insert into disabled blocks to prevent building from projection
        public List<string> DisabledBlocks = new List<string>() // List of blocks to disable
        {

        };

        public List<BlockDef> Definitions = new List<BlockDef>()
        {
            // List of blocks to modify. Can be as many or few as desired
            new BlockDef()
            {
                BlockName = "BatteryBlock/SmallBlockSmallAlkalineBatteryBlock", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block

                    new BlockAction
                    {
                        Action = ChangeComponentCount,
                        Index = 3,
                        Count = 1
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "BatteryBlock/LargeBlockAlkalineBatteryBlock", // Name of the block to modify. Format is "MyObjectBuilder_Type/Subtype" in the same format as BlockVariantGroups
                BlockActions = new[] // List of modifications to make. Can be as many or few as desired
                {
                    // The following modifications can be used on any type of block

                    new BlockAction
                    {
                        Action = ChangeComponentCount,
                        Index = 3,
                        Count = 20
                    },

                }
            }

        };

    }
}
