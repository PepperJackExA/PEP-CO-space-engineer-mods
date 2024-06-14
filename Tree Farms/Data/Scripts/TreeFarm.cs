using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Define the weighted probabilities for each range
        // Ranges 1-3 and 7-10 are less common than 4-6
        private static int[] probabilities = { 1, 1, 1, 3, 4, 3, 2, 1, 1, 1 };

        private int updateInterval = 600; // Configurable interval in ticks (10 ticks per second)
        private string subId = "AppleTreeFarm"; // Configurable subID for the block
        private int tickCounter = 0;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                MyEntities.OnEntityAdd += MyEntities_OnEntityAdd;
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (tickCounter++ >= updateInterval)
            {
                tickCounter = 0;
                DropApplesNearBlocks();
            }
        }

        private void MyEntities_OnEntityAdd(MyEntity obj)
        {
            // No longer needed to handle tree addition specifically
        }

        private void DropApplesNearBlocks()
        {
            var entities = MyEntities.GetEntities();
            foreach (var entity in entities)
            {
                var grid = entity as IMyCubeGrid;
                if (grid != null)
                {
                    var blocks = new List<IMySlimBlock>();
                    grid.GetBlocks(blocks, b => b.FatBlock != null && b.FatBlock.BlockDefinition.SubtypeId.ToString() == subId.ToString());

                    foreach (var block in blocks)
                    {
                        // Define the random number of apples to drop using weighted probabilities
                        int dropAmount = GetWeightedRandomNumber();

                        // Slightly damage the block
                        float damageAmount = 0.1f; // Amount of damage to apply
                        block.DoDamage(damageAmount, MyDamageType.Grind, true);

                        // Generate random positions for each apple
                        for (int i = 0; i < dropAmount; i++)
                        {
                            // Generate random offsets for x and y coordinates
                            double xOffset = rand.NextDouble() * 2 - 1; // Random value between -1 and 1
                            double yOffset = rand.NextDouble() * 2 - 1; // Random value between -1 and 1

                            // Define the position of the apple above the base of the block
                            Vector3D applePosition = block.FatBlock.GetPosition() + (block.FatBlock.WorldMatrix.Up * (i + 1)) +
                                                     (block.FatBlock.WorldMatrix.Right * xOffset) + (block.FatBlock.WorldMatrix.Forward * yOffset);

                            // Create a random rotation for the apple
                            Quaternion appleRotation = Quaternion.CreateFromYawPitchRoll((float)(rand.NextDouble() * Math.PI * 2),
                                                                                 (float)(rand.NextDouble() * Math.PI * 2),
                                                                                 (float)(rand.NextDouble() * Math.PI * 2));

                            // Create the apple item
                            MyObjectBuilder_ConsumableItem DropApple = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ConsumableItem>("Apple");
                            VRage.MyFixedPoint amount = (VRage.MyFixedPoint)1; // Each apple is a single item

                            // Spawn the apple as a floating object
                            MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(amount, DropApple), applePosition, appleRotation.Up, block.FatBlock.WorldMatrix.Up);
                        }
                    }
                }
            }
        }

        // Method to get a weighted random number based on defined probabilities
        private int GetWeightedRandomNumber()
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
                    return i + 1; // Return the index (which is the value itself) starting from 1
                }
            }

            // This should never happen if probabilities are correctly defined
            return 1; // Default to 1 if something goes wrong
        }
    }
}
