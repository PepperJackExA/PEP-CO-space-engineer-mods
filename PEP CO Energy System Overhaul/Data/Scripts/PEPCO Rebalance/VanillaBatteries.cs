using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO_VanillaBatteriesRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class GasChanges_Session : MySessionComponentBase
    {
        public override void LoadData()
        {

            LGVanillaBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeBlockBatteryBlock"));
            LGVanillaBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeBlockBatteryBlockWarfare2"));
            SGVanillaBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallBlockBatteryBlock"));
            SGVanillaBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallBlockBatteryBlockWarfare2"));
            SGVanillaSmallBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallBlockSmallBatteryBlock"));
           
        }

        private void LGVanillaBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower = 3; //3
            definition.MaxPowerOutput = 1.2f; //12
            definition.RequiredPowerInput = 1.2f; //12
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGVanillaBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower = 1; //1
            definition.MaxPowerOutput = 0.4f; //4
            definition.RequiredPowerInput = 0.4f; //4
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGVanillaSmallBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower = 0.05f; //0.05
            definition.MaxPowerOutput = 0.02f; //0.2
            definition.RequiredPowerInput = 0.02f; //0.2
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
                
    }
}