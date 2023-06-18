using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

namespace PEPCO.VanillaEnginesRebalance
{

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase

    {
        
        public override void LoadData()
        {

            LGVanillaEngines(new MyDefinitionId(typeof(MyObjectBuilder_HydrogenEngine), "LargeHydrogenEngine"));
            SGVanillaEngines(new MyDefinitionId(typeof(MyObjectBuilder_HydrogenEngine), "SmallHydrogenEngine"));
         
        }

        private void LGVanillaEngines(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyHydrogenEngineDefinition;
            definition.MaxPowerOutput *= 1; //5
            definition.FuelProductionToCapacityMultiplier *= 1;//0.025
            definition.FuelCapacity *= 1; //100
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + definition.Fuel.FuelId.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Fuel Pro to Cap Mult: " + definition.FuelProductionToCapacityMultiplier + "\n" +
                                        "Max Fuel Capacity: " + definition.FuelCapacity + "L";
        }
        private void SGVanillaEngines(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyHydrogenEngineDefinition;
            definition.MaxPowerOutput *= 1; //0.5
            definition.FuelProductionToCapacityMultiplier *= 1; //0.025
            definition.FuelCapacity *= 1; //100
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + definition.Fuel.FuelId.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Fuel Pro to Cap Mult: " + definition.FuelProductionToCapacityMultiplier + "\n" +
                                        "Max Fuel Capacity: " + definition.FuelCapacity + "L";
        }

    }
}