using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO_VanillaThrustersRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class GasChanges_Session : MySessionComponentBase
    {
        public override void LoadData()
        {
            //Vanilla Hydrogen
            LGVanillaLargeHydrogenThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockLargeHydrogenThrust"));
            LGVanillaLargeHydrogenThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockLargeHydrogenThrustIndustrial"));

            LGVanillaSmallHydrogenThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockSmallHydrogenThrust"));
            LGVanillaSmallHydrogenThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockSmallHydrogenThrustIndustrial"));

            SGVanillaLargeHydrogenThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockLargeHydrogenThrust"));
            SGVanillaLargeHydrogenThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockLargeHydrogenThrustIndustrial"));

            SGVanillaSmallHydrogenThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockSmallHydrogenThrust"));
            SGVanillaSmallHydrogenThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockSmallHydrogenThrustIndustrial"));

            //Vanilla Atmo

            LGVanillaLargeAtmoThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockLargeAtmosphericThrust"));
            LGVanillaLargeAtmoThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockLargeAtmosphericThrustSciFi"));

            LGVanillaSmallAtmoThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockSmallAtmosphericThrust"));
            LGVanillaSmallAtmoThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockSmallAtmosphericThrustSciFi"));

            SGVanillaLargeAtmoThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockLargeAtmosphericThrust"));
            SGVanillaLargeAtmoThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockLargeAtmosphericThrustSciFi"));

            SGVanillaSmallAtmoThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockSmallAtmosphericThrust"));
            SGVanillaSmallAtmoThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockSmallAtmosphericThrustSciFi"));

            //Ion Thrusters
            LGVanillaLargeIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockLargeThrust"));
            LGVanillaLargeIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockLargeThrustSciFi"));
            LGVanillaLargeIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockLargeModularThruster"));

            LGVanillaSmallIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockSmallThrust"));
            LGVanillaSmallIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockSmallThrustSciFi"));
            LGVanillaSmallIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "LargeBlockSmallModularThruster"));

            SGVanillaLargeIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockLargeThrust"));
            SGVanillaLargeIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockLargeThrustSciFi"));
            SGVanillaLargeIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockLargeModularThruster"));

            SGVanillaSmallIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockSmallThrust"));
            SGVanillaSmallIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockSmallThrustSciFi"));
            SGVanillaSmallIonThrust(new MyDefinitionId(typeof(MyObjectBuilder_Thrust), "SmallBlockSmallModularThruster"));
        }
        private void LGVanillaLargeHydrogenThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; //7200 KN
            definition.MaxPowerConsumption *= 1; //7.5
            definition.FuelConverter.Efficiency *= 1; //2
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void LGVanillaSmallHydrogenThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; //15
            definition.ForceMagnitude *= 1; //1080 KN
            definition.MaxPowerConsumption  *= 1; //1.25
            definition.FuelConverter.Efficiency *= 1; //2
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void SGVanillaLargeHydrogenThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; //15
            definition.ForceMagnitude *= 1; //480 KN
            definition.MaxPowerConsumption *= 1; //0.6
            definition.FuelConverter.Efficiency *= 1; //2
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void SGVanillaSmallHydrogenThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; //98.4 KN
            definition.MaxPowerConsumption *= 1; //0.125
            definition.FuelConverter.Efficiency *= 1; //2
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void LGVanillaLargeAtmoThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; // 6580 KN
            definition.MaxPowerConsumption *= 1; // 16.8
            definition.FuelConverter.Efficiency *= 1; // 1
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void LGVanillaSmallAtmoThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; // 648 KN
            definition.MaxPowerConsumption *= 1; // 2.4
            definition.FuelConverter.Efficiency *= 1; // 1
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void SGVanillaLargeAtmoThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; // 576 KN
            definition.MaxPowerConsumption *= 1; // 2.4
            definition.FuelConverter.Efficiency *= 1; // 1
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void SGVanillaSmallAtmoThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; // 96 KN
            definition.MaxPowerConsumption *= 1; // 0.6
            definition.FuelConverter.Efficiency *= 1; // 1
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void LGVanillaLargeIonThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; // 4320 KN
            definition.MaxPowerConsumption *= 1; // 33.6
            definition.FuelConverter.Efficiency *= 1; // 1 
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void LGVanillaSmallIonThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; // 345.6 KN
            definition.MaxPowerConsumption *= 1; // 3.36
            definition.FuelConverter.Efficiency *= 1; // 1 
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void SGVanillaLargeIonThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; // 172.8 KN
            definition.MaxPowerConsumption *= 1; // 2.4
            definition.FuelConverter.Efficiency *= 1; // 1 
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
        private void SGVanillaSmallIonThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            definition.PCU *= 1; // 15
            definition.ForceMagnitude *= 1; // 14.4 KN
            definition.MaxPowerConsumption *= 1; // 0.2
            definition.FuelConverter.Efficiency *= 1; // 1 
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Max Thrust Force: " + definition.ForceMagnitude / 1000 + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption + " " + "\n" +
                                  "Fuel Efficiency: " + definition.FuelConverter.Efficiency;
        }
    }
}