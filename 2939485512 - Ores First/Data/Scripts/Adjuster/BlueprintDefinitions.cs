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
            
         // Sotne to ore
            new BlueprintDef()
            {
                BlueprintName = "MyObjectBuilder_BlueprintDefinition/StoneOreToIngotStarting",
                BPActions = new[]
                {
                    new BPAction
                    {
                        Action = ReplaceResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ingot/Stone"
                    },
                    new BPAction
                    {
                        Action = ChangeAmountResult,
                        Index = 0,
                        Amount = 25f
                    },

                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 1
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 2
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 3
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 4
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 5
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Ice",
                        Amount = 0.05f
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Sulfur",
                        Amount = 0.05f
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Organic",
                        Amount = 0.1f
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Nickel",
                        Amount = 2f
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Silicon",
                        Amount = 3f
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Copper",
                        Amount = 5f
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Silver",
                        Amount = 0.1f
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Cobalt",
                        Amount = 0.1f
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Iron",
                        Amount = 15f
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
                        Item = "MyObjectBuilder_Ingot/Stone"
                    },
                    new BPAction
                    {
                        Action = ChangeAmountResult,
                        Index = 0,
                        Amount = 65f
                    },

                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 1
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 2
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 3
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 4
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Organic",
                        Amount = 0.1f
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
                        Item = "MyObjectBuilder_Ingot/Stone"
                    },
                    new BPAction
                    {
                        Action = ChangeAmountResult,
                        Index = 0,
                        Amount = 50f
                    },

                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 1
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 2
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 3
                    },
                    new BPAction
                    {
                        Action = RemoveResult,
                        Index = 4
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Organic",
                        Amount = 0.15f
                    },

                }
            },
        // Survival kit - Ore to Ingots 
            new BlueprintDef()
            {
                BlueprintName = "MyObjectBuilder_BlueprintDefinition/IronOreToIngotStarting",
                BPActions = new[]
                {
                    new BPAction
                    {
                        Action = ChangeProductionTime,
                        Amount = 0.2f
                    },

                }
            },
            new BlueprintDef()
            {
                BlueprintName = "MyObjectBuilder_BlueprintDefinition/CopperOreToIngotStarting",
                BPActions = new[]
                {
                    new BPAction
                    {
                        Action = ChangeProductionTime,
                        Amount = 0.2f
                    },

                }
            },
        };
    }
}