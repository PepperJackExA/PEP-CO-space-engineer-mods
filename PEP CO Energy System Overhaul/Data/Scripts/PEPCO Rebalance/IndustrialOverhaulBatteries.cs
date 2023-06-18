using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO.IndustrialOverhaulBatteriesRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase
    {
        public override void LoadData()
        {
            // Alkaline
            LGIndustiralOverhaulAlkalineBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeBlockAlkalineBatteryBlock"));

            SGVIndustrialOverhaulSmallAlkalineBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallBlockSmallAlkalineBatteryBlock"));

            // Acid
            LGIndustiralOverhaulAcidBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeBlockAcidBatteryBlock"));

            SGVIndustrialOverhaulAcidBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallBlockAcidBatteryBlock"));
            
            SGVIndustrialOverhaulSmallAcidBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallBlockSmallAcidBatteryBlock"));

            // Capacitor

            LGIndustiralOverhaulCapacitorBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeCapacitor"));

            SGVIndustrialOverhaulCapacitorBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallCapacitor"));

            // Electron-Matrix Bank

            LGIndustiralOverhaulElectronMatrixBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "LargeElectronMatrix"));

            SGVIndustrialOverhaulElectronMatrixBattery(new MyDefinitionId(typeof(MyObjectBuilder_BatteryBlock), "SmallElectronMatrix"));
        }

        // Alkaline
        private void LGIndustiralOverhaulAlkalineBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //3
            definition.MaxPowerOutput *= 1; //12
            definition.RequiredPowerInput *= 1; //12
            definition.InitialStoredPowerRatio *= 1;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGVIndustrialOverhaulSmallAlkalineBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //0.05
            definition.MaxPowerOutput *= 1; //0.2
            definition.RequiredPowerInput *= 1; //0.2
            definition.InitialStoredPowerRatio *= 1;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";

        }

        // Acid
        private void LGIndustiralOverhaulAcidBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //3
            definition.MaxPowerOutput *= 1; //12
            definition.RequiredPowerInput *= 1; //12
            definition.InitialStoredPowerRatio *= 1;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGVIndustrialOverhaulAcidBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //1
            definition.MaxPowerOutput *= 1; //4
            definition.RequiredPowerInput *= 1; //4
            definition.InitialStoredPowerRatio *= 1;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGVIndustrialOverhaulSmallAcidBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //0.05
            definition.MaxPowerOutput *= 1; //0.2
            definition.RequiredPowerInput *= 1; //0.2
            definition.InitialStoredPowerRatio *= 1;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";

        }
        // Capacitor
        private void LGIndustiralOverhaulCapacitorBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //3
            definition.MaxPowerOutput *= 1; //12
            definition.RequiredPowerInput *= 1; //12
            definition.InitialStoredPowerRatio *= 1;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGVIndustrialOverhaulCapacitorBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //0.05
            definition.MaxPowerOutput *= 1; //0.2
            definition.RequiredPowerInput *= 1; //0.2
            definition.InitialStoredPowerRatio *= 1;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";

        }
        // ElectronMatrix
        private void LGIndustiralOverhaulElectronMatrixBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //3
            definition.MaxPowerOutput *= 1; //12
            definition.RequiredPowerInput *= 1; //12
            definition.InitialStoredPowerRatio *= 1;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";
        }
        private void SGVIndustrialOverhaulElectronMatrixBattery(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyBatteryBlockDefinition;
            definition.PCU *= 1; //15
            definition.MaxStoredPower *= 1; //0.05
            definition.MaxPowerOutput *= 1; //0.2
            definition.RequiredPowerInput *= 1; //0.2
            definition.InitialStoredPowerRatio *= 1;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Maxed Stored Power: " + definition.MaxStoredPower + "MWh" + "\n" +
                                        "Maxed Power Output: " + definition.MaxPowerOutput + "MW" + "\n" +
                                        "Max Input Power: " + definition.RequiredPowerInput + "MW";

        }
    }
}