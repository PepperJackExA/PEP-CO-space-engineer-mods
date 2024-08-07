using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace PEPCO.Economy
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class EconomicSessionComponent : MySessionComponentBase
    {
        // Base costs for raw materials
        private Dictionary<string, double> rawMaterialBaseCosts = new Dictionary<string, double>()
        {
            { "IronIngot", 10 },
            { "NickelIngot", 20 },
            { "SiliconIngot", 15 }
        };

        // Base production costs for components
        private Dictionary<string, double> componentBaseProductionCosts = new Dictionary<string, double>()
        {
            { "SteelPlate", 5 },
            { "ConstructionComponent", 3 },
            { "Computer", 10 },
            { "Motor", 7 }
        };

        // Components and their raw material requirements
        private Dictionary<string, Dictionary<string, int>> componentRawMaterials = new Dictionary<string, Dictionary<string, int>>();

        private Dictionary<string, double> componentCurrentPrices = new Dictionary<string, double>();

        private int updateTickCounter = 0;
        private const int updateInterval = 600; // Update prices every 600 ticks (10 seconds assuming 60 ticks per second)

        public override void LoadData()
        {
            InitializeComponentRawMaterials();

            foreach (var component in componentBaseProductionCosts.Keys)
            {
                componentCurrentPrices[component] = componentBaseProductionCosts[component];
            }

            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
        }

        public override void UpdateBeforeSimulation()
        {
            updateTickCounter++;

            if (updateTickCounter >= updateInterval)
            {
                updateTickCounter = 0;

                // Iterate over all entities to find EconomicBlocks and calculate prices
                List<IMyEntity> entities = new List<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCubeBlock && ((IMyCubeBlock)e).BlockDefinition.SubtypeName == "EconomicBlock");

                foreach (var component in componentBaseProductionCosts.Keys)
                {
                    double totalCost = 0;

                    foreach (var entity in entities)
                    {
                        var block = entity as IMyCubeBlock;
                        var position = block.GetPosition();
                        double distance = Vector3D.Distance(position, Vector3D.Zero); // Assuming source is at (0, 0, 0)
                        totalCost += CalculateComponentCost(component, distance);
                    }

                    if (entities.Count > 0)
                    {
                        componentCurrentPrices[component] = totalCost / entities.Count;
                    }
                }

                // Log updated prices for debugging
                foreach (var component in componentCurrentPrices)
                {
                    MyLog.Default.WriteLine($"Component: {component.Key}, Price: {component.Value}");
                }
            }
        }

        private void InitializeComponentRawMaterials()
        {
            foreach (var blueprint in MyDefinitionManager.Static.GetBlueprintDefinitions())
            {
                string componentName = blueprint.Id.SubtypeName;

                if (!componentBaseProductionCosts.ContainsKey(componentName))
                    continue;

                var materials = new Dictionary<string, int>();

                foreach (var prerequisite in blueprint.Prerequisites)
                {
                    string materialName = prerequisite.Id.SubtypeName;
                    int amount = (int)prerequisite.Amount;
                    materials[materialName] = amount;
                }

                componentRawMaterials[componentName] = materials;
            }
        }

        private double CalculateComponentCost(string component, double distance)
        {
            double totalRawMaterialCost = 0;

            if (componentRawMaterials.ContainsKey(component))
            {
                foreach (var material in componentRawMaterials[component])
                {
                    string materialName = material.Key;
                    int materialAmount = material.Value;

                    if (rawMaterialBaseCosts.ContainsKey(materialName))
                    {
                        double materialCost = rawMaterialBaseCosts[materialName];
                        totalRawMaterialCost += materialCost * materialAmount;
                    }
                }
            }

            double productionCost = componentBaseProductionCosts.ContainsKey(component) ? componentBaseProductionCosts[component] : 0;
            double totalCost = (totalRawMaterialCost + productionCost) * distance;

            return totalCost;
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            var player = MyAPIGateway.Session.Player;

            if (player == null)
            {
                return;
            }

            if (messageText.StartsWith("/economy "))
            {
                var command = messageText.Substring("/economy ".Length).ToLower();

                if (command.StartsWith("price "))
                {
                    var parts = command.Split(' ');
                    if (parts.Length == 2)
                    {
                        string component = parts[1];

                        if (componentCurrentPrices.ContainsKey(component))
                        {
                            MyAPIGateway.Utilities.ShowMessage("Economy", $"Current price of {component}: {componentCurrentPrices[component]}");
                        }
                        else
                        {
                            MyAPIGateway.Utilities.ShowMessage("Economy", $"Component {component} not found.");
                        }
                    }

                    sendToOthers = false;
                }
            }
        }
    }
}
