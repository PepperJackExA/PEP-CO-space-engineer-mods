using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;

namespace PEPCO.VanillaOxygenGeneratorRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase
    {
        public override void LoadData()
        {
            // H2O2 Generators
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
            int gas1ratio = 10; //10

            string gas2 = "Hydrogen";
            int gas2ratio = 20; //20

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas2ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas2),

                },
            };

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + gas2 + " /1 ice" + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond * 2 + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n" +
                                  gas2 + " Output/s: " + (definition.IceConsumptionPerSecond * gas2ratio).ToString("0.##") + "\n";

        }
        private void SGVanillaOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 5
            definition.OperationalPowerConsumption *= 1; // 0.1 MW

            string gas1 = "Oxygen";
            int gas1ratio = 10; //10

            string gas2 = "Hydrogen";
            int gas2ratio = 20; //20

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas2ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas2),

                },
            };

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + gas2 + " /1 ice" + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond * 2 + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n" +
                                  gas2 + " Output/s: " + (definition.IceConsumptionPerSecond * gas2ratio).ToString("0.##") + "\n";
        }

    }
}