using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace pepcoTreesDropApples
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class Main : MySessionComponentBase
    {
        public static Random rand = new Random();

        // Define the weighted probabilities for each range
        // Ranges 1-3 and 7-10 are less common than 4-6
        private static int[] probabilities = { 1, 1, 1, 3, 4, 3, 2, 1, 1, 1 };

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                MyEntities.OnEntityAdd += MyEntities_OnEntityAdd;
            }
        }

        private void MyEntities_OnEntityAdd(MyEntity obj)
        {
            string entityString = obj.ToString();

            // Check if the added entity is a tree
            if (entityString.StartsWith("MyDebrisTr"))
            {
                // Define the random number of apples to drop using weighted probabilities
                int dropAmount = GetWeightedRandomNumber();
                IMyEntity tree = obj as IMyEntity;

                // Generate random positions for each apple
                for (int i = 0; i < dropAmount; i++)
                {
                    // Generate random offsets for x and y coordinates
                    double xOffset = rand.NextDouble() * 2 - 1; // Random value between -1 and 1
                    double yOffset = rand.NextDouble() * 2 - 1; // Random value between -1 and 1

                    // Define the position of the apple above the base of the tree
                    Vector3D applePosition = tree.GetPosition() + (tree.WorldMatrix.Up * (i + 1)) + 
                                             (tree.WorldMatrix.Right * xOffset) + (tree.WorldMatrix.Forward * yOffset);

                    // Create a random rotation for the apple
                    Quaternion appleRotation = Quaternion.CreateFromYawPitchRoll((float)(rand.NextDouble() * Math.PI * 2),
                                                                                 (float)(rand.NextDouble() * Math.PI * 2),
                                                                                 (float)(rand.NextDouble() * Math.PI * 2));

                    // Create the apple item
                    MyObjectBuilder_ConsumableItem DropApple = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ConsumableItem>("Apple");
                    VRage.MyFixedPoint amount = 1; // Each apple is a single item

                    // Spawn the apple as a floating object
                    MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(amount, DropApple), applePosition, appleRotation.Up, tree.WorldMatrix.Up);
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
