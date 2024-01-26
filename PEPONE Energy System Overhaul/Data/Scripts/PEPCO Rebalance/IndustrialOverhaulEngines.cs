using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

namespace PEPCO.IndustrialOverhaulEnginesRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase

    {

        public override void LoadData()
        {
            // Gasoline
            LGIndustiralOverhaulGasolineEngines(new MyDefinitionId(typeof(MyObjectBuilder_HydrogenEngine), "LargeGasolineEngine"));
            SGIndustiralOverhaulGasolineEngines(new MyDefinitionId(typeof(MyObjectBuilder_HydrogenEngine), "SmallGasolineEngine"));

            // Steam
            LGIndustiralOverhaulSteamEngines(new MyDefinitionId(typeof(MyObjectBuilder_HydrogenEngine), "SteamTurbine"));
            LGIndustiralOverhaulSteamEngines(new MyDefinitionId(typeof(MyObjectBuilder_HydrogenEngine), "SteamTurbineMirrored"));

            // Fusion Reactor
            LGIndustiralOverhaulFusionReactorEngines(new MyDefinitionId(typeof(MyObjectBuilder_HydrogenEngine), "FusionReactor"));


        }
        // Gasoline
        private void LGIndustiralOverhaulGasolineEngines(MyDefinitionId definitionId)
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
        private void SGIndustiralOverhaulGasolineEngines(MyDefinitionId definitionId)
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
        // Steam
        private void LGIndustiralOverhaulSteamEngines(MyDefinitionId definitionId)
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
        // Fusion
        private void LGIndustiralOverhaulFusionReactorEngines(MyDefinitionId definitionId)
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
    }
}