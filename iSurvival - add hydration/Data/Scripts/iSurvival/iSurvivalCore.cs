using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using PEPCO.iSurvival.Chat;
using PEPCO.iSurvival.Log;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using VRage.Game;
using VRage;

namespace PEPCO.iSurvival.Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class iSurvivalSessionSettings : MySessionComponentBase
    {
        public iSurvivalSettings Settings = new iSurvivalSettings();

        // Configurable multipliers for different stats
        public static float staminadrainmultiplier = 1;
        public static float fatiguedrainmultiplier = 1;
        public static float healthdrainmultiplier = 1;
        public static float hungerdrainmultiplier = 1;
        public static float waterdrainmultiplier = 1;
        public static float sanitydrainmultiplier = 1;

        public static float staminaincreasemultiplier = 1;
        public static float fatigueincreasemultiplier = 1;
        public static float healthincreasemultiplier = 1;
        public static float hungerincreasemultiplier = 1;
        public static float waterincreasemultiplier = 1;
        public static float sanityincreasemultiplier = 1;

        // Multipliers for Process Effects
        public static float ProcessSittingEffectMultiplier = 1;
        public static float ProcessStandingEffectMultiplier = 1;
        public static float ProcessCrouchingEffectMultiplier = 1;
        public static float ProcessCrouchWalkingEffectMultiplier = 1;
        public static float ProcessWalkingEffectMultiplier = 1;
        public static float ProcessRunningEffectMultiplier = 1;
        public static float ProcessLadderEffectMultiplier = 1;
        public static float ProcessFlyingEffectMultiplier = 1;
        public static float ProcessFallingEffectMultiplier = 1;
        public static float ProcessSprintingEffectMultiplier = 1;
        public static float ProcessJumpingEffectMultiplier = 1;
        public static float ProcessDefaultMovementEffectMultiplier = 1;
        public static float ProcessHealthAndSanityEffectsMultiplier = 1;
        public static float ProcessOrganicCollectionMultiplier = 1;

        //base movement numbers
        public static float FallingStaminaDecrease = 0.1f;
        public static float CrouchingFatigueIncrease = 0.1f;
        public static float CrouchingStaminaIncrease = 2;
        public static float CrouchWalkStaminaIncrease = 0.1f;
        public static float FlyingStaminaDecrease = 2;
        public static float JumpingStaminaDecrease = 0.1f;
        public static float LadderStaminaDecrease = 0.1f;
        public static float RunningStaminaDecrease = 2;
        public static float SittingFatigueIncrease = 0.5f;
        public static float SittingStaminaIncrease = 2;
        public static float SprintingStaminaDecrease = 4;
        public static float SprintingWaterDecrease = 0.1f;
        public static float StandingStaminaIncrease = 0.5f;
        public static float WalkingStaminaIncrease = 0;


        public static List<ulong> playerExceptions = new List<ulong>();

        public ChatCommands ChatCommands;

        public override void LoadData()
        {
            Settings.Load();
            ApplySettings();
            playerExceptions = Settings.playerExceptions.Distinct().ToList();
            ChatCommands = new ChatCommands(this);
        }

        // Updates settings and applies them
        public void UpdateSettings()
        {
            Settings.SaveConfigAfterChatUpdate();
            ApplySettings();
            playerExceptions = Settings.playerExceptions.Distinct().ToList();
        }

        // Applies settings from the configuration to the session variables
        private void ApplySettings()
        {
            staminadrainmultiplier = Settings.staminadrainmultiplier;
            fatiguedrainmultiplier = Settings.fatiguedrainmultiplier;
            healthdrainmultiplier = Settings.healthdrainmultiplier;
            hungerdrainmultiplier = Settings.hungerdrainmultiplier;
            waterdrainmultiplier = Settings.waterdrainmultiplier;
            sanitydrainmultiplier = Settings.sanitydrainmultiplier;

            staminaincreasemultiplier = Settings.staminaincreasemultiplier;
            fatigueincreasemultiplier = Settings.fatigueincreasemultiplier;
            healthincreasemultiplier = Settings.healthincreasemultiplier;
            hungerincreasemultiplier = Settings.hungerincreasemultiplier;
            waterincreasemultiplier = Settings.waterincreasemultiplier;
            sanityincreasemultiplier = Settings.sanityincreasemultiplier;

            ProcessSittingEffectMultiplier = Settings.ProcessSittingEffectMultiplier;
            ProcessStandingEffectMultiplier = Settings.ProcessStandingEffectMultiplier;
            ProcessCrouchingEffectMultiplier = Settings.ProcessCrouchingEffectMultiplier;
            ProcessCrouchWalkingEffectMultiplier = Settings.ProcessCrouchWalkingEffectMultiplier;
            ProcessWalkingEffectMultiplier = Settings.ProcessWalkingEffectMultiplier;
            ProcessRunningEffectMultiplier = Settings.ProcessRunningEffectMultiplier;
            ProcessLadderEffectMultiplier = Settings.ProcessLadderEffectMultiplier;
            ProcessFlyingEffectMultiplier = Settings.ProcessFlyingEffectMultiplier;
            ProcessFallingEffectMultiplier = Settings.ProcessFallingEffectMultiplier;
            ProcessSprintingEffectMultiplier = Settings.ProcessSprintingEffectMultiplier;
            ProcessJumpingEffectMultiplier = Settings.ProcessJumpingEffectMultiplier;
            ProcessDefaultMovementEffectMultiplier = Settings.ProcessDefaultMovementEffectMultiplier;
            ProcessHealthAndSanityEffectsMultiplier = Settings.ProcessHealthAndSanityEffectsMultiplier;
            ProcessOrganicCollectionMultiplier = Settings.ProcessOrganicCollectionMultiplier;

            FallingStaminaDecrease = Settings.FallingStaminaDecrease;
            CrouchingFatigueIncrease = Settings.CrouchingFatigueIncrease;
            CrouchingStaminaIncrease = Settings.CrouchingStaminaIncrease;
            CrouchWalkStaminaIncrease = Settings.CrouchWalkStaminaIncrease;
            FlyingStaminaDecrease = Settings.FlyingStaminaDecrease;
            JumpingStaminaDecrease = Settings.JumpingStaminaDecrease;
            LadderStaminaDecrease = Settings.LadderStaminaDecrease;
            RunningStaminaDecrease = Settings.RunningStaminaDecrease;
            SittingFatigueIncrease = Settings.SittingFatigueIncrease;
            SittingStaminaIncrease = Settings.SittingStaminaIncrease;
            SprintingStaminaDecrease = Settings.SprintingStaminaDecrease;
            SprintingWaterDecrease = Settings.SprintingWaterDecrease;
            StandingStaminaIncrease = Settings.StandingStaminaIncrease;
            WalkingStaminaIncrease = Settings.WalkingStaminaIncrease;


        }

        protected override void UnloadData()
        {
            ChatCommands?.Dispose();
        }

        public class iSurvivalSettings
        {
            const string VariableId = nameof(iSurvivalSettings);
            const string FileName = "iSurvivalSettings.ini";
            const string IniSection = "Config";

            // Multipliers for different stats
            public float staminadrainmultiplier = 1;
            public float fatiguedrainmultiplier = 1;
            public float healthdrainmultiplier = 1;
            public float hungerdrainmultiplier = 1;
            public float waterdrainmultiplier = 1;
            public float sanitydrainmultiplier = 1;

            public float staminaincreasemultiplier = 1;
            public float fatigueincreasemultiplier = 1;
            public float healthincreasemultiplier = 1;
            public float hungerincreasemultiplier = 1;
            public float waterincreasemultiplier = 1;
            public float sanityincreasemultiplier = 1;

            // Multipliers for Process Effects
            public float ProcessSittingEffectMultiplier = 1;
            public float ProcessStandingEffectMultiplier = 1;
            public float ProcessCrouchingEffectMultiplier = 1;
            public float ProcessCrouchWalkingEffectMultiplier = 1;
            public float ProcessWalkingEffectMultiplier = 1;
            public float ProcessRunningEffectMultiplier = 1;
            public float ProcessLadderEffectMultiplier = 1;
            public float ProcessFlyingEffectMultiplier = 1;
            public float ProcessFallingEffectMultiplier = 1;
            public float ProcessSprintingEffectMultiplier = 1;
            public float ProcessJumpingEffectMultiplier = 1;
            public float ProcessDefaultMovementEffectMultiplier = 1;
            public float ProcessHealthAndSanityEffectsMultiplier = 1;
            public float ProcessOrganicCollectionMultiplier = 1;

            //base movemenet numbers
            public float FallingStaminaDecrease = 0.2f;
            public float CrouchingFatigueIncrease = 0.25f;
            public float CrouchingStaminaIncrease = 2.5f;
            public float CrouchWalkStaminaIncrease = 1f;
            public float FlyingStaminaDecrease = 0.3f;
            public float JumpingStaminaDecrease = 10f;
            public float LadderStaminaDecrease = 0.1f;
            public float RunningStaminaDecrease = 0.2f;
            public float SittingFatigueIncrease = 0.5f;
            public float SittingStaminaIncrease = 4f;
            public float SprintingStaminaDecrease = 5f;
            public float SprintingWaterDecrease = 0.1f;
            public float StandingStaminaIncrease = 1.5f;
            public float WalkingStaminaIncrease = 1f;



            public List<ulong> playerExceptions = new List<ulong>();

            // Loads settings based on whether the session is server or client
            public void Load()
            {
                if (MyAPIGateway.Session.IsServer)
                    LoadOnHost();
                else
                    LoadOnClient();
            }

            // Loads settings on the server
            public void LoadOnHost()
            {
                MyIni iniParser = new MyIni();

                // Load file if exists then save it regardless so that it can be sanitized and updated
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(iSurvivalSettings)))
                {
                    using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(iSurvivalSettings)))
                    {
                        string text = file.ReadToEnd();
                        MyIniParseResult result;
                        if (!iniParser.TryParse(text, out result))
                            throw new Exception($"Config error: {result}");
                        LoadConfig(iniParser);
                    }
                }

                SaveConfig(iniParser);
                SaveToStorage(iniParser.ToString());
            }

            // Loads settings on the client
            public void LoadOnClient()
            {
                string text;
                if (!MyAPIGateway.Utilities.GetVariable<string>(VariableId, out text))
                    throw new Exception("No config found in sandbox.sbc!");

                MyIni iniParser = new MyIni();
                MyIniParseResult result;
                if (!iniParser.TryParse(text, out result))
                    throw new Exception($"Config error: {result}");
                LoadConfig(iniParser);
            }

            // Loads configuration from the ini parser
            public void LoadConfig(MyIni iniParser)
            {
                staminadrainmultiplier = (float)iniParser.Get(IniSection, nameof(staminadrainmultiplier)).ToDouble(staminadrainmultiplier);
                fatiguedrainmultiplier = (float)iniParser.Get(IniSection, nameof(fatiguedrainmultiplier)).ToDouble(fatiguedrainmultiplier);
                healthdrainmultiplier = (float)iniParser.Get(IniSection, nameof(healthdrainmultiplier)).ToDouble(healthdrainmultiplier);
                hungerdrainmultiplier = (float)iniParser.Get(IniSection, nameof(hungerdrainmultiplier)).ToDouble(hungerdrainmultiplier);
                waterdrainmultiplier = (float)iniParser.Get(IniSection, nameof(waterdrainmultiplier)).ToDouble(waterdrainmultiplier);
                sanitydrainmultiplier = (float)iniParser.Get(IniSection, nameof(sanitydrainmultiplier)).ToDouble(sanitydrainmultiplier);

                staminaincreasemultiplier = (float)iniParser.Get(IniSection, nameof(staminaincreasemultiplier)).ToDouble(staminaincreasemultiplier);
                fatigueincreasemultiplier = (float)iniParser.Get(IniSection, nameof(fatigueincreasemultiplier)).ToDouble(fatigueincreasemultiplier);
                healthincreasemultiplier = (float)iniParser.Get(IniSection, nameof(healthincreasemultiplier)).ToDouble(healthincreasemultiplier);
                hungerincreasemultiplier = (float)iniParser.Get(IniSection, nameof(hungerincreasemultiplier)).ToDouble(hungerincreasemultiplier);
                waterincreasemultiplier = (float)iniParser.Get(IniSection, nameof(waterincreasemultiplier)).ToDouble(waterincreasemultiplier);
                sanityincreasemultiplier = (float)iniParser.Get(IniSection, nameof(sanityincreasemultiplier)).ToDouble(sanityincreasemultiplier);

                ProcessSittingEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessSittingEffectMultiplier)).ToDouble(ProcessSittingEffectMultiplier);
                ProcessStandingEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessStandingEffectMultiplier)).ToDouble(ProcessStandingEffectMultiplier);
                ProcessCrouchingEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessCrouchingEffectMultiplier)).ToDouble(ProcessCrouchingEffectMultiplier);
                ProcessCrouchWalkingEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessCrouchWalkingEffectMultiplier)).ToDouble(ProcessCrouchWalkingEffectMultiplier);
                ProcessWalkingEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessWalkingEffectMultiplier)).ToDouble(ProcessWalkingEffectMultiplier);
                ProcessRunningEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessRunningEffectMultiplier)).ToDouble(ProcessRunningEffectMultiplier);
                ProcessLadderEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessLadderEffectMultiplier)).ToDouble(ProcessLadderEffectMultiplier);
                ProcessFlyingEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessFlyingEffectMultiplier)).ToDouble(ProcessFlyingEffectMultiplier);
                ProcessFallingEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessFallingEffectMultiplier)).ToDouble(ProcessFallingEffectMultiplier);
                ProcessSprintingEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessSprintingEffectMultiplier)).ToDouble(ProcessSprintingEffectMultiplier);
                ProcessJumpingEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessJumpingEffectMultiplier)).ToDouble(ProcessJumpingEffectMultiplier);
                ProcessDefaultMovementEffectMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessDefaultMovementEffectMultiplier)).ToDouble(ProcessDefaultMovementEffectMultiplier);
                ProcessHealthAndSanityEffectsMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessHealthAndSanityEffectsMultiplier)).ToDouble(ProcessHealthAndSanityEffectsMultiplier);
                ProcessOrganicCollectionMultiplier = (float)iniParser.Get(IniSection, nameof(ProcessOrganicCollectionMultiplier)).ToDouble(ProcessOrganicCollectionMultiplier);

                FallingStaminaDecrease = (float)iniParser.Get(IniSection, nameof(FallingStaminaDecrease)).ToDouble(FallingStaminaDecrease);
                CrouchingFatigueIncrease = (float)iniParser.Get(IniSection, nameof(CrouchingFatigueIncrease)).ToDouble(CrouchingFatigueIncrease);
                CrouchingStaminaIncrease = (float)iniParser.Get(IniSection, nameof(CrouchingStaminaIncrease)).ToDouble(CrouchingStaminaIncrease);
                CrouchWalkStaminaIncrease = (float)iniParser.Get(IniSection, nameof(CrouchWalkStaminaIncrease)).ToDouble(CrouchWalkStaminaIncrease);
                FlyingStaminaDecrease = (float)iniParser.Get(IniSection, nameof(FlyingStaminaDecrease)).ToDouble(FlyingStaminaDecrease);
                JumpingStaminaDecrease = (float)iniParser.Get(IniSection, nameof(JumpingStaminaDecrease)).ToDouble(JumpingStaminaDecrease);
                LadderStaminaDecrease = (float)iniParser.Get(IniSection, nameof(LadderStaminaDecrease)).ToDouble(LadderStaminaDecrease);
                RunningStaminaDecrease = (float)iniParser.Get(IniSection, nameof(RunningStaminaDecrease)).ToDouble(RunningStaminaDecrease);
                SittingFatigueIncrease = (float)iniParser.Get(IniSection, nameof(SittingFatigueIncrease)).ToDouble(SittingFatigueIncrease);
                SittingStaminaIncrease = (float)iniParser.Get(IniSection, nameof(SittingStaminaIncrease)).ToDouble(SittingStaminaIncrease);
                SprintingStaminaDecrease = (float)iniParser.Get(IniSection, nameof(SprintingStaminaDecrease)).ToDouble(SprintingStaminaDecrease);
                SprintingWaterDecrease = (float)iniParser.Get(IniSection, nameof(SprintingWaterDecrease)).ToDouble(SprintingWaterDecrease);
                StandingStaminaIncrease = (float)iniParser.Get(IniSection, nameof(StandingStaminaIncrease)).ToDouble(StandingStaminaIncrease);
                WalkingStaminaIncrease = (float)iniParser.Get(IniSection, nameof(WalkingStaminaIncrease)).ToDouble(WalkingStaminaIncrease);


                // Get the player list from the ini and split it to an array
                playerExceptions = iniParser.Get(IniSection, nameof(playerExceptions)).ToString().Trim().Split('\n')
                    .Select(config =>
                    {
                        ulong playerId;
                        return ulong.TryParse(config, out playerId) ? playerId : 0;
                    })
                    .Where(playerId => playerId > 0)
                    .ToList();

            }

            // Saves configuration to the ini parser
            public void SaveConfig(MyIni iniParser)
            {
                iniParser.Set(IniSection, nameof(staminadrainmultiplier), staminadrainmultiplier);
                iniParser.Set(IniSection, nameof(fatiguedrainmultiplier), fatiguedrainmultiplier);
                iniParser.Set(IniSection, nameof(healthdrainmultiplier), healthdrainmultiplier);
                iniParser.Set(IniSection, nameof(hungerdrainmultiplier), hungerdrainmultiplier);
                iniParser.Set(IniSection, nameof(waterdrainmultiplier), waterdrainmultiplier);
                iniParser.Set(IniSection, nameof(sanitydrainmultiplier), sanitydrainmultiplier);

                iniParser.Set(IniSection, nameof(staminaincreasemultiplier), staminaincreasemultiplier);
                iniParser.Set(IniSection, nameof(fatigueincreasemultiplier), fatigueincreasemultiplier);
                iniParser.Set(IniSection, nameof(healthincreasemultiplier), healthincreasemultiplier);
                iniParser.Set(IniSection, nameof(hungerincreasemultiplier), hungerincreasemultiplier);
                iniParser.Set(IniSection, nameof(waterincreasemultiplier), waterincreasemultiplier);
                iniParser.Set(IniSection, nameof(sanityincreasemultiplier), sanityincreasemultiplier);

                iniParser.Set(IniSection, nameof(ProcessSittingEffectMultiplier), ProcessSittingEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessStandingEffectMultiplier), ProcessStandingEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessCrouchingEffectMultiplier), ProcessCrouchingEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessCrouchWalkingEffectMultiplier), ProcessCrouchWalkingEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessWalkingEffectMultiplier), ProcessWalkingEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessRunningEffectMultiplier), ProcessRunningEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessLadderEffectMultiplier), ProcessLadderEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessFlyingEffectMultiplier), ProcessFlyingEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessFallingEffectMultiplier), ProcessFallingEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessSprintingEffectMultiplier), ProcessSprintingEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessJumpingEffectMultiplier), ProcessJumpingEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessDefaultMovementEffectMultiplier), ProcessDefaultMovementEffectMultiplier);
                iniParser.Set(IniSection, nameof(ProcessHealthAndSanityEffectsMultiplier), ProcessHealthAndSanityEffectsMultiplier);
                iniParser.Set(IniSection, nameof(ProcessOrganicCollectionMultiplier), ProcessOrganicCollectionMultiplier);

                iniParser.Set(IniSection, nameof(FallingStaminaDecrease), FallingStaminaDecrease);
                iniParser.Set(IniSection, nameof(CrouchingFatigueIncrease), CrouchingFatigueIncrease);
                iniParser.Set(IniSection, nameof(CrouchingStaminaIncrease), CrouchingStaminaIncrease);
                iniParser.Set(IniSection, nameof(CrouchWalkStaminaIncrease), CrouchWalkStaminaIncrease);
                iniParser.Set(IniSection, nameof(FlyingStaminaDecrease), FlyingStaminaDecrease);
                iniParser.Set(IniSection, nameof(JumpingStaminaDecrease), JumpingStaminaDecrease);
                iniParser.Set(IniSection, nameof(LadderStaminaDecrease), LadderStaminaDecrease);
                iniParser.Set(IniSection, nameof(RunningStaminaDecrease), RunningStaminaDecrease);
                iniParser.Set(IniSection, nameof(SittingFatigueIncrease), SittingFatigueIncrease);
                iniParser.Set(IniSection, nameof(SittingStaminaIncrease), SittingStaminaIncrease);
                iniParser.Set(IniSection, nameof(SprintingStaminaDecrease), SprintingStaminaDecrease);
                iniParser.Set(IniSection, nameof(SprintingWaterDecrease), SprintingWaterDecrease);
                iniParser.Set(IniSection, nameof(StandingStaminaIncrease), StandingStaminaIncrease);
                iniParser.Set(IniSection, nameof(WalkingStaminaIncrease), WalkingStaminaIncrease);


                iniParser.Set(IniSection, nameof(playerExceptions), string.Join("\n", playerExceptions.Distinct().ToArray()));
                iniParser.SetComment(IniSection, nameof(playerExceptions), "Add the IDs of players who should be exempt from the iSurvival mod");


            }

            // Saves configuration after chat update
            public void SaveConfigAfterChatUpdate()
            {
                MyIni iniParser = new MyIni();
                SaveConfig(iniParser);
                SaveToStorage(iniParser.ToString());
            }

            // Saves configuration to world storage
            private void SaveToStorage(string saveText)
            {
                MyAPIGateway.Utilities.SetVariable<string>(VariableId, saveText);

                using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(iSurvivalSettings)))
                {
                    file.Write(saveText);
                }
            }
        }
    }

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class iSurvivalSession : MySessionComponentBase
    {
        public static float previousStaminaValue = 0;
        public static ushort modId = 19008;
        public static int runCount = 0;
        public static Random rand = new Random();
        public static List<long> blinkList = new List<long>();
        public static int loadWait = 120;

        public static float playerinventoryfillfactor = 0.0f;

        protected override void UnloadData()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(modId, OnMessageReceived);
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(modId, OnMessageReceived);
        }

        // Main update method called every simulation frame
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

                foreach (var playerId in blinkList)
                {
                    if (playerId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                        blink(false);
                    else
                        MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("unblink"), MyVisualScriptLogicProvider.GetSteamId(playerId), true);
                }
                blinkList.Clear();

                if (runCount % 60 > 0) // Run every second
                    return;

                ProcessPlayers();

                if (runCount > 299)
                    runCount = 0;
            }
            catch (Exception ex)
            {
                Echo("iSurvival error", ex.ToString());
            }
        }

        // Processes all players in the game
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

                ProcessPlayerMovement(player, statComp, stamina, fatigue, hunger, water, sanity, health);
            }
        }

        // Processes the player's movement and updates stats accordingly
        private void ProcessPlayerMovement(IMyPlayer player, MyEntityStatComponent statComp, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {

            var block = player.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
            if (block != null)
            {
                ProcessBlockEffects(player, statComp, block, stamina, fatigue, water, sanity, hunger, health);
                return;
            }

            ProcessMovementEffects(player, statComp, stamina, fatigue, hunger, water, sanity, health);
            ProcessNormalEffects(player, statComp, stamina, fatigue, hunger, water, sanity, health);
        }
        private void ProcessNormalEffects(IMyPlayer player, MyEntityStatComponent statComp, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            ProcessHealthAndSanityEffects(player, stamina, fatigue, hunger, water, sanity, health);
            ProcessOrganicCollection(player, hunger, water);
            ConstantStaminaRecovery(stamina, hunger, water, fatigue, health, sanity);
            
        }

        // Applies effects if the player is in certain blocks (CryoChamber, Toilet, etc.)
        private void ProcessBlockEffects(IMyPlayer player, MyEntityStatComponent statComp, IMyCubeBlock block, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat water, MyEntityStat sanity, MyEntityStat hunger, MyEntityStat health)
        {
            var blockDef = block.BlockDefinition.SubtypeId.ToString();
            if (blockDef.Contains("Bed"))
            {
                stamina.Increase(2f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                fatigue.Increase(1f * iSurvivalSessionSettings.fatigueincreasemultiplier, null);
                if (sanity.Value < 50) sanity.Increase(0.5f * iSurvivalSessionSettings.sanityincreasemultiplier, null);
            }
            else if (blockDef.Contains("Cryo"))
            {
                return;
            }
            else if (blockDef.Contains("Toilet") || blockDef.Contains("Bathroom"))
            {
                fatigue.Increase(1f, null);
                stamina.Increase(1f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                if (hunger.Value > 20)
                {
                    hunger.Decrease(2f * iSurvivalSessionSettings.hungerdrainmultiplier, null);
                    player.Character.GetInventory(0).AddItems((MyFixedPoint)0.1f, (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                }
            }
            else ProcessSittingEffect(player, statComp, stamina, fatigue, hunger, water, sanity, health);
            {
            }
        }


        // Applies effects based on the player's movement state
        private void ProcessMovementEffects(IMyPlayer player, MyEntityStatComponent statComp, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            var movementState = player.Character.CurrentMovementState;
            switch (movementState)
            {
                //case MyCharacterMovementEnum.Sitting:
                //    ProcessSittingEffect(player, stamina, fatigue);
                //    break;
                case MyCharacterMovementEnum.Standing:
                case MyCharacterMovementEnum.RotatingLeft:
                case MyCharacterMovementEnum.RotatingRight:
                    ProcessStandingEffect(player, stamina);
                    break;
                case MyCharacterMovementEnum.Sprinting:
                    ProcessSprintingEffect(player, stamina, water);
                    break;
                case MyCharacterMovementEnum.Crouching:
                case MyCharacterMovementEnum.CrouchRotatingLeft:
                case MyCharacterMovementEnum.CrouchRotatingRight:
                    ProcessCrouchingEffect(player, stamina, fatigue);
                    break;
                case MyCharacterMovementEnum.CrouchWalking:
                case MyCharacterMovementEnum.CrouchBackWalking:
                case MyCharacterMovementEnum.CrouchWalkingLeftBack:
                case MyCharacterMovementEnum.CrouchWalkingLeftFront:
                case MyCharacterMovementEnum.CrouchWalkingRightBack:
                case MyCharacterMovementEnum.CrouchWalkingRightFront:
                case MyCharacterMovementEnum.CrouchStrafingLeft:
                case MyCharacterMovementEnum.CrouchStrafingRight:
                    ProcessCrouchWalkingEffect(player, stamina);
                    break;
                case MyCharacterMovementEnum.Walking:
                case MyCharacterMovementEnum.BackWalking:
                case MyCharacterMovementEnum.WalkStrafingLeft:
                case MyCharacterMovementEnum.WalkStrafingRight:
                case MyCharacterMovementEnum.WalkingRightBack:
                case MyCharacterMovementEnum.WalkingRightFront:
                    ProcessWalkingEffect(player, stamina);
                    break;
                case MyCharacterMovementEnum.Running:
                case MyCharacterMovementEnum.Backrunning:
                case MyCharacterMovementEnum.RunningLeftBack:
                case MyCharacterMovementEnum.RunningLeftFront:
                case MyCharacterMovementEnum.RunningRightBack:
                case MyCharacterMovementEnum.RunningRightFront:
                case MyCharacterMovementEnum.RunStrafingLeft:
                case MyCharacterMovementEnum.RunStrafingRight:
                    ProcessRunningEffect(player, stamina);
                    break;
                case MyCharacterMovementEnum.LadderUp:
                case MyCharacterMovementEnum.LadderDown:
                    ProcessLadderEffect(player, stamina);
                    break;
                case MyCharacterMovementEnum.Flying:
                    ProcessFlyingEffect(player, stamina);
                    break;
                case MyCharacterMovementEnum.Falling:
                    ProcessFallingEffect(player, stamina);
                    break;
                case MyCharacterMovementEnum.Jump:
                    ProcessJumpingEffect(player, stamina);
                    break;
                default:
                    ProcessDefaultMovementEffect(player, stamina, water, hunger, fatigue, sanity);
                    break;
            }

        }


        // START Movement Processing
        private void ProcessSittingEffect(IMyPlayer player, MyEntityStatComponent statComp, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {

            var averageVitalStats = ((hunger.Value + sanity.Value + water.Value + (fatigue.Value * 2) + (health.Value / 2)) / 5);
            if (sanity.Value < 25) sanity.Increase((averageVitalStats / 100) * iSurvivalSessionSettings.sanityincreasemultiplier, null);
            if (stamina.Value < 100) stamina.Increase((averageVitalStats / 100) * iSurvivalSessionSettings.sanityincreasemultiplier, null);
            if (fatigue.Value < 100) fatigue.Increase((averageVitalStats / 200) * iSurvivalSessionSettings.fatigueincreasemultiplier, null);
            MyAPIGateway.Utilities.ShowMessage("sitting:", $"{averageVitalStats} = {(averageVitalStats / 100)}");

            ProcessNormalEffects(player, statComp, stamina, fatigue, hunger, water, sanity, health);
        }

        private void ProcessStandingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            stamina.Increase(iSurvivalSessionSettings.StandingStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier * iSurvivalSessionSettings.ProcessStandingEffectMultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Standing", "Stamina increased");
        }

        private void ProcessCrouchingEffect(IMyPlayer player, MyEntityStat stamina, MyEntityStat fatigue)
        {
            stamina.Increase(iSurvivalSessionSettings.CrouchingStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier * iSurvivalSessionSettings.ProcessCrouchingEffectMultiplier, null);
            fatigue.Increase(iSurvivalSessionSettings.CrouchingFatigueIncrease * iSurvivalSessionSettings.fatigueincreasemultiplier * iSurvivalSessionSettings.ProcessCrouchingEffectMultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Crouching", "Stamina increased, Fatigue increased");
        }

        private void ProcessCrouchWalkingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            float playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
            stamina.Increase(iSurvivalSessionSettings.CrouchWalkStaminaIncrease * (1 + playerinventoryfillfactor) * iSurvivalSessionSettings.staminaincreasemultiplier * iSurvivalSessionSettings.ProcessCrouchWalkingEffectMultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Crouch Walking", "Stamina increased");
        }

        private void ProcessWalkingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            float playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
            stamina.Increase(iSurvivalSessionSettings.WalkingStaminaIncrease * (1 + playerinventoryfillfactor) * iSurvivalSessionSettings.staminaincreasemultiplier * iSurvivalSessionSettings.ProcessWalkingEffectMultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Walking", "Stamina increased");
        }

        private void ProcessRunningEffect(IMyPlayer player, MyEntityStat stamina)
        {
            float playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
            stamina.Decrease(iSurvivalSessionSettings.RunningStaminaDecrease * (1 + playerinventoryfillfactor) * iSurvivalSessionSettings.staminadrainmultiplier * iSurvivalSessionSettings.ProcessRunningEffectMultiplier, null);
            //MyAPIGateway.Utilities.ShowMessage("Running", $"fillfactor {playerinventoryfillfactor} = {playerinventoryfillfactor + iSurvivalSessionSettings.RunningStaminaDecrease}");
        }

        private void ProcessLadderEffect(IMyPlayer player, MyEntityStat stamina)
        {
            float playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
            stamina.Decrease(iSurvivalSessionSettings.LadderStaminaDecrease * (1 + playerinventoryfillfactor) * iSurvivalSessionSettings.staminadrainmultiplier * iSurvivalSessionSettings.ProcessLadderEffectMultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Ladder", "Stamina decreased, Water decreased");
        }

        private void ProcessFlyingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            stamina.Decrease(iSurvivalSessionSettings.FlyingStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier * iSurvivalSessionSettings.ProcessFlyingEffectMultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Flying", "Stamina decreased, Water decreased");
        }

        private void ProcessFallingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            float playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
            stamina.Decrease(iSurvivalSessionSettings.FallingStaminaDecrease * (1 + playerinventoryfillfactor) * iSurvivalSessionSettings.staminadrainmultiplier * iSurvivalSessionSettings.ProcessFallingEffectMultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Falling", "Stamina decreased, Water decreased");
        }

        private void ProcessSprintingEffect(IMyPlayer player, MyEntityStat stamina, MyEntityStat water)
        {
            if (stamina.Value > 0)
            {
                float playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
                stamina.Decrease(iSurvivalSessionSettings.SprintingStaminaDecrease * (1 + playerinventoryfillfactor) * iSurvivalSessionSettings.staminadrainmultiplier * iSurvivalSessionSettings.ProcessSprintingEffectMultiplier, null);
                water.Decrease(iSurvivalSessionSettings.SprintingWaterDecrease * iSurvivalSessionSettings.waterdrainmultiplier * iSurvivalSessionSettings.ProcessSprintingEffectMultiplier, null);

                //MyAPIGateway.Utilities.ShowMessage("Sprinting", "Stamina decreased, Water decreased");
            }
        }

        private void ProcessJumpingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            if (stamina.Value > 0)
            {
                float playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
                stamina.Decrease(iSurvivalSessionSettings.JumpingStaminaDecrease * (1 + playerinventoryfillfactor) * iSurvivalSessionSettings.staminadrainmultiplier * iSurvivalSessionSettings.ProcessJumpingEffectMultiplier, null);

                //MyAPIGateway.Utilities.ShowMessage("Jumping", "Stamina decreased, Water decreased");
            }
        }

        private void ProcessDefaultMovementEffect(IMyPlayer player, MyEntityStat stamina, MyEntityStat water, MyEntityStat hunger, MyEntityStat fatigue, MyEntityStat sanity)
        {
            if (stamina.Value > 0)
            {
                float playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
                float staminaEffect = (1 + playerinventoryfillfactor) * (1 + hunger.Value / 100) * iSurvivalSessionSettings.staminadrainmultiplier;
                float waterEffect = (1 + playerinventoryfillfactor) * (1 + fatigue.Value / 100) * iSurvivalSessionSettings.waterdrainmultiplier;

                stamina.Decrease(staminaEffect, null);
                water.Decrease(waterEffect, null);
            }
        }
        // Sanity balance mechanic
        private void UpdateSanity(IMyPlayer player, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat health, MyEntityStat sanity)
        {

            float environmentalFactor = WeatherEffects.GetEnvironmentalFactor(player);
            // Calculate the average of the vital stats
            var averageVitalStats = ((hunger.Value + sanity.Value + water.Value + fatigue.Value + (health.Value / 2)) / 5);
            var decreaseFactor = (100 - averageVitalStats) / 100;
            var oxygenLevel = OxygenLevelEnvironmentalFactor(player);
            if (oxygenLevel < 0.8)
            {
                sanity.Decrease((decreaseFactor / 100) * iSurvivalSessionSettings.sanitydrainmultiplier * environmentalFactor, null);
                MyAPIGateway.Utilities.ShowMessage("Sanity:", $"oxygen: {oxygenLevel} = {(decreaseFactor / 10)* environmentalFactor} env:{environmentalFactor}");
            }
            // If all vital stats are above 50, increase sanity if it's below 50
            if (averageVitalStats > 50 && sanity.Value < 25)
            {
                sanity.Increase((averageVitalStats / 1000) * iSurvivalSessionSettings.sanityincreasemultiplier, null);
            }

            // If any vital stat is below 50, decrease sanity if it's above 50
            if (averageVitalStats < 75 && sanity.Value > 25)
            {
                sanity.Decrease((averageVitalStats / 1000) * iSurvivalSessionSettings.sanitydrainmultiplier * environmentalFactor, null);
            }

            // Display sanity update message
            //MyAPIGateway.Utilities.ShowMessage("Sanity:", $"{averageVitalStats} = {(averageVitalStats / 1000)}");
        }

        private void ConstantStaminaRecovery(MyEntityStat stamina, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat health, MyEntityStat sanity)
        {
            // Constant stamina recovery based on sanity and food levels
            if (stamina.Value < 100)
            {
                var averageVitalStats = ((hunger.Value + sanity.Value + water.Value + (fatigue.Value * 2) + (health.Value / 2)) / 5);

                stamina.Increase((averageVitalStats / 50) * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                //MyAPIGateway.Utilities.ShowMessage("Constant:", $"{averageVitalStats} = {(averageVitalStats / 50)}");
            }
        }
        // Processes effects on health and sanity based on other stats
        private void ProcessHealthAndSanityEffects(IMyPlayer player, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            UpdateSanity(player, hunger, water, fatigue, health, sanity);
            float environmentalFactor = WeatherEffects.GetEnvironmentalFactor(player);

            // Decrease health and sanity if all critical stats are very low
            if (hunger.Value < 1 && fatigue.Value < 1 && water.Value < 1 && sanity.Value < 1)
            {
                health.Decrease(5 * iSurvivalSessionSettings.healthdrainmultiplier * environmentalFactor, null);
                sanity.Decrease(5 * iSurvivalSessionSettings.sanitydrainmultiplier * environmentalFactor, null);
                //MyAPIGateway.Utilities.ShowMessage("Health", "Health and sanity decreased due to critical levels of hunger, fatigue, water, and sanity.");
            }
            // Decrease health if any of the critical stats are very low
            else if (hunger.Value < 1 || fatigue.Value < 1 || water.Value < 1 || sanity.Value < 1)
            {
                health.Decrease(1 * iSurvivalSessionSettings.healthdrainmultiplier * environmentalFactor, null);
                sanity.Decrease(1 * iSurvivalSessionSettings.sanitydrainmultiplier * environmentalFactor, null);
                //MyAPIGateway.Utilities.ShowMessage("Health", "Health decreased due to low levels of hunger, fatigue, water, or sanity.");
            }


            // Calculate the dynamic fatigue drain based on stamina and sanity if stamina is actively draining
            if (stamina.Value < 1)
            {
                health.Decrease(5 * iSurvivalSessionSettings.healthdrainmultiplier * environmentalFactor, null);
                sanity.Decrease(5 * iSurvivalSessionSettings.sanitydrainmultiplier * environmentalFactor, null);
                //MyAPIGateway.Utilities.ShowMessage("Health", "Health and sanity decreased due to critically low stamina.");
            }
            if (stamina.Value < previousStaminaValue)
            {
                float sanityMultiplier = 1f;
                if (sanity.Value > 50)
                {
                    sanityMultiplier = 1 - ((sanity.Value - 50) / 100);
                }
                else
                {
                    sanityMultiplier = 1 + ((50 - sanity.Value) / 50);
                }

                float dynamicFatigueDrain = ((100 - stamina.Value) / 100f) * iSurvivalSessionSettings.fatiguedrainmultiplier * sanityMultiplier;
                fatigue.Decrease(dynamicFatigueDrain * environmentalFactor, null);

                // Further decrease health and sanity if stamina is critically low

            }

            // Update previous stamina value
            previousStaminaValue = stamina.Value;

            //Increase health and decrease hunger if hunger is above 20 and health is below 100
            if (hunger.Value > 20 && health.Value < 100)
            {
                health.Increase(1 * iSurvivalSessionSettings.healthincreasemultiplier * environmentalFactor, null);
                hunger.Decrease(1 * iSurvivalSessionSettings.hungerdrainmultiplier * environmentalFactor, null);
                sanity.Decrease(1 * iSurvivalSessionSettings.sanitydrainmultiplier * environmentalFactor, null);
                //MyAPIGateway.Utilities.ShowMessage("Health", "Health increased and hunger decreased.");
            }

            // Decrease fatigue and increase stamina based on hunger
            //if (fatigue.Value > 0 && stamina.Value < 100)
            //{
            //    fatigue.Decrease(((100 - hunger.Value) / 100) * iSurvivalSessionSettings.fatiguedrainmultiplier, null);
            //    stamina.Increase(2 * ((100 - hunger.Value) / 100) * iSurvivalSessionSettings.staminaincreasemultiplier, null);
            //    //MyAPIGateway.Utilities.ShowMessage("Health", "Fatigue decreased and stamina increased.");
            //}

            // Return early if not enough time has passed
            if (runCount < 300) return;


            // Total number of updates (ticks) for each time period
            float totalUpdatesHunger = 2700 * 4; // 45 minutes
            float totalUpdatesFatigue = 1800 * 4; // 30 minutes
            float totalUpdatesSanity = 7200 * 4; // 2 hours
            float totalUpdatesWater = 1200 * 4; // 20 minutes

            // Overtime loss rates per update
            float hungerOvertimeRate = 1f / totalUpdatesHunger;
            float fatigueOvertimeRate = 1f / totalUpdatesFatigue;
            float sanityOvertimeRate = 1f / totalUpdatesSanity;
            float waterOvertimeRate = 1f / totalUpdatesWater;

            // Apply overtime loss to stats
            hunger.Decrease(hungerOvertimeRate * iSurvivalSessionSettings.hungerdrainmultiplier * environmentalFactor, null);
            fatigue.Decrease(fatigueOvertimeRate * iSurvivalSessionSettings.fatiguedrainmultiplier * environmentalFactor, null);
            sanity.Decrease(sanityOvertimeRate * iSurvivalSessionSettings.sanitydrainmultiplier * environmentalFactor, null);
            water.Decrease(waterOvertimeRate * iSurvivalSessionSettings.waterdrainmultiplier * environmentalFactor, null);

            // Hunger, sanity, and water decrease faster if stamina and fatigue are low

            if (stamina.Value < 100 && fatigue.Value < 100)
            {
                float fatigueMultiplier = (100 - fatigue.Value) / 100;
                float sanityRecoveryMultiplier = (sanity.Value > 50) ? 1 - ((sanity.Value - 50) / 100) : 1 + ((50 - sanity.Value) / 50);
                float foodRecoveryMultiplier = hunger.Value / 100;
                float waterMultiplier = water.Value / 100;

                // Calculate the rates for hunger, sanity, and water decrease
                float hungerDecreaseRate = iSurvivalSessionSettings.hungerdrainmultiplier * fatigueMultiplier * sanityRecoveryMultiplier * foodRecoveryMultiplier * waterMultiplier * environmentalFactor;
                float sanityDecreaseRate = iSurvivalSessionSettings.sanitydrainmultiplier * fatigueMultiplier * sanityRecoveryMultiplier * foodRecoveryMultiplier * waterMultiplier * environmentalFactor;
                float waterDecreaseRate = iSurvivalSessionSettings.waterdrainmultiplier * fatigueMultiplier * sanityRecoveryMultiplier * foodRecoveryMultiplier * waterMultiplier * environmentalFactor;

                // Decrease hunger, sanity, and water
                hunger.Decrease(hungerDecreaseRate, null);
                sanity.Decrease(sanityDecreaseRate, null);
                water.Decrease(waterDecreaseRate, null);
            }


            // Random chance to blink based on fatigue
            if (fatigue.Value > 20 || rand.Next((int)fatigue.Value) > 0) return;
            Blink(player);
        }
        private void Blink(IMyPlayer player)
        {
            blinkList.Add(player.IdentityId);
            if (player.IdentityId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                blink(true);
            else if (MyAPIGateway.Multiplayer.IsServer)
                MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("blink"), MyVisualScriptLogicProvider.GetSteamId(player.IdentityId), true);

        }


        private void ProcessOrganicCollection(IMyPlayer player, MyEntityStat hunger, MyEntityStat water)
        {
            string recycleItem = "MyObjectBuilder_Component/SteelPlate";
            var inventory = player.Character.GetInventory();
            MyDefinitionId requiredItemDefinition = MyDefinitionId.Parse($"{recycleItem}"); // Change this to the required item
            MyFixedPoint requiredItemAmount = 1; // Set the required amount

            if (inventory.ContainItems(requiredItemAmount, requiredItemDefinition))
            {
                if (hunger.Value > 20 && water.Value > 20)
                {
                    float organicsAmount = (100 - hunger.Value) / 100f + (100 - water.Value) / 100f;
                    inventory.RemoveItemsOfType(requiredItemAmount, requiredItemDefinition);

                    inventory.AddItems((MyFixedPoint)organicsAmount, (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                    inventory.AddItems((MyFixedPoint)organicsAmount, (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Ice")));

                    //MyAPIGateway.Utilities.ShowMessage("Organics", $"Collected {organicsAmount} organics and ice based on hunger and water levels.");
                }
            }
            else
            {
                //MyAPIGateway.Utilities.ShowMessage("Organics", "You need a SteelPlate in your inventory to collect organics and ice.");
            }
        }

        public class WeatherEffects
        {
            public static float GetEnvironmentalFactor(IMyPlayer player)
            {
                Vector3D playerPosition = player.GetPosition();
                string currentWeather = MyVisualScriptLogicProvider.GetWeather(playerPosition);
                MyAPIGateway.Utilities.ShowMessage("Weather", $"Current: {currentWeather}");

                if (currentWeather.ToLower().Contains("clear"))
                {
                    return 1.0f; // No effect
                }
                else if (currentWeather.ToLower().Contains("rain"))
                {
                    return 1.1f; // Slightly increased drain due to discomfort
                }
                else if (currentWeather.ToLower().Contains("storm"))
                {
                    return 1.3f; // Higher drain due to harsh conditions
                }
                else if (currentWeather.ToLower().Contains("sand"))
                {
                    return 1.5f; // Much higher drain, especially for water
                }
                else if (currentWeather.ToLower().Contains("snow"))
                {
                    return 1.2f; // Increased drain due to cold stress
                }
                else
                {
                    return 1.0f; // Default effect
                }
            }
        }

        private double OxygenLevelEnvironmentalFactor(IMyPlayer player)
{
            double oxygenLevel = MyAPIGateway.Session.Player.Character.OxygenLevel;
            MyAPIGateway.Utilities.ShowMessage("Weather", $"Current: {oxygenLevel}");
            return oxygenLevel;




        }

        // END Movement Processing
        // Background process for receiving messages
        private void OnMessageReceived(ushort modId, byte[] data, ulong sender, bool reliable)
        {
            try
            {
                var msg = Encoding.ASCII.GetString(data);
                blink(msg == "blink");
            }
            catch (Exception ex)
            {
                Echo("Fatigue exception", ex.ToString());
            }
        }

        // Triggers the blink effect
        public void blink(bool blink)
        {
            MyVisualScriptLogicProvider.ScreenColorFadingSetColor(Color.Black, 0L);
            MyVisualScriptLogicProvider.ScreenColorFadingStart(0.25f, blink, 0L);
        }

        // Retrieves the specified stat from the player's stat component
        private MyEntityStat GetPlayerStat(MyEntityStatComponent statComp, string statName)
        {
            MyEntityStat stat;
            statComp.TryGetStat(MyStringHash.GetOrCompute(statName), out stat);
            return stat;
        }

        // Logs messages to the console
        public static void Echo(string msg1, string msg2 = "")
        {
            MyLog.Default.WriteLineAndConsole($"{msg1}: {msg2}");
        }
    }

    // HUD Stat for Player Fatigue
    public class MyStatPlayerFatigue : IMyHudStat
    {
        public MyStatPlayerFatigue()
        {
            Id = MyStringHash.GetOrCompute("player_fatigue");
        }

        private float m_currentValue;
        private string m_valueStringCache;

        public MyStringHash Id { get; protected set; }

        public float CurrentValue
        {
            get { return m_currentValue; }
            protected set
            {
                if (m_currentValue == value)
                    return;
                m_currentValue = value;
                m_valueStringCache = null;
            }
        }

        public virtual float MaxValue => 1f;
        public virtual float MinValue => 0.0f;

        public string GetValueString()
        {
            if (m_valueStringCache == null)
                m_valueStringCache = ToString();
            return m_valueStringCache;
        }

        public void Update()
        {
            MyEntityStatComponent statComp = MyAPIGateway.Session.Player?.Character?.Components.Get<MyEntityStatComponent>();
            if (statComp == null)
                return;
            MyEntityStat Fatigue;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Fatigue"), out Fatigue))
                CurrentValue = Fatigue.Value / Fatigue.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Water
    public class MyStatPlayerWater : IMyHudStat
    {
        public MyStatPlayerWater()
        {
            Id = MyStringHash.GetOrCompute("player_water");
        }

        private float m_currentValue;
        private string m_valueStringCache;

        public MyStringHash Id { get; protected set; }

        public float CurrentValue
        {
            get { return m_currentValue; }
            protected set
            {
                if (m_currentValue == value)
                    return;
                m_currentValue = value;
                m_valueStringCache = null;
            }
        }

        public virtual float MaxValue => 1f;
        public virtual float MinValue => 0.0f;

        public string GetValueString()
        {
            if (m_valueStringCache == null)
                m_valueStringCache = ToString();
            return m_valueStringCache;
        }

        public void Update()
        {
            MyEntityStatComponent statComp = MyAPIGateway.Session.Player?.Character?.Components.Get<MyEntityStatComponent>();
            if (statComp == null)
                return;
            MyEntityStat Water;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Water"), out Water))
                CurrentValue = Water.Value / Water.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Hunger
    public class MyStatPlayerHunger : IMyHudStat
    {
        public MyStatPlayerHunger()
        {
            Id = MyStringHash.GetOrCompute("player_hunger");
        }

        private float m_currentValue;
        private string m_valueStringCache;

        public MyStringHash Id { get; protected set; }

        public float CurrentValue
        {
            get { return m_currentValue; }
            protected set
            {
                if (m_currentValue == value)
                    return;
                m_currentValue = value;
                m_valueStringCache = null;
            }
        }

        public virtual float MaxValue => 1f;
        public virtual float MinValue => 0.0f;

        public string GetValueString()
        {
            if (m_valueStringCache == null)
                m_valueStringCache = ToString();
            return m_valueStringCache;
        }

        public void Update()
        {
            MyEntityStatComponent statComp = MyAPIGateway.Session.Player?.Character?.Components.Get<MyEntityStatComponent>();
            if (statComp == null)
                return;

            MyEntityStat Hunger;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Hunger"), out Hunger))
                CurrentValue = Hunger.Value / Hunger.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Stamina
    public class MyStatPlayerStamina : IMyHudStat
    {
        public MyStatPlayerStamina()
        {
            Id = MyStringHash.GetOrCompute("player_stamina");
        }

        private float m_currentValue;
        private string m_valueStringCache;

        public MyStringHash Id { get; protected set; }

        public float CurrentValue
        {
            get { return m_currentValue; }
            protected set
            {
                if (m_currentValue == value)
                    return;
                m_currentValue = value;
                m_valueStringCache = null;
            }
        }

        public virtual float MaxValue => 1f;
        public virtual float MinValue => 0.0f;

        public string GetValueString()
        {
            if (m_valueStringCache == null)
                m_valueStringCache = ToString();
            return m_valueStringCache;
        }

        public void Update()
        {
            MyEntityStatComponent statComp = MyAPIGateway.Session.Player?.Character?.Components.Get<MyEntityStatComponent>();
            if (statComp == null)
                return;
            MyEntityStat Stamina;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Stamina"), out Stamina))
                CurrentValue = Stamina.Value / Stamina.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Sanity
    public class MyStatPlayerSanity : IMyHudStat
    {
        public MyStatPlayerSanity()
        {
            Id = MyStringHash.GetOrCompute("player_sanity");
        }

        private float m_currentValue;
        private string m_valueStringCache;

        public MyStringHash Id { get; protected set; }

        public float CurrentValue
        {
            get { return m_currentValue; }
            protected set
            {
                if (m_currentValue == value)
                    return;
                m_currentValue = value;
                m_valueStringCache = null;
            }
        }

        public virtual float MaxValue => 1f;
        public virtual float MinValue => 0.0f;

        public string GetValueString()
        {
            if (m_valueStringCache == null)
                m_valueStringCache = ToString();
            return m_valueStringCache;
        }

        public void Update()
        {
            MyEntityStatComponent statComp = MyAPIGateway.Session.Player?.Character?.Components.Get<MyEntityStatComponent>();
            if (statComp == null)
                return;
            MyEntityStat Sanity;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Sanity"), out Sanity))
                CurrentValue = Sanity.Value / Sanity.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

}
