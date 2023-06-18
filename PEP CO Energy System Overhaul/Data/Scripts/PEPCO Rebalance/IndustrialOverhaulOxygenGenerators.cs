using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using System.Collections.Generic;

namespace PEPCO.IndustrialOverhaulOxygenGeneratorRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase
    {
        public override void LoadData()
        {
            // Geothermal Wellhead
            LGIndustrialOverhaulGeothermalOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "GeothermalWellHead"));
            // Gasoline Refinery
            LGIndustiralOverhaulGasolineRefineryOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "GasolineRefinery"));
            // Rocket Fuel Refinery
            LGIndustiralOverhaulRocketFuelRefineryOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "RocketFuelRefinery"));
            // Coal Boiler
            LGIndustiralOverhaulCoalFurnaceOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "CoalFurnace"));
            // Nuclear Reactor
            LGIndustiralOverhaulNuclearReactorOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "NuclearReactor"));
            // Fast Neutron Reactor
            LGIndustiralOverhaulFastNeutronReactorOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "FastNeutronReactor"));
            // Deuterium Extractor
            LGIndustiralOverhaulDeuteriumExtractorOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "DeuteriumExtractor"));
            // Deuterium Ramscoop
            LGIndustiralOverhaulDeuteriumRamscoopOxygenGenerator(new MyDefinitionId(typeof(MyObjectBuilder_OxygenGenerator), "DeuteriumRamscoop"));


        }
        private void LGIndustrialOverhaulGeothermalOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 25
            definition.OperationalPowerConsumption *= 1; // 0.5 MW

            string gas1 = "Steam";
            int gas1ratio = 250000000; //250000000

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
            };
            
            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + " /1 ice" + "\n" +
                                  "Consumption: " + (definition.IceConsumptionPerSecond / 1000000).ToString("0.##") + "M heat / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n";

        }
        private void LGIndustiralOverhaulGasolineRefineryOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 10
            
            definition.OperationalPowerConsumption *= 1; // 2 MW
            definition.BlueprintClasses.;
            string gas1 = "Gasoline";
            int gas1ratio = 8; //8

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
            };

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond * 2 + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n";

        }
        private void LGIndustiralOverhaulRocketFuelRefineryOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 25
            definition.OperationalPowerConsumption *= 1; // 0.5 MW

            string gas1 = "Oxygen";
            int gas1ratio = 10; //10

            string gas2 = "Hydrogen";
            int gas2ratio = 20; //20

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas2ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas2),

                },
            };

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + gas2 + " /1 ice" + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond * 2 + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n" +
                                  gas2 + " Output/s: " + (definition.IceConsumptionPerSecond * gas2ratio).ToString("0.##") + "\n";

        }
        private void LGIndustiralOverhaulCoalFurnaceOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 25
            definition.OperationalPowerConsumption *= 1; // 0.5 MW

            string gas1 = "Oxygen";
            int gas1ratio = 10; //10

            string gas2 = "Hydrogen";
            int gas2ratio = 20; //20

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas2ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas2),

                },
            };

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + gas2 + " /1 ice" + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond * 2 + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n" +
                                  gas2 + " Output/s: " + (definition.IceConsumptionPerSecond * gas2ratio).ToString("0.##") + "\n";

        }
        private void LGIndustiralOverhaulNuclearReactorOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 25
            definition.OperationalPowerConsumption *= 1; // 0.5 MW

            string gas1 = "Oxygen";
            int gas1ratio = 10; //10

            string gas2 = "Hydrogen";
            int gas2ratio = 20; //20

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas2ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas2),

                },
            };

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + gas2 + " /1 ice" + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond * 2 + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n" +
                                  gas2 + " Output/s: " + (definition.IceConsumptionPerSecond * gas2ratio).ToString("0.##") + "\n";

        }
        private void LGIndustiralOverhaulFastNeutronReactorOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 25
            definition.OperationalPowerConsumption *= 1; // 0.5 MW

            string gas1 = "Oxygen";
            int gas1ratio = 10; //10

            string gas2 = "Hydrogen";
            int gas2ratio = 20; //20

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas2ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas2),

                },
            };

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + gas2 + " /1 ice" + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond * 2 + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n" +
                                  gas2 + " Output/s: " + (definition.IceConsumptionPerSecond * gas2ratio).ToString("0.##") + "\n";

        }
        private void LGIndustiralOverhaulDeuteriumExtractorOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 25
            definition.OperationalPowerConsumption *= 1; // 0.5 MW

            string gas1 = "Oxygen";
            int gas1ratio = 10; //10

            string gas2 = "Hydrogen";
            int gas2ratio = 20; //20

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas2ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas2),

                },
            };

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + gas2 + " /1 ice" + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond * 2 + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n" +
                                  gas2 + " Output/s: " + (definition.IceConsumptionPerSecond * gas2ratio).ToString("0.##") + "\n";

        }
        private void LGIndustiralOverhaulDeuteriumRamscoopOxygenGenerator(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyOxygenGeneratorDefinition;
            definition.PCU *= 1; // 50
            definition.IceConsumptionPerSecond *= 1; // 25
            definition.OperationalPowerConsumption *= 1; // 0.5 MW

            string gas1 = "Oxygen";
            int gas1ratio = 10; //10

            string gas2 = "Hydrogen";
            int gas2ratio = 20; //20

            definition.ProducedGases = new List<MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo>
            {
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas1ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas1),
                },
                new MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo
                {
                    IceToGasRatio = gas2ratio,
                    Id = MyDefinitionId.Parse("GasProperties/"+gas2),

                },
            };

            definition.DescriptionEnum = null;
            definition.DescriptionString = "Produces: " + gas1 + " and " + gas2 + " /1 ice" + "\n" +
                                  "Consumption: " + definition.IceConsumptionPerSecond * 2 + " Ice / Second" + "\n" +
                                  "Power Consuption: " + definition.OperationalPowerConsumption + " MW" + "\n" +
                                  gas1 + " Output/s: " + (definition.IceConsumptionPerSecond * gas1ratio).ToString("0.##") + "\n" +
                                  gas2 + " Output/s: " + (definition.IceConsumptionPerSecond * gas2ratio).ToString("0.##") + "\n";

        }


    }
}