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
            definition.MaxPowerOutput *= 1; // 300 MW
            definition.FuelProductionToCapacityMultiplier *= 1;// 3600
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Fuel Pro to Cap Mult: " + definition.FuelProductionToCapacityMultiplier;
        }
        private void LGVanillaSmallReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput *= 1; // 15 MW
            definition.FuelProductionToCapacityMultiplier *= 1;// 3600
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Fuel Pro to Cap Mult: " + definition.FuelProductionToCapacityMultiplier;
        }
        private void SGVanillaLargeReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput *= 1; // 14.75 MW
            definition.FuelProductionToCapacityMultiplier *= 1;// 3600
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Fuel Pro to Cap Mult: " + definition.FuelProductionToCapacityMultiplier;
        }
        private void SGVanillaSmallReactor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyReactorDefinition;
            definition.MaxPowerOutput *= 1; // 0.5 MW
            definition.FuelProductionToCapacityMultiplier *= 1;// 3600
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Fuel Type: " + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Fuel Pro to Cap Mult: " + definition.FuelProductionToCapacityMultiplier;
        }
    }
}