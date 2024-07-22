using Sandbox.Game.Entities;
using System;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.Components;
using PEPCO.iSurvival.Core;
using VRage.Game;
using VRage;
using Sandbox.Game.Components;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Utils;
using System.Security.Cryptography;
using System.Text;
using VRage.Network;

namespace PEPCO.iSurvival.RecycleFilters
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class iSurvivalRecycleSession : MySessionComponentBase
    {
        public static int runCount = 0;
        public static int loadWait = 120;
        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (++runCount % 15 > 0) // Run every quarter of a second
                   
                return;
                MyAPIGateway.Utilities.ShowMessage("recycle", $"{runCount}");
                if (!MyAPIGateway.Multiplayer.IsServer && loadWait > 0) // Delay loading message handler for clients
                {
                    if (--loadWait == 0)
                        return;
                }



                if (runCount % 60 > 0) // Run every second
                    return;

                ProcessPlayers();

                if (runCount > 299)
                    runCount = 0;
            }
            catch { }

        }
        private void ProcessPlayers()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players, p => p.Character != null && p.Character.ToString().Contains("Astronaut"));

            foreach (IMyPlayer player in players)
            {
                if (iSurvivalSessionSettings.playerExceptions.Contains(MyAPIGateway.Session.LocalHumanPlayer.SteamUserId)) continue;

                var statComp = player.Character?.Components.Get<MyEntityStatComponent>();
                if (statComp == null)
                    continue;

                MyEntityStat fatigue, hunger, stamina, health, water, sanity;
                if (!statComp.TryGetStat(MyStringHash.GetOrCompute("Fatigue"), out fatigue) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Hunger"), out hunger) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Stamina"), out stamina) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Health"), out health) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Water"), out water) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Sanity"), out sanity))

                    continue;
                ProcessOrganicCollection(player, hunger, water);
            }
        }

        private void ProcessOrganicCollection(IMyPlayer player, MyEntityStat hunger, MyEntityStat water)
        {
            string recycleItem = "MyObjectBuilder_Component/SteelPlate";
            var inventory = player.Character.GetInventory();
            MyDefinitionId requiredItemDefinition = MyDefinitionId.Parse($"{recycleItem}");
            MyFixedPoint requiredItemAmount = 1;

            if (inventory.ContainItems(requiredItemAmount, requiredItemDefinition))
            {
                if (hunger.Value > 20 && water.Value > 20)
                {
                    float organicsAmount = (100 - hunger.Value) / 100f + (100 - water.Value) / 100f;
                    inventory.RemoveItemsOfType(requiredItemAmount, requiredItemDefinition);

                    inventory.AddItems((MyFixedPoint)organicsAmount,
                        (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                    inventory.AddItems((MyFixedPoint)organicsAmount,
                        (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Ice")));

                    // Optional: Display a message to the player
                    MyAPIGateway.Utilities.ShowMessage("Organics", $"Collected {organicsAmount} organics and ice based on hunger and water levels.");
                }
            }
            else
            {
                // Optional: Display a message if the player does not have the required item
                MyAPIGateway.Utilities.ShowMessage("Organics", "You need a SteelPlate in your inventory to collect organics and ice.");
            }
        }
    }
}
