using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage;
using Sandbox.Game.Entities;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRageMath;

namespace PepcoTreeFarm
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class TreeFarm : MySessionComponentBase
    {
        public static Random rand = new Random();

        private int updateInterval = 300; // Configurable interval in ticks (60 ticks per second)
        private bool loading = true;
        private int loadingTicks = 3;
        private bool isServer;
        private bool isDedicated;
        private int tickCounter = 0; // Declare tickCounter as a class-level variable

        // Configurable drop settings
        private Dictionary<string, DropSettings> blockDropSettings = new Dictionary<string, DropSettings>
        {
            //"SubtypeId", new DropSettings("ItemName", typeof(ItemType), MinAmount, MaxAmount, new int[] { probability }, DamageAmount)
            { "AppleTreeFarmLG", new DropSettings("SteelPlate", typeof(MyObjectBuilder_Component), 1, 5, new int[] { 1, 1, 1, 3, 4, 3, 2, 1, 1, 1 }, 1f) },
            { "AppleTreeFarm", new DropSettings("Apple", typeof(MyObjectBuilder_ConsumableItem), 1, 5, new int[] { 1, 1, 1, 3, 4, 3, 2, 1, 1, 1 }, 1f) },
            // Add more blocks and their drop settings here
        };

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                MyEntities.OnEntityAdd += MyEntities_OnEntityAdd;
            }

            isServer = MyAPIGateway.Multiplayer.IsServer;
            isDedicated = MyAPIGateway.Utilities.IsDedicated;
        }

        protected override void UnloadData()
        {
            MyEntities.OnEntityAdd -= MyEntities_OnEntityAdd;
        }

        public override void UpdateBeforeSimulation()
        {
            if (loading)
            {
                if (loadingTicks-- <= 0)
                {
                    loading = false;
                }
                return;
            }

            if (tickCounter++ >= updateInterval)
            {
                tickCounter = 0;
                DropItemsNearBlocks();
            }
        }

        private void MyEntities_OnEntityAdd(MyEntity obj)
        {
            // No longer needed to handle tree addition specifically
        }

        private void DropItemsNearBlocks()
        {
            var entities = MyEntities.GetEntities();
            foreach (var entity in entities)
            {
                var grid = entity as IMyCubeGrid;
                if (grid != null)
                {
                    var blocks = new List<IMySlimBlock>();
                    // Filter blocks to those with valid SubtypeIds for dropping items
                    grid.GetBlocks(blocks, b => b.FatBlock != null && blockDropSettings.ContainsKey(b.FatBlock.BlockDefinition.SubtypeId.ToString()));

                    foreach (var block in blocks)
                    {
                        if (IsEnvironmentSuitable(grid, block))
                        {
                            var subtypeId = block.FatBlock.BlockDefinition.SubtypeId.ToString();
                            var settings = blockDropSettings[subtypeId];
                            DropItems(block, settings);
                            block.DoDamage(settings.DamageAmount, MyDamageType.Grind, true);
                        }
                    }
                }
            }
        }

        private bool IsEnvironmentSuitable(IMyCubeGrid grid, IMySlimBlock block)
        {
            bool isAirtight = grid.IsRoomAtPositionAirtight(block.FatBlock.Position);
            double oxygenLevel = MyAPIGateway.Session.OxygenProviderSystem.GetOxygenInPoint(block.FatBlock.GetPosition());

            // Log for debugging
            MyAPIGateway.Utilities.ShowMessage("TreeFarm", $"Airtight: {isAirtight}, Oxygen Level: {oxygenLevel}");

            // Check if the block is in an airtight room or if oxygen level is sufficient
            return isAirtight || oxygenLevel > 0.5;
        }

        private void DropItems(IMySlimBlock block, DropSettings settings)
        {
            int dropAmount = GetWeightedRandomNumber(settings.Probabilities);
            for (int i = 0; i < dropAmount; i++)
            {
                double xOffset = rand.NextDouble() * 2 - 1;
                double yOffset = rand.NextDouble() * 2 - 1;

                Vector3D dropPosition = block.FatBlock.GetPosition() + (block.FatBlock.WorldMatrix.Up * (i + 1)) +
                                        (block.FatBlock.WorldMatrix.Right * xOffset) + (block.FatBlock.WorldMatrix.Forward * yOffset);

                Quaternion dropRotation = Quaternion.CreateFromYawPitchRoll((float)(rand.NextDouble() * Math.PI * 2),
                                                                            (float)(rand.NextDouble() * Math.PI * 2),
                                                                            (float)(rand.NextDouble() * Math.PI * 2));

                // Create the object builder dynamically based on the type
                MyObjectBuilder_PhysicalObject dropItem = CreateObjectBuilder(settings.ItemName, settings.ItemType);
                VRage.MyFixedPoint amount = (VRage.MyFixedPoint)1;

                MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(amount, dropItem), dropPosition, dropRotation.Up, block.FatBlock.WorldMatrix.Up);
            }
        }

        private MyObjectBuilder_PhysicalObject CreateObjectBuilder(string itemName, Type itemType)
        {
            MyObjectBuilder_PhysicalObject dropItem = null;
            if (itemType == typeof(MyObjectBuilder_ConsumableItem))
            {
                dropItem = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ConsumableItem>(itemName);
            }
            else if (itemType == typeof(MyObjectBuilder_Ore))
            {
                dropItem = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(itemName);
            }
            else if (itemType == typeof(MyObjectBuilder_Ingot))
            {
                dropItem = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ingot>(itemName);
            }
            else if (itemType == typeof(MyObjectBuilder_Component))
            {
                dropItem = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>(itemName);
            }
            else
            {
                throw new Exception($"Unsupported item type: {itemType}");
            }
            return dropItem;
        }

        private int GetWeightedRandomNumber(int[] probabilities)
        {
            int totalWeight = 0;
            foreach (int weight in probabilities)
            {
                totalWeight += weight;
            }

            int randomValue = rand.Next(1, totalWeight + 1);
            int cumulativeWeight = 0;
            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulativeWeight += probabilities[i];
                if (randomValue <= cumulativeWeight)
                {
                    return i + 1;
                }
            }

            return 1;
        }

        private class DropSettings
        {
            public string ItemName { get; }
            public Type ItemType { get; } // Changed to Type to support different item types
            public int MinAmount { get; }
            public int MaxAmount { get; }
            public int[] Probabilities { get; }
            public float DamageAmount { get; } // Added damageAmount to DropSettings

            public DropSettings(string itemName, Type itemType, int minAmount, int maxAmount, int[] probabilities, float damageAmount)
            {
                ItemName = itemName;
                ItemType = itemType;
                MinAmount = minAmount;
                MaxAmount = maxAmount;
                Probabilities = probabilities;
                DamageAmount = damageAmount;
            }
        }
    }
}
