﻿using System.Collections.Generic;
using static PepCo_OresFirst.DefinitionStructure;
using static PepCo_OresFirst.DefinitionStructure.BlueprintDef;
using static PepCo_OresFirst.DefinitionStructure.BlueprintDef.BPAction.BPMod;

namespace PepCo_OresFirst
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
                        Amount = 0.25f
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
                        Amount = 0.375f
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
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ingot/Stone",
                        Amount = 25f
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
                        Item = "MyObjectBuilder_Ore/Ice",
                        Amount = 0.5f
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Organic",
                        Amount = 0.75f
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
                        Item = "MyObjectBuilder_Ore/Ice",
                        Amount = 1
                    },
                    new BPAction
                    {
                        Action = InsertResult,
                        Index = 0,
                        Item = "MyObjectBuilder_Ore/Organic",
                        Amount = 1.25f
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