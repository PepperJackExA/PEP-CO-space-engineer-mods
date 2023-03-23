using System.Collections.Generic;
using static ModAdjuster.DefinitionStructure;
using static ModAdjuster.DefinitionStructure.BlueprintDef;
using static ModAdjuster.DefinitionStructure.BlueprintDef.BPAction.BPMod;

namespace ModAdjuster
{
    public class BlueprintDefinitions
    {
        public List<BlueprintDef> Definitions = new List<BlueprintDef>()
        {   
            

            new BlueprintDef()
            {
                BlueprintName = "MyObjectBuilder_BlueprintDefinition/StoneOreToIngotStarting",
                BPActions = new[]
                {
                    new BPAction
                    {
                        Action = ReplaceResult, 
                        Index = 0,
                        Item = "MyObjectBuilder_Ingot/Cobalt"
                    },
                    new BPAction
                    {
                        Action = ReplaceResult, 
                        Index = 1,
                        Item = "MyObjectBuilder_Ingot/Silver",
                    },
                    new BPAction
                    {
                        Action = RemoveResult, 
                        Index = 2,
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 3,
                    },
                    new BPAction
                    {
                        Action = RemoveResult, 
                        Index = 4,
                    },
                    new BPAction
                    {
                        Action = RemoveResult, 
                        Index = 5,
                    },
                }
            },
            new BlueprintDef()
            {
                BlueprintName = "MyObjectBuilder_BlueprintDefinition/StoneOreToIngotBasic",
                BPActions = new[]
                {
                    new BPAction
                    {
                        Action = ReplaceResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ingot/Cobalt"
                    },
                    new BPAction
                    {
                        Action = ReplaceResult,
                        Index = 1,
                        Item = "MyObjectBuilder_Ingot/Silver",
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 2,
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 3,
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 4,
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 5,
                    },
                }
            },
            new BlueprintDef()
            {
                BlueprintName = "MyObjectBuilder_BlueprintDefinition/StoneOreToIngot",
                BPActions = new[]
                {
                    new BPAction
                    {
                        Action = ReplaceResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ingot/Cobalt"
                    },
                    new BPAction
                    {
                        Action = ReplaceResult,
                        Index = 1,
                        Item = "MyObjectBuilder_Ingot/Silver",
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 2,
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 3,
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 4,
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 5,
                    },
                }
            },

        };
    }
}