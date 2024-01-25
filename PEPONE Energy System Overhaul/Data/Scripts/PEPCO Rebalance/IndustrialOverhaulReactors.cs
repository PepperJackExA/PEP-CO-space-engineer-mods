using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO.IndustrialOverhaulReactorRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase

    {

        public override void LoadData()
        {

            LGIndustrialOverhaulLargeRTGReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "LargeRTG"));
            SGIndustrialOverhaulSmallRTGReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "SmallRTG"));

            LGIndustrialOverhaulLargeCompactReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "LargeCompactReactor"));
            SGIndustrialOverhaulSmallCompactReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "SmallCompactReactor"));


        }

        private void LGIndustrialOverhaulLargeRTGReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput *= 1; // 300 MW
            definition.FuelProductionToCapacityMultiplier *= 1;
            MyReactorDefinition.FuelInfo fueldef = definition.FuelInfos[0];
            float perSec = fueldef.ConsumptionPerSecond_Items;

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + fueldef.FuelDefinition.Id.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + " MW" + "\n" +
                                        "Fuel Use: " + (perSec * 1000).ToString("0.##") + " grams/s" + "\n" +
                                        "Fuel Multiplier: " + definition.FuelProductionToCapacityMultiplier + "\n" +
                                        "Output per kg: " + ((1000 / (perSec * 1000)) * definition.MaxPowerOutput).ToString("0.##") + " MW";
        }
        private void SGIndustrialOverhaulSmallRTGReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput *= 1; // 15 MW
            definition.FuelProductionToCapacityMultiplier *= 1;
            MyReactorDefinition.FuelInfo fueldef = definition.FuelInfos[0];
            float perSec = fueldef.ConsumptionPerSecond_Items;

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + fueldef.FuelDefinition.Id.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + " MW" + "\n" +
                                        "Fuel Use: " + (perSec * 1000).ToString("0.##") + " grams/s" + "\n" +
                                        "Fuel Multiplier: " + definition.FuelProductionToCapacityMultiplier + "\n" +
                                        "Output per kg: " + ((1000 / (perSec * 1000)) * definition.MaxPowerOutput).ToString("0.##") + " MW";
        }
        private void LGIndustrialOverhaulLargeCompactReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput *= 1; // 14.75 MW
            definition.FuelProductionToCapacityMultiplier *= 1;
            MyReactorDefinition.FuelInfo fueldef = definition.FuelInfos[0];
            float perSec = fueldef.ConsumptionPerSecond_Items;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + fueldef.FuelDefinition.Id.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + " MW" + "\n" +
                                        "Fuel Use: " + (perSec * 1000).ToString("0.##") + " grams/s" + "\n" +
                                        "Fuel Multiplier: " + definition.FuelProductionToCapacityMultiplier + "\n" +
                                        "Output per kg: " + ((1000 / (perSec * 1000)) * definition.MaxPowerOutput).ToString("0.##") + " MW";
        }
        private void SGIndustrialOverhaulSmallCompactReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput *= 1; // 0.5 MW
            definition.FuelProductionToCapacityMultiplier *= 1;// 3600
            MyReactorDefinition.FuelInfo fueldef = definition.FuelInfos[0];
            float perSec = fueldef.ConsumptionPerSecond_Items;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + fueldef.FuelDefinition.Id.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + " MW" + "\n" +
                                        "Fuel Use: " + (perSec * 1000).ToString("0.##") + " grams/s" + "\n" +
                                        "Fuel Multiplier: " + definition.FuelProductionToCapacityMultiplier + "\n" +
                                        "Output per kg: " + ((1000 / (perSec * 1000)) * definition.MaxPowerOutput).ToString("0.##") + " MW";
        }
    }
}