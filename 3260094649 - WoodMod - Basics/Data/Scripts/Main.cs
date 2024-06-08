using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace pepcoTreesDropLogs
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
                // Define the random number of Logs to drop using weighted probabilities
                int dropAmount = GetWeightedRandomNumber();
                IMyEntity tree = obj as IMyEntity;

                // Validate if tree is not null to avoid potential null reference exceptions
                if (tree == null) return;

                // Define the base position at the base of the tree
                Vector3D basePosition = tree.GetPosition();

                // Generate random positions for each Log
                for (int i = 0; i < dropAmount; i++)
                {

                    // Calculate the position of the Log directly above the previous one
                    Vector3D logPosition = basePosition + (tree.WorldMatrix.Up * (i + 5));

                    // Create a random rotation for the Log
                    Quaternion logRotation = Quaternion.CreateFromYawPitchRoll(
                        (float)(rand.NextDouble() * Math.PI * 2),
                        (float)(rand.NextDouble() * Math.PI * 2),
                        (float)(rand.NextDouble() * Math.PI * 2)
                    );

                    // Create the Log item
                    MyObjectBuilder_Component DropLog = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>("OakWoodLog");
                    VRage.MyFixedPoint amount = 1; // Each Log is a single item

                    // Spawn the Log as a floating object
                    MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(amount, DropLog), logPosition, logRotation.Up, tree.WorldMatrix.Up);
                }

                // Remove the tree entity
                MyAPIGateway.Entities.MarkForClose(tree);
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
