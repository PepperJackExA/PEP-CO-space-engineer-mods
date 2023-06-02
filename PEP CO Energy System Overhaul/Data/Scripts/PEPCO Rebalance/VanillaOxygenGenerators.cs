using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using System.Collections.Generic;

namespace PEPCO.VanillaOxygenGeneratorRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase
    {
        public override void LoadData()
        {

            LGVanillaOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), ""));
            SGVanillaOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "OxygenGeneratorSmall"));
            
        }
        private void LGVanillaOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 25
            definition.OperationalPowerConsumption *= 1; // 0.5 MW

            string gas1 = "Oxygen";
            string gas2 = "Hydrogen";
            int numberofgases = 2;

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = 1,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = 1,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas2),

                },
            };
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + gas2 + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond*numberofgases + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW/s";
        }
        private void SGVanillaOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 5
            definition.OperationalPowerConsumption *= 1; // 0.1 MW
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: Hydrogen and Oxygen" + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW/s";
        }

    }
}