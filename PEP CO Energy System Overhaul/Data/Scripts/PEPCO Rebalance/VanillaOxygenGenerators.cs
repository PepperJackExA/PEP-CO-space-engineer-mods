using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO_OxygenGeneratorRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class GasChanges_Session : MySessionComponentBase
    {
        public override void LoadData()
        {

            LGVanillaOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), ""));
            SGVanillaOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "OxygenGeneratorSmall"));

        }

        private void LGVanillaOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 15
            definition.IceConsumptionPerSecond *= 1; // 50 
            definition.OperationalPowerConsumption *= 1; // 1
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.ProducedGases + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond + " / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW/s";
        }
        private void SGVanillaOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 15
            definition.IceConsumptionPerSecond *= 1; // 10
            definition.OperationalPowerConsumption *= 1; // 0.25
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.ProducedGases + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond + " / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW/s";
        }

    }
}