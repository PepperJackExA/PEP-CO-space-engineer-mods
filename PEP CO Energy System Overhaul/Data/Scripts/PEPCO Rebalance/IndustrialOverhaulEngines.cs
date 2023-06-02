using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

namespace PEPCO_IndustrialOverhaulEnginesRebalance
{

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class GasChanges_Session : MySessionComponentBase

    {
        public override void LoadData()
        {
            
            LGGasolineIOEngines(new MyDefinitionId(typeof(MyObjectBuilder_HydrogenEngine), "LargeGasolineEngine"));
            SGGasolineIOEngines(new MyDefinitionId(typeof(MyObjectBuilder_HydrogenEngine), "SmallGasolineEngine"));

        }

        private void LGGasolineIOEngines(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyHydrogenEngineDefinition;
            definition.MaxPowerOutput *= 1; //15
            definition.FuelProductionToCapacityMultiplier *= 1; //10
            definition.FuelCapacity *=  1; //10
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + definition.Fuel.FuelId.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Fuel Pro to Cap Mult: " + definition.FuelProductionToCapacityMultiplier + "\n" +
                                        "Max Fuel Capacity: " + definition.FuelCapacity + "L";
        }
        private void SGGasolineIOEngines(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyHydrogenEngineDefinition;
            definition.MaxPowerOutput *= 1; //1.5
            definition.FuelProductionToCapacityMultiplier *= 1; //10
            definition.FuelCapacity *= 1; //10
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + definition.Fuel.FuelId.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Fuel Pro to Cap Mult: " + definition.FuelProductionToCapacityMultiplier + "\n" +
                                        "Max Fuel Capacity: " + definition.FuelCapacity + "L";
        }

    }
}