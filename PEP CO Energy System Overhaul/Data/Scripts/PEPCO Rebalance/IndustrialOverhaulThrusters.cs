using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO_IndustrialOverhaulThrustersRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class GasChanges_Session : MySessionComponentBase
    {
        public override void LoadData()
        {
            //IndustrialOverhaul Rocket
            LGIndustrialOverhaulRocketThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockLargeRocketThrust"));

            LGIndustrialOverhaulSmallRocketThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockSmallRocketThrust"));

            SGIndustrialOverhaulLargeRocketThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockLargeRocketThrust"));

            SGIndustrialOverhaulSmallRocketThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockSmallRocketThrust"));

            //IndustrialOverhaul Fusion

            LGIndustrialOverhaulFusionThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "FusionThruster"));

        }
        private void LGIndustrialOverhaulRocketThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; //
            definition.ForceMagnitude *= 1; //
            definition.MaxPowerConsumption *= 1; //
            definition.FuelConverter.Efficiency *= 1; //
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void LGIndustrialOverhaulSmallRocketThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; //
            definition.ForceMagnitude *= 1; //
            definition.MaxPowerConsumption  *= 1; //
            definition.FuelConverter.Efficiency *= 1; //
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void SGIndustrialOverhaulLargeRocketThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; //
            definition.ForceMagnitude *= 1; //
            definition.MaxPowerConsumption *= 1; //
            definition.FuelConverter.Efficiency *= 1; //
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void SGIndustrialOverhaulSmallRocketThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; //
            definition.ForceMagnitude *= 1; //
            definition.MaxPowerConsumption *= 1; //
            definition.FuelConverter.Efficiency *= 1; //
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void LGIndustrialOverhaulFusionThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; //
            definition.ForceMagnitude *= 1; //
            definition.MaxPowerConsumption *= 1; //
            definition.FuelConverter.Efficiency *= 1; //
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
       
    }
}