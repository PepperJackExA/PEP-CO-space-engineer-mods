using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using Sandbox.Game.Entities;
using System.Collections.Generic;
using Sandbox.Game.Components;
using VRage.Game.Components;

namespace PEPCO.iSurvival.Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class iSurvivalSession : MySessionComponentBase
    {
        private Dictionary<long, int> playerItemCounts = new Dictionary<long, int>();

        public override void UpdateAfterSimulation()
        {
            if (!MyAPIGateway.Multiplayer.IsServer) // Ensure only server executes this
                return;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player.Character == null)
                    continue;

                // Monitor the player's inventory
                var inventory = player.Character.GetInventory();
                if (inventory != null)
                {
                    long playerId = player.IdentityId;
                    int currentItemCount = inventory.ItemCount;

                    if (playerItemCounts.ContainsKey(playerId))
                    {
                        // Check if the item count decreased (indicating consumption)
                        if (playerItemCounts[playerId] > currentItemCount)
                        {
                            // An item was consumed, trigger your effect modification
                            var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
                            ConsumableHandler.ModifyConsumableEffect(player, statComp, -1); // Example: assuming -1 hunger change for consumed item
                        }
                    }

                    // Update the stored item count
                    playerItemCounts[playerId] = currentItemCount;
                }
            }
        }
    }
}
