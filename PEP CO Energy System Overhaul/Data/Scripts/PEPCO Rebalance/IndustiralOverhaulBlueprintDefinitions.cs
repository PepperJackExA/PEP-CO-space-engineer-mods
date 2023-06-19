using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;
using System.Collections.Generic;
using VRage;
using VRage.Utils;
using System;

namespace PEPCO.IndustrialOverhaulBlueprintDefinitionsRebalance
{

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase
    {
        public override void LoadData()
        {

            VanillaStoneOreToIngot(new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), "StoneOreToIngot"));

        }

        private void VanillaStoneOreToIngot(MyDefinitionId definitionId)
        {
           
            var definition = MyDefinitionManager.Static.GetBlueprintDefinition(definitionId);

            
            // Add more Prerequisites
            var newPrerequisite = new MyBlueprintDefinitionBase.Item[definition.Prerequisites.Length + 1];
            newPrerequisite[1] = new MyBlueprintDefinitionBase.Item();
            newPrerequisite[1].Id = MyDefinitionId.Parse("MyObjectBuilder_Ore/Gold");
            newPrerequisite[1].Amount = 10;
            definition.Prerequisites = newPrerequisite;
            // Prerequisites
            definition.Prerequisites[0].Id = MyDefinitionId.Parse("MyObjectBuilder_Ore/Stone");
            definition.Prerequisites[0].Amount = 10;

            // Add more Results
            var newResult = new MyBlueprintDefinitionBase.Item[definition.Results.Length + 1];
            newResult[4] = new MyBlueprintDefinitionBase.Item();
            newResult[4].Id = MyDefinitionId.Parse("MyObjectBuilder_Ore/Ice");
            newResult[4].Amount = 10;
            definition.Results = newResult;
            // Results
            definition.Results[0].Id = MyDefinitionId.Parse("MyObjectBuilder_Ore/Stone");
            definition.Results[0].Amount = 100;
            definition.Results[1].Id = MyDefinitionId.Parse("MyObjectBuilder_Ore/Gold");
            definition.Results[1].Amount = 1;
            definition.Results[2].Id = MyDefinitionId.Parse("MyObjectBuilder_Ore/Uranium");
            definition.Results[2].Amount = 1;
            definition.Results[3].Id = MyDefinitionId.Parse("MyObjectBuilder_Ore/Iron");
            definition.Results[3].Amount = 1;



        }

    }
}