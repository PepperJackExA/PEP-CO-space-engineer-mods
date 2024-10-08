using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;
using PEPCO.iSurvival.Log;
using PEPCO.iSurvival.stats;
using PEPCO.iSurvival.Chat;
using PEPCO.iSurvival.Effects;
using PEPCO.iSurvival.factors;
using PEPCO.iSurvival.settings;
using Sandbox.Game.Entities.Character.Components;
using VRage.ModAPI;
using Sandbox.Game.Components;
using VRage.Utils;
using System.Text;

namespace PEPCO.iSurvival.Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class iSurvivalSession : MySessionComponentBase
    {
        public static ushort modId = 19008;
        public static int runCount = 0;
        public static int loadWait = 120;

        //blink stuff
        public static List<long> blinkList = new List<long>();
        public static Random rand = new Random();




        public static void ApplyStatChange(MyEntityStat stat, double multiplier, double baseChange)
        {
            if (stat == null) return;
            string statName = stat.StatId.ToString();

            //MyAPIGateway.Utilities.ShowMessage($"test", $"statName: {statName}");
            // Calculate the change amount (drain or heal)
            double changeAmount = (stats.StatManager._statSettings[statName].Base + baseChange) * (multiplier * stats.StatManager._statSettings[statName].Multiplier);

            // Apply the change: if negative, decrease; if positive, increase
            if (changeAmount < 0)
            {
                changeAmount *= stats.StatManager._statSettings[stat.StatId.ToString()].DecreaseMultiplier;
                stat.Decrease((float)-changeAmount, null); // Negative for drain
            }
            else
            {
                changeAmount *= stats.StatManager._statSettings[stat.StatId.ToString()].IncreaseMultiplier;
                stat.Increase((float)changeAmount, null); // Positive for heal
            }
        }


        public override void LoadData()
        {
            base.LoadData();

            if (MyAPIGateway.Multiplayer.IsServer)
            {
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(modId, OnMessageReceived);
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            // Unregister message handler when the mod is unloaded
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(modId, OnMessageReceived);
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(modId, Effects.Processes.Blink.getPoke);
        }

        public override void UpdateAfterSimulation()
        {
            try
            {
                if (++runCount % 15 > 0) // Run every quarter of a second
                    return;                       


                if (MyAPIGateway.Multiplayer.IsServer && runCount % 60 == 0) // Run every second on server
                {
                    ProcessPlayersSafely();
                }
                // START BLINK STUFF
                // Blinking during load screen causes crash, don't load messagehandler on clients for 30s
                if (!MyAPIGateway.Multiplayer.IsServer && loadWait > 0)
                {
                    if (--loadWait == 0)
                        MyAPIGateway.Multiplayer.RegisterMessageHandler(modId, Effects.Processes.Blink.getPoke);
                    return;
                }
                foreach (var playerId in blinkList)
                    if (playerId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                        Effects.Processes.Blink.blink(false);
                    else
                        MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("unblink"), MyVisualScriptLogicProvider.GetSteamId(playerId), true);
                blinkList.Clear();
                // END BLINK STUFF

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
                p.Character.IsPlayer &&
                p.Character.ToString().Contains("Astronaut") &&
                p.Character.Integrity > 0
            );

            foreach (IMyPlayer player in players)
            {
                if (player?.Character == null)
                    continue;

                // Skip players in the exception list
                if (iSurvivalSessionSettings.playerExceptions.Contains(player.SteamUserId))
                    continue;

                // Ensure the character components are valid
                var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
                if (statComp == null)
                    continue;


                // Process player stats if alive and not recently revived
                Effects.Processes.ProcessPlayer(player, statComp);
            }
        }


        private void OnMessageReceived(ushort modId, byte[] data, ulong sender, bool reliable)
        {
            // Handle messages from clients
            // Parse and process data from the client here
        }
    }
}
