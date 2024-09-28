using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRageMath;
using PEPCO.iSurvival.Log;
using PEPCO.iSurvival.stats;
using PEPCO.iSurvival.Chat;
using PEPCO.iSurvival.Effects;
using PEPCO.iSurvival.factors;
using PEPCO.iSurvival.settings;
using VRage.Game.ModAPI;
using Sandbox.Game.Components;

namespace PEPCO.iSurvival.Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class iSurvivalSession : MySessionComponentBase
    {
        public static ushort modId = 19008;
        public static int runCount = 0;
        public static Random rand = new Random();
        public static int loadWait = 120;
        public static float currentAveragestats = 0;
        public static float playerinventoryfillfactor = 0.0f;

        // Tracks revival status to avoid crashes when accessing player stats
        private static Dictionary<long, int> revivalTimers = new Dictionary<long, int>();

        public static void ApplyStatChange(MyEntityStat stat, double multiplier, double baseChange)
        {
            if (stat == null) return;

            // Calculate the change amount (drain or heal)
            double changeAmount = baseChange * multiplier;

            // Apply the change: if negative, decrease; if positive, increase
            if (changeAmount < 0)
            {
                stat.Decrease((float)-changeAmount, null); // Negative for drain
            }
            else
            {
                stat.Increase((float)changeAmount, null); // Positive for heal
            }
        }

        protected override void UnloadData()
        {
            try
            {
                var players = new List<IMyPlayer>();
                MyAPIGateway.Multiplayer.Players.GetPlayers(players);

                // Reset stats for all players currently in the game
                foreach (var player in players)
                {
                    if (player.Character != null && !player.Character.IsDead)
                    {
                        Effects.Processes.ResetPlayerStats(player); // Reset player stats
                        RemovePlayerStatComponents(player); // Remove custom components if any
                    }
                }

                // Unregister message handlers and other events here if needed
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(modId, OnMessageReceived);
            }
            catch (Exception ex)
            {
                iSurvivalLog.Error("Error during UnloadData", ex.ToString());
            }
            base.UnloadData();
        }

        public override void UpdateAfterSimulation()
        {
            try
            {
                if (++runCount % 15 > 0) // Run every quarter of a second
                    return;

                if (!MyAPIGateway.Multiplayer.IsServer && loadWait > 0) // Delay loading message handler for clients
                {
                    if (--loadWait == 0)
                        MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(modId, OnMessageReceived);
                    return;
                }

                if (runCount % 60 == 0) // Run every second
                {
                    ProcessPlayersSafely();
                }

                if (runCount > 299)
                    runCount = 0;
            }
            catch (Exception ex)
            {
                iSurvivalLog.Error("iSurvival error", ex.ToString());
            }
        }

        // Safely process all players, with checks for revival and null components
        private void ProcessPlayersSafely()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players, p =>
                p.Character != null &&
                p.Character.ToString().Contains("Astronaut") &&
                p.Character.IsPlayer &&
                p.Character.Integrity > 0
            );

            foreach (IMyPlayer player in players)
            {
                if (player == null || player.Character == null)
                    continue;

                // Ensure the character components are valid
                var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
                if (statComp == null)
                    continue;

                // Check player death state
                if (player.Character.IsDead)
                {
                    if (!revivalTimers.ContainsKey(player.IdentityId))
                    {
                        OnPlayerDeath(player); // Reset stats on death
                        revivalTimers[player.IdentityId] = 0; // Start a timer for the revival check
                    }
                }
                else
                {
                    // Player is alive, process stats
                    if (revivalTimers.ContainsKey(player.IdentityId))
                    {
                        // Increment timer, remove from dictionary if exceeded safe period
                        revivalTimers[player.IdentityId]++;
                        if (revivalTimers[player.IdentityId] > 300)
                            revivalTimers.Remove(player.IdentityId);
                    }
                    else
                    {
                        Effects.Processes.ProcessPlayer(player); // Process player stats if alive and not recently revived
                    }
                }
            }
        }

        private void OnPlayerDeath(IMyPlayer player)
        {
            // Check if player is already marked as dead to prevent multiple resets
            if (revivalTimers.ContainsKey(player.IdentityId))
                return;

            // Reset stats when the player dies
            Effects.Processes.ResetPlayerStats(player);

            // Optionally remove components if necessary
            RemovePlayerStatComponents(player);
        }

        public static void RemovePlayerStatComponents(IMyPlayer player)
        {
            var character = player.Character;
            if (character == null) return;

            var statComp = character.Components.Get<MyEntityStatComponent>();
            if (statComp != null)
            {
                // Check if stat component is active before removing it
                if (character.Integrity > 0)
                {
                    character.Components.Remove<MyEntityStatComponent>(); // Remove the stat component
                }
            }

            // Additional custom components can be removed here if needed
        }

        private void OnMessageReceived(ushort modId, byte[] data, ulong sender, bool reliable)
        {
            // Handle messages from clients
        }
    }
}
