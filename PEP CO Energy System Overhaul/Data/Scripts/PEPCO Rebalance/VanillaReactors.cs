using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO.VanillaReactorRebalance
{

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase

    {
        public const float ReactorLGlargeUranium = ReactorLGsmallUranium / 0.05f; //300
        public const float ReactorLGsmallUranium = 15; //15
        public const float ReactorSGlargeUranium = ReactorSGsmallUranium / 0.0338983050847458f; //14.75
        public const float ReactorSGsmallUranium = 0.5f; //0.5

        public override void LoadData()
        {

            LGVanillaLargeReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "LargeBlockLargeGenerator"));
            LGVanillaLargeReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "LargeBlockLargeGeneratorWarfare2"));

            LGVanillaSmallReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "LargeBlockSmallGenerator"));
            LGVanillaSmallReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "LargeBlockSmallGeneratorWarfare2"));

            SGVanillaLargeReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "SmallBlockLargeGenerator"));
            SGVanillaLargeReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "SmallBlockLargeGeneratorWarfare2"));

            SGVanillaSmallReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "SmallBlockSmallGenerator"));
            SGVanillaSmallReactor(new MyDefinitionId(typeof(MyObjectBuilder_Reactor), "SmallBlockSmallGeneratorWarfare2"));


        }

        private void LGVanillaLargeReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput = ReactorLGlargeUranium; // 300 MW
            MyReactorDefinition.FuelInfo fueldef = definition.FuelInfos[0];
            float perSec = fueldef.ConsumptionPerSecond_Items;

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + fueldef.FuelDefinition.Id.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + " MW" + "\n" +
                                        "Fuel Use: " + (perSec * 1000).ToString("0.##") + " grams/s" + "\n" +
                                        "Output per kg: " + ((1000 / (perSec * 1000)) * definition.MaxPowerOutput).ToString("0.##") + " MW";
        }
        private void LGVanillaSmallReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput = ReactorLGsmallUranium; // 15 MW
            MyReactorDefinition.FuelInfo fueldef = definition.FuelInfos[0];
            float perSec = fueldef.ConsumptionPerSecond_Items;

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + fueldef.FuelDefinition.Id.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + " MW" + "\n" +
                                        "Fuel Use: " + (perSec * 1000).ToString("0.##") + " grams/s" + "\n" +
                                        "Output per kg: " + ((1000 / (perSec * 1000)) * definition.MaxPowerOutput).ToString("0.##") + " MW";
        }
        private void SGVanillaLargeReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput = ReactorSGlargeUranium; // 14.75 MW
            MyReactorDefinition.FuelInfo fueldef = definition.FuelInfos[0];
            float perSec = fueldef.ConsumptionPerSecond_Items;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + fueldef.FuelDefinition.Id.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + " MW" + "\n" +
                                        "Fuel Use: " + (perSec * 1000).ToString("0.##") + " grams/s" + "\n" +
                                        "Output per kg: " + ((1000 / (perSec * 1000)) * definition.MaxPowerOutput).ToString("0.##") + " MW";
        }
        private void SGVanillaSmallReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput = ReactorSGsmallUranium; // 0.5 MW
            definition.FuelProductionToCapacityMultiplier *= 1;// 3600
            MyReactorDefinition.FuelInfo fueldef = definition.FuelInfos[0];
            float perSec = fueldef.ConsumptionPerSecond_Items;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + fueldef.FuelDefinition.Id.SubtypeId + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + " MW" + "\n" +
                                        "Fuel Use: " + (perSec * 1000).ToString("0.##") + " grams/s" + "\n" +
                                        "Output per kg: " + ((1000 / (perSec * 1000)) * definition.MaxPowerOutput).ToString("0.##") + " MW";
        }
    }
}