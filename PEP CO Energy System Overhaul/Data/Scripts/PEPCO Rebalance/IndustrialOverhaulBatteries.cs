using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO_IndustrialOverhaulBatteriesRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class GasChanges_Session : MySessionComponentBase
    {
        public override void LoadData()
        {

            LGAcidIOBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeBlockAcidBatteryBlock"));
            SGAcidIOBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallBlockAcidBatteryBlock"));
            SGAcidIOSmallBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallBlockSmallAcidBatteryBlock"));

            LGAlkalineIOBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeBlockAlkalineBatteryBlock"));
            SGAlkalineIOSmallBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallBlockSmallAlkalineBatteryBlock"));

            LGIOCapacitor(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeCapacitor"));
            SGIOCapacitor(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallCapacitor"));

            LGIOElectronMatrix(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeElectronMatrix"));
            SGIOElectronMatrix(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallElectronMatrix"));



        }

        private void LGAcidIOBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //1.5
            definition.MaxPowerOutput *= 1; //6
            definition.RequiredPowerInput *= 1; //6
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGAcidIOBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //0.5
            definition.MaxPowerOutput *= 1; //2
            definition.RequiredPowerInput *= 1; //2
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGAcidIOSmallBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //0.05
            definition.MaxPowerOutput *= 1; //0.1
            definition.RequiredPowerInput *= 1; //0.1
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void LGAlkalineIOBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //1
            definition.MaxPowerOutput *= 1; //6
            definition.RequiredPowerInput *= 1; //0
            definition.InitialStoredPowerRatio = 100;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGAlkalineIOSmallBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //0.02
            definition.MaxPowerOutput *= 1; //0.1
            definition.RequiredPowerInput *= 1; //0
            definition.InitialStoredPowerRatio = 100;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void LGIOCapacitor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //0.1
            definition.MaxPowerOutput *= 1; //250
            definition.RequiredPowerInput *= 1; //50
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGIOCapacitor(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //0.01
            definition.MaxPowerOutput *= 1; //25
            definition.RequiredPowerInput *= 1; //5
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void LGIOElectronMatrix(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //250
            definition.MaxPowerOutput *= 1; //50
            definition.RequiredPowerInput *= 1; //50
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGIOElectronMatrix(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //25
            definition.MaxPowerOutput *= 1; //5
            definition.RequiredPowerInput *= 1; //5
            definition.InitialStoredPowerRatio = 0;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
    }
}