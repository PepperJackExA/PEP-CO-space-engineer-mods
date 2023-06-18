using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

namespace PEPCO.VanillaThrustersRebalance
{
    
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase
    {
        public const float ThrustLGlargeHydrogen = ThrustLGsmallHydrogen/0.15f;
        public const float ThrustLGsmallHydrogen = 1080000;
        public const float ThrustSGlargeHydrogen = ThrustSGsmallHydrogen/0.15f;
        public const float ThrustSGsmallHydrogen = 98400;


        public const float ThrustLGlargeAtmo = ThrustLGlargeHydrogen / 2;
        public const float ThrustLGsmallAtmo = ThrustLGsmallHydrogen / 2;
        public const float ThrustSGlargeAtmo = ThrustSGlargeHydrogen / 2;
        public const float ThrustSGsmallAtmo = ThrustSGsmallHydrogen / 2;


        public const float ThrustLGlargeIon = ThrustLGlargeAtmo / 2;
        public const float ThrustLGsmallIon = ThrustLGsmallAtmo / 2;
        public const float ThrustSGlargeIon = ThrustSGlargeAtmo / 2;
        public const float ThrustSGsmallIon = ThrustSGsmallAtmo / 2;


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
            var gas = MyDefinitionManager.Static.GetDefinition(definitionId) as MyGasProperties;

            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustLGlargeHydrogen; //7200 KN
            definition.MaxPowerConsumption *= 1; //7.5
            definition.FuelConverter.Efficiency *= 1; //2
            definition.FuelConverter.FuelId.SubtypeId = "Hydrogen";
            MyGasProperties fuelDef;
            MyDefinitionManager.Static.TryGetDefinition(definition.FuelConverter.FuelId, out fuelDef);
            float eff = (fuelDef.EnergyDensity * definition.FuelConverter.Efficiency);
            float maxFuelUsage = definition.MaxPowerConsumption / eff;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Energy Denisty: " + fuelDef.EnergyDensity + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Fuel Consumption: " + (maxFuelUsage / 1000).ToString("0.##") + " kL/s " + "\n" +
                                  "Thrust to fuel Ratio: " + (definition.ForceMagnitude / maxFuelUsage / 1000).ToString("0.##") + " N/L";
        }
        private void LGVanillaSmallHydrogenThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;

