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
using static PEPCO.iSurvival.Effects.Processes;


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

        //Starvation Stuff
        public static int starvationMessageCooldown = 0; // Cooldown timer (in frames)


        public static Dictionary<long, RPGLeveling> playerRPGSystems = new Dictionary<long, RPGLeveling>();



        public static void ApplyStatChange(IMyPlayer player, MyEntityStat stat, double multiplier, double baseChange)
        {
            if (stat == null) return;
            string statName = stat.StatId.ToString();

            // Retrieve RPG stats from the player
            var statComp = player.Character?.Components?.Get<MyEntityStatComponent>();
            MyEntityStat strength, dexterity, constitution, wisdom, intelligence, charisma;

            statComp.TryGetStat(MyStringHash.GetOrCompute("Strength"), out strength);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Dexterity"), out dexterity);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Constitution"), out constitution);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Wisdom"), out wisdom);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Intelligence"), out intelligence);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Charisma"), out charisma);

            // Calculate efficiency boost from RPG stats
            double rpgBoost = CalculateRPGBoost(strength, dexterity, constitution, wisdom, intelligence, charisma, statName);

            // Calculate the change amount with the RPG boost
            double changeAmount = (stats.StatManager._statSettings[statName].Base + baseChange)
                                  * (multiplier * stats.StatManager._statSettings[statName].Multiplier)
                                  * rpgBoost;

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
        // Calculate RPG boost based on player stats
        private static double CalculateRPGBoost(MyEntityStat strength, MyEntityStat dexterity, MyEntityStat constitution, MyEntityStat wisdom, MyEntityStat intelligence, MyEntityStat charisma, string statName)
        {
            double boost = 1.0;
            const double baseStat = 10.0;

            switch (statName.ToLower())
            {
                case "stamina":
                    if (strength != null) boost += CalculateRPGEffect(strength.Value, baseStat, 0.2);
                    if (dexterity != null) boost += CalculateRPGEffect(dexterity.Value, baseStat, 0.1);
                    break;
                case "fatigue":
                    if (constitution != null) boost += CalculateRPGEffect(constitution.Value, baseStat, 0.15);
                    if (wisdom != null) boost += CalculateRPGEffect(wisdom.Value, baseStat, 0.1);
                    break;
                case "calories":
                case "fat":
                case "protein":
                    if (constitution != null) boost += CalculateRPGEffect(constitution.Value, baseStat, 0.1);
                    if (strength != null) boost += CalculateRPGEffect(strength.Value, baseStat, 0.05);
                    if (dexterity != null) boost += CalculateRPGEffect(dexterity.Value, baseStat, 0.05);
                    break;
                case "sanity":
                    if (wisdom != null) boost += CalculateRPGEffect(wisdom.Value, baseStat, 0.2);
                    if (charisma != null) boost += CalculateRPGEffect(charisma.Value, baseStat, 0.1);
                    break;
            }
            return boost;
        }

        // Helper function to calculate RPG effects based on stat values
        private static double CalculateRPGEffect(double currentValue, double baseValue, double maxEffect)
        {
            double effect = (currentValue - baseValue) / baseValue;
            return MathHelper.Clamp(effect * maxEffect, -maxEffect, maxEffect);
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
                if (MyAPIGateway.Multiplayer.IsServer && runCount % 60 == 0) // Run every second on server
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

                // Initialize the player's RPG stats dictionary
                Dictionary<string, MyEntityStat> playerStats = new Dictionary<string, MyEntityStat>();

                // Retrieve stats using TryGetStat
                MyEntityStat strength, dexterity, constitution, wisdom, intelligence, charisma;

                if (statComp.TryGetStat(MyStringHash.GetOrCompute("Strength"), out strength))
                    playerStats["Strength"] = strength;

                if (statComp.TryGetStat(MyStringHash.GetOrCompute("Dexterity"), out dexterity))
                    playerStats["Dexterity"] = dexterity;

                if (statComp.TryGetStat(MyStringHash.GetOrCompute("Constitution"), out constitution))
                    playerStats["Constitution"] = constitution;

                if (statComp.TryGetStat(MyStringHash.GetOrCompute("Wisdom"), out wisdom))
                    playerStats["Wisdom"] = wisdom;

                if (statComp.TryGetStat(MyStringHash.GetOrCompute("Intelligence"), out intelligence))
                    playerStats["Intelligence"] = intelligence;

                if (statComp.TryGetStat(MyStringHash.GetOrCompute("Charisma"), out charisma))
                    playerStats["Charisma"] = charisma;

                // Initialize or retrieve the player's RPG leveling system
                long playerId = player.IdentityId;

                if (!playerRPGSystems.ContainsKey(playerId))
                {
                    playerRPGSystems[playerId] = new RPGLeveling(playerStats);
                }

                RPGLeveling rpgLeveling = playerRPGSystems[playerId];

                // Add XP to the player (example value of 10 XP per cycle, customize this)
                rpgLeveling.AddXP(1f);


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