            definition.PCU *= 1; //15
            definition.ForceMagnitude = ThrustLGsmallHydrogen; //1080 KN
            definition.MaxPowerConsumption  *= 1; //1.25
            definition.FuelConverter.Efficiency *= 1; //2
            definition.FuelConverter.FuelId.SubtypeId = "Hydrogen";
            MyGasProperties fuelDef;
            MyDefinitionManager.Static.TryGetDefinition(definition.FuelConverter.FuelId, out fuelDef);
            float eff = (fuelDef.EnergyDensity * definition.FuelConverter.Efficiency);
            float maxFuelUsage = definition.MaxPowerConsumption / eff;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Energy Denisty: " + fuelDef.EnergyDensity + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Fuel Consumption: " + (maxFuelUsage / 1000).ToString("0.##") + " kL/s " + "\n" +
                                  "Thrust to fuel Ratio: " + (definition.ForceMagnitude / maxFuelUsage / 1000).ToString("0.##") + " N/L";
        }
        private void SGVanillaLargeHydrogenThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;

            definition.PCU *= 1; //15
            definition.ForceMagnitude = ThrustSGlargeHydrogen; //480 KN
            definition.MaxPowerConsumption *= 1; //0.6
            definition.FuelConverter.Efficiency *= 1; //2
            definition.FuelConverter.FuelId.SubtypeId = "Hydrogen";
            MyGasProperties fuelDef;
            MyDefinitionManager.Static.TryGetDefinition(definition.FuelConverter.FuelId, out fuelDef);
            float eff = (fuelDef.EnergyDensity * definition.FuelConverter.Efficiency);
            float maxFuelUsage = definition.MaxPowerConsumption / eff;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Energy Denisty: " + fuelDef.EnergyDensity + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Fuel Consumption: " + (maxFuelUsage / 1000).ToString("0.##") + " kL/s " + "\n" +
                                  "Thrust to fuel Ratio: " + (definition.ForceMagnitude / maxFuelUsage / 1000).ToString("0.##") + " N/L";
        }
        private void SGVanillaSmallHydrogenThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;

            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustSGsmallHydrogen; //98.4 KN
            definition.MaxPowerConsumption *= 1; //0.125
            definition.FuelConverter.Efficiency *= 1; //2
            definition.FuelConverter.FuelId.SubtypeId = "Hydrogen";
            MyGasProperties fuelDef;
            MyDefinitionManager.Static.TryGetDefinition(definition.FuelConverter.FuelId, out fuelDef);
            float eff = (fuelDef.EnergyDensity * definition.FuelConverter.Efficiency);
            float maxFuelUsage = definition.MaxPowerConsumption / eff;
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + definition.FuelConverter.FuelId.SubtypeId + "\n" +
                                  "Energy Denisty: " + fuelDef.EnergyDensity + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Fuel Consumption: " + (maxFuelUsage / 1000).ToString("0.##") + " kL/s " + "\n" +
                                  "Thrust to fuel Ratio: " + (definition.ForceMagnitude / maxFuelUsage / 1000).ToString("0.##") + " N/L";
        }
        private void LGVanillaLargeAtmoThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;

            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustLGlargeAtmo; // 6580 KN
            definition.MaxPowerConsumption *= 1; // 16.8
            definition.FuelConverter.Efficiency *= 1; // 1
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + "Electricity" + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption.ToString("0.##") + " MW" + "\n" +
                                  "Thrust/Power Ratio: " + (definition.ForceMagnitude / definition.MaxPowerConsumption / 1000).ToString("0.##") + " N/MW";
        }
        private void LGVanillaSmallAtmoThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            
            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustLGsmallAtmo; // 648 KN
            definition.MaxPowerConsumption *= 1; // 2.4
            definition.FuelConverter.Efficiency *= 1; // 1
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + "Electricity" + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption.ToString("0.##") + " MW" + "\n" +
                                  "Thrust/Power Ratio: " + (definition.ForceMagnitude / definition.MaxPowerConsumption / 1000).ToString("0.##") + " N/MW";
        }
        private void SGVanillaLargeAtmoThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            
            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustSGlargeAtmo; // 576 KN
            definition.MaxPowerConsumption *= 1; // 2.4
            definition.FuelConverter.Efficiency *= 1; // 1
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + "Electricity" + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption.ToString("0.##") + " MW" + "\n" +
                                  "Thrust/Power Ratio: " + (definition.ForceMagnitude / definition.MaxPowerConsumption / 1000).ToString("0.##") + " N/MW";
        }
        private void SGVanillaSmallAtmoThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            
            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustSGsmallAtmo; // 96 KN
            definition.MaxPowerConsumption *= 1; // 0.6
            definition.FuelConverter.Efficiency *= 1; // 1
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + "Electricity" + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption.ToString("0.##") + " MW" + "\n" +
                                  "Thrust/Power Ratio: " + (definition.ForceMagnitude / definition.MaxPowerConsumption / 1000).ToString("0.##") + " N/MW";
        }
        private void LGVanillaLargeIonThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            
            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustLGlargeIon; // 4320 KN
            definition.MaxPowerConsumption *= 1; // 33.6
            definition.FuelConverter.Efficiency *= 1; // 1 
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + "Electricity" + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption.ToString("0.##") + " MW" + "\n" +
                                  "Thrust/Power Ratio: " + (definition.ForceMagnitude / definition.MaxPowerConsumption / 1000).ToString("0.##") + " N/MW";
        }
        private void LGVanillaSmallIonThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            
            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustLGsmallIon; // 345.6 KN
            definition.MaxPowerConsumption *= 1; // 3.36
            definition.FuelConverter.Efficiency *= 1; // 1 
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + "Electricity" + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption.ToString("0.##") + " MW" + "\n" +
                                  "Thrust/Power Ratio: " + (definition.ForceMagnitude / definition.MaxPowerConsumption / 1000).ToString("0.##") + " N/MW";
        }
        private void SGVanillaLargeIonThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            
            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustSGlargeIon; // 172.8 KN
            definition.MaxPowerConsumption *= 1; // 2.4
            definition.FuelConverter.Efficiency = 1; // 1 
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + "Electricity" + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption.ToString("0.##") + " MW" + "\n" +
                                  "Thrust/Power Ratio: " + (definition.ForceMagnitude / definition.MaxPowerConsumption / 1000).ToString("0.##") + " N/MW";
        }
        private void SGVanillaSmallIonThrust(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyThrustDefinition;
            
            //definition.DisplayNameEnum = null;
            //definition.DisplayNameString = "Large Oxygen Ion Thruster";
            // definition.FuelConverter = new MyFuelConverterInfo
            //{
            //    FuelId = MyDefinitionId.Parse("GasProperties/Oxygen")
            //};

            definition.PCU *= 1; // 15
            definition.ForceMagnitude = ThrustSGsmallIon; // 14.4 KN
            definition.MaxPowerConsumption *= 1; // 0.2
            definition.FuelConverter.Efficiency *= 1; // 1 
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Uses: " + "Electricity" + "\n" +
                                  "Max Thrust Force: " + (definition.ForceMagnitude / 1000).ToString("0.##") + " KN" + "\n" +
                                  "Max Power Consumption: " + definition.MaxPowerConsumption.ToString("0.##") + " MW" + "\n" +
                                  "Thrust/Power Ratio: " + (definition.ForceMagnitude / definition.MaxPowerConsumption / 1000).ToString("0.##") + " N/MW";
        }
    }
}