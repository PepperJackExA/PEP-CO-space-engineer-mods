using PEPCO.iSurvival.Chat;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

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

        //base movement numbers
        public static float FallingStaminaDecrease = 1.1f;
        public static float CrouchingFatigueIncrease = 1.5f;
        public static float CrouchingStaminaIncrease = 2;
        public static float CrouchWalkStaminaIncrease = 1.5f;
        public static float FlyingStaminaDecrease = 2;
        public static float JumpingStaminaDecrease = 1.1f;
        public static float LadderStaminaDecrease = 1.1f;
        public static float RunningStaminaDecrease = 2;
        public static float SittingFatigueIncrease = 5f;
        public static float SittingStaminaIncrease = 2;
        public static float SprintingStaminaDecrease = 4;
        public static float StandingStaminaIncrease = 0.5f;
        public static float WalkingStaminaIncrease = 0;

        public static float playerinventoryfillMultiplyer = 0;


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
            StandingStaminaIncrease = Settings.StandingStaminaIncrease;
            WalkingStaminaIncrease = Settings.WalkingStaminaIncrease;

            playerinventoryfillMultiplyer = Settings.playerinventoryfillMultiplyer;


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

            //base movemenet numbers
            public float FallingStaminaDecrease = 0.25f;
            public float CrouchingFatigueIncrease = 0.5f;
            public float CrouchingStaminaIncrease = 2.5f;
            public float CrouchWalkStaminaIncrease = 1f;
            public float FlyingStaminaDecrease = 0.25f;
            public float JumpingStaminaDecrease = 2f;
            public float LadderStaminaDecrease = 0.1f;
            public float RunningStaminaDecrease = 1f;
            public float SittingFatigueIncrease = 2f;
            public float SittingStaminaIncrease = 2.5f;
            public float SprintingStaminaDecrease = 4f;
            public float StandingStaminaIncrease = 1f;
            public float WalkingStaminaIncrease = 0.5f;

            //inventory multiplyer
            public float playerinventoryfillMultiplyer = 4f;



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
                StandingStaminaIncrease = (float)iniParser.Get(IniSection, nameof(StandingStaminaIncrease)).ToDouble(StandingStaminaIncrease);
                WalkingStaminaIncrease = (float)iniParser.Get(IniSection, nameof(WalkingStaminaIncrease)).ToDouble(WalkingStaminaIncrease);

                playerinventoryfillMultiplyer = (float)iniParser.Get(IniSection, nameof(playerinventoryfillMultiplyer)).ToDouble(playerinventoryfillMultiplyer);


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
                iniParser.Set(IniSection, nameof(StandingStaminaIncrease), StandingStaminaIncrease);
                iniParser.Set(IniSection, nameof(WalkingStaminaIncrease), WalkingStaminaIncrease);

                iniParser.Set(IniSection, nameof(playerinventoryfillMultiplyer), playerinventoryfillMultiplyer);


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
        public static float previousStaminaValueforFatigueUpdate = 0;
        public static float previousStaminaValueforHungerUpdate = 0;
        public static float previousStaminaValueforWaterUpdate = 0;

        public static ushort modId = 19008;
        public static int runCount = 0;
        public static Random rand = new Random();
        public static List<long> blinkList = new List<long>();
        public static int loadWait = 120;
        public static float currentAveragestats = 0;

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
        // Processes all players in the game
        private void ProcessPlayers()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players, p =>
                p.Character != null &&
                p.Character.ToString().Contains("Astronaut") &&
                p.Character.IsPlayer &&
                !p.Character.IsDead &&
                p.Character.Integrity > 0
            );

            foreach (IMyPlayer player in players)
            {
                // Ensure player and its character are valid
                if (player == null || player.Character == null) continue;

                // Skip players in the exception list
                if (iSurvivalSessionSettings.playerExceptions.Contains(player.SteamUserId))
                    continue;

                var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
                if (statComp == null) continue;

                MyEntityStat fatigue, hunger, stamina, health, water, sanity;
                if (!statComp.TryGetStat(MyStringHash.GetOrCompute("Fatigue"), out fatigue) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Hunger"), out hunger) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Stamina"), out stamina) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Health"), out health) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Water"), out water) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Sanity"), out sanity))
                    continue;

                currentAveragestats = (hunger.Value + (sanity.Value / 2) + water.Value + (fatigue.Value * 2) + (health.Value / 2)) / 5;
                // MyAPIGateway.Utilities.ShowMessage("iSurvival:", $"average: {currentAveragestats}");

                playerinventoryfillfactor = 1 + (player.Character.GetInventory()?.VolumeFillFactor ?? 0) * iSurvivalSessionSettings.playerinventoryfillMultiplyer;
                // MyAPIGateway.Utilities.ShowMessage("iSurvival:", $"GetInventory:{playerinventoryfillfactor}");

                ProcessPlayerMovement(player, statComp, stamina, fatigue, hunger, water, sanity, health);
            }
        }

        private float ProcessDrain(IMyPlayer player)
        {
            return 1 + ((100 - currentAveragestats) / 100) * playerinventoryfillfactor / (1 + OxygenLevelEnvironmentalFactor(player)) * GetEnvironmentalFactor(player);
        }

        private float ProcessIncrease(IMyPlayer player)
        {
            return 1 + (currentAveragestats / 100) / playerinventoryfillfactor * (1 + OxygenLevelEnvironmentalFactor(player)) / GetEnvironmentalFactor(player);
        }

        // Processes the player's movement and updates stats accordingly
        private void ProcessPlayerMovement(IMyPlayer player, MyEntityStatComponent statComp, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            //MyAPIGateway.Utilities.ShowMessage("Sanity:", $"Current:{sanity.Value}");


            var block = player.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
            if (block != null)
            {
                ProcessBlockEffects(player, statComp, block, stamina, fatigue, water, sanity, hunger, health);
                return;
            }

            ProcessMovementEffects(player, statComp, stamina, fatigue, hunger, water, sanity, health);
        }

        // Applies effects if the player is in certain blocks (CryoChamber, Toilet, etc.)
        private void ProcessBlockEffects(IMyPlayer player, MyEntityStatComponent statComp, IMyCubeBlock block, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat water, MyEntityStat sanity, MyEntityStat hunger, MyEntityStat health)
        {
            var blockDef = block.BlockDefinition.SubtypeId.ToString();
            if (blockDef.Contains("Cryo")) return;

            if (blockDef.Contains("Bed"))
            {
                stamina.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                fatigue.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.fatigueincreasemultiplier, null);
                if (sanity.Value < currentAveragestats)
                {
                    sanity.Increase(0.5f * iSurvivalSessionSettings.sanityincreasemultiplier, null);
                }
                return;
            }
            else if (blockDef.Contains("Toilet") || blockDef.Contains("Bathroom"))
            {
                fatigue.Increase(ProcessIncrease(player), null);
                stamina.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                if (hunger.Value > 20)
                {
                    hunger.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.hungerdrainmultiplier, null);
                    player.Character.GetInventory(0).AddItems((MyFixedPoint)(((100 - currentAveragestats) / 100) * iSurvivalSessionSettings.hungerdrainmultiplier), (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                }
            }
            else ProcessSittingEffect(player, statComp, stamina, fatigue, hunger, water, sanity, health);
        }


        // Applies effects based on the player's movement state
        private void ProcessMovementEffects(IMyPlayer player, MyEntityStatComponent statComp, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            ProcessWaterUpdateEffects(player, stamina, fatigue, hunger, water, sanity, health);
            ProcessHungerUpdateEffects(player, stamina, fatigue, hunger, water, sanity, health);
            ProcessFatigueUpdateEffects(player, stamina, fatigue, hunger, water, sanity, health);
            ProcessHealthAndSanityEffects(player, stamina, fatigue, hunger, water, sanity, health);
            var movementState = player.Character.CurrentMovementState;
            switch (movementState)
            {
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
                    // Handle other movement states or do nothing
                    break;
            }
        }

        // START Movement Processing
        private void ProcessSittingEffect(IMyPlayer player, MyEntityStatComponent statComp, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {

            ProcessWaterUpdateEffects(player, stamina, fatigue, hunger, water, sanity, health);
            ProcessHungerUpdateEffects(player, stamina, fatigue, hunger, water, sanity, health);
            ProcessFatigueUpdateEffects(player, stamina, fatigue, hunger, water, sanity, health);
            ProcessHealthAndSanityEffects(player, stamina, fatigue, hunger, water, sanity, health);

            if (fatigue.Value < currentAveragestats)
            {
                //MyAPIGateway.Utilities.ShowMessage("sitting:", $"Increase:{ProcessIncrease(player) * iSurvivalSessionSettings.SittingFatigueIncrease * iSurvivalSessionSettings.fatigueincreasemultiplier}");
                fatigue.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.SittingFatigueIncrease*iSurvivalSessionSettings.fatigueincreasemultiplier, null);
            }
            if (stamina.Value < currentAveragestats)
            {
                //MyAPIGateway.Utilities.ShowMessage("sitting:", $"Increase:{ProcessIncrease(player) * iSurvivalSessionSettings.SittingStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier}");
                stamina.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.SittingStaminaIncrease*iSurvivalSessionSettings.staminaincreasemultiplier, null);
            }

        }

        private void ProcessStandingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            if (stamina.Value < currentAveragestats)
            {
                //MyAPIGateway.Utilities.ShowMessage("Standing:", $"Increase:{ProcessIncrease(player) * iSurvivalSessionSettings.StandingStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier}");
                stamina.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.StandingStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier, null);
            }
        }

        private void ProcessCrouchingEffect(IMyPlayer player, MyEntityStat stamina, MyEntityStat fatigue)
        {
            if (stamina.Value < currentAveragestats)
            {
                //MyAPIGateway.Utilities.ShowMessage("Crouching:", $"Stamina Increase:{ProcessIncrease(player) * iSurvivalSessionSettings.CrouchingStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier}");
                stamina.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.CrouchingStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier, null);
            }
            if (fatigue.Value < currentAveragestats)
            {
                //MyAPIGateway.Utilities.ShowMessage("Crouching:", $"Fatigue Increase:{ProcessIncrease(player) * iSurvivalSessionSettings.CrouchingFatigueIncrease * iSurvivalSessionSettings.fatigueincreasemultiplier}");
                fatigue.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.CrouchingFatigueIncrease * iSurvivalSessionSettings.fatigueincreasemultiplier, null);
            }
        }

        private void ProcessCrouchWalkingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            if (stamina.Value < currentAveragestats)
            {
                //MyAPIGateway.Utilities.ShowMessage("CrouchWalking:", $"stamina Increase:{ProcessIncrease(player) * iSurvivalSessionSettings.CrouchingFatigueIncrease * iSurvivalSessionSettings.fatigueincreasemultiplier}");
                stamina.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.CrouchWalkStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier, null);
            }
            //MyAPIGateway.Utilities.ShowMessage("Crouch Walking", "Stamina increased");
        }

        private void ProcessWalkingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            if (stamina.Value < currentAveragestats)
            {
                //MyAPIGateway.Utilities.ShowMessage("Walking:", $"stamina Increase:{ProcessIncrease(player) * iSurvivalSessionSettings.WalkingStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier}");
                stamina.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.WalkingStaminaIncrease * iSurvivalSessionSettings.staminaincreasemultiplier, null);
            }
            //MyAPIGateway.Utilities.ShowMessage("Walking", "Stamina increased");
        }

        private void ProcessRunningEffect(IMyPlayer player, MyEntityStat stamina)
        {
            //MyAPIGateway.Utilities.ShowMessage("Running:", $"stamina Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.RunningStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier}");
            stamina.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.RunningStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier, null);

        }

        private void ProcessLadderEffect(IMyPlayer player, MyEntityStat stamina)
        {
            //MyAPIGateway.Utilities.ShowMessage("Ladder:", $"stamina Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.LadderStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier}");
            stamina.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.LadderStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Ladder", "Stamina decreased, Water decreased");
        }

        private void ProcessFlyingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            //MyAPIGateway.Utilities.ShowMessage("Flying:", $"stamina Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.FlyingStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier}");
            stamina.Decrease(ProcessDrain(player)* playerinventoryfillfactor * iSurvivalSessionSettings.FlyingStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Flying", "Stamina decreased, Water decreased");
        }

        private void ProcessFallingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            //MyAPIGateway.Utilities.ShowMessage("Falling:", $"stamina Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.FallingStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier}");
            stamina.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.FallingStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier, null);

            //MyAPIGateway.Utilities.ShowMessage("Falling", "Stamina decreased, Water decreased");
        }

        private void ProcessSprintingEffect(IMyPlayer player, MyEntityStat stamina, MyEntityStat water)
        {
            if (stamina.Value > 0)
            {
                //MyAPIGateway.Utilities.ShowMessage("Sprinting:", $"stamina Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.SprintingStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier}");
                stamina.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.SprintingStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier, null);

                //MyAPIGateway.Utilities.ShowMessage("Sprinting", "Stamina decreased, Water decreased");
            }
        }

        private void ProcessJumpingEffect(IMyPlayer player, MyEntityStat stamina)
        {
            if (stamina.Value > 0)
            {
                //MyAPIGateway.Utilities.ShowMessage("Jumping:", $"Stamina Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.JumpingStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier}");
                stamina.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.JumpingStaminaDecrease * iSurvivalSessionSettings.staminadrainmultiplier, null);

                //MyAPIGateway.Utilities.ShowMessage("Jumping", "Stamina decreased, Water decreased");
            }
        }

        private void ProcessDefaultMovementEffect(IMyPlayer player, MyEntityStat stamina, MyEntityStat water, MyEntityStat hunger, MyEntityStat fatigue, MyEntityStat sanity)
        {
            if (stamina.Value > 0)
            {
                //MyAPIGateway.Utilities.ShowMessage("Default:", $"Increase:{ProcessIncrease(player)} = Decrease:{ProcessDrain(player)}.... YOU SHOULD NOT SEE THIS let pepperjack Know what you did to get this!");
            }
        }



        // Processes effects on health and sanity based on other stats
        private void ProcessHealthAndSanityEffects(IMyPlayer player, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            // Sanity balance mechanic
            if (sanity.Value < currentAveragestats - 50)
            {
                //MyAPIGateway.Utilities.ShowMessage("UpdateSanity:", $"Sanity Increase:{(ProcessDrain(player) / 60) * iSurvivalSessionSettings.sanityincreasemultiplier}");
                sanity.Increase((ProcessDrain(player) / 60) * iSurvivalSessionSettings.sanityincreasemultiplier, null);
            }
            else if (sanity.Value > currentAveragestats - 50)
            {
                //MyAPIGateway.Utilities.ShowMessage("UpdateSanity:", $"Sanity Decrease:{(ProcessDrain(player) / 60) * iSurvivalSessionSettings.sanityincreasemultiplier}");
                sanity.Decrease((ProcessDrain(player) / 60) * iSurvivalSessionSettings.sanityincreasemultiplier, null);
            }

            // Decrease health if any of the critical stats are very low
            if (hunger.Value < 1)
            {
                health.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.healthdrainmultiplier, null);
                //MyAPIGateway.Utilities.ShowMessage("Health", "Health decreased due to low levels of hunger, fatigue, water, or sanity.");
            }
            if (fatigue.Value < 1)
            {
                health.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.healthdrainmultiplier, null);
                //MyAPIGateway.Utilities.ShowMessage("fatigue", "Health decreased due to low levels of hunger, fatigue, water, or sanity.");
            }
            if (water.Value < 1)
            {
                health.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.healthdrainmultiplier, null);
                //MyAPIGateway.Utilities.ShowMessage("water", "Health decreased due to low levels of hunger, fatigue, water, or sanity.");
            }
            if (sanity.Value < 1)
            {
                health.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.healthdrainmultiplier, null);
                //MyAPIGateway.Utilities.ShowMessage("sanity", "Health decreased due to low levels of hunger, fatigue, water, or sanity.");
            }
            if (stamina.Value < 1)
            {
                health.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.healthdrainmultiplier, null);
                //MyAPIGateway.Utilities.ShowMessage("Health", "Health and sanity decreased due to critically low stamina.");
            }


            //Increase health and decrease hunger if hunger is above 20 and health is below 100
            if ((health.Value < (currentAveragestats)))
            {
                //MyAPIGateway.Utilities.ShowMessage("regen:", $"Health Increase:{ProcessIncrease(player) * iSurvivalSessionSettings.healthincreasemultiplier}");
                health.Increase(ProcessIncrease(player) * iSurvivalSessionSettings.healthincreasemultiplier, null);

                //MyAPIGateway.Utilities.ShowMessage("regen:", $"Hunger Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.hungerdrainmultiplier}");
                hunger.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.hungerdrainmultiplier, null);

                //MyAPIGateway.Utilities.ShowMessage("regen:", $"Sanity Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.sanityincreasemultiplier}");
                sanity.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.sanitydrainmultiplier, null);

                //MyAPIGateway.Utilities.ShowMessage("regen:", $"Water Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.waterincreasemultiplier}");
                water.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.waterdrainmultiplier, null);

                //MyAPIGateway.Utilities.ShowMessage("regen:", $"Water Decrease:{ProcessDrain(player) * iSurvivalSessionSettings.fatigueincreasemultiplier}");
                fatigue.Decrease(ProcessDrain(player) * iSurvivalSessionSettings.fatiguedrainmultiplier, null);
                }
            if (runCount < 300) return;

            //StaminaLowEffect(player, stamina, fatigue, hunger, water, sanity, health);


            // Random chance to blink based on fatigue
            if ((sanity.Value > 0 && fatigue.Value > 20) || rand.Next((int)fatigue.Value) > 0) return;
            Blink(player);
        }

        private void ProcessFatigueUpdateEffects(IMyPlayer player, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            // constant fatigue drain
            fatigue.Decrease((ProcessDrain(player) / 60) * iSurvivalSessionSettings.fatiguedrainmultiplier, null);

            // extra if over averagestats
            if (fatigue.Value > (currentAveragestats - ((hunger.Value + water.Value) / 2)))
            {
                //MyAPIGateway.Utilities.ShowMessage("FatigueUpdate:", $"fatigue Decrease:{(ProcessDrain(player) / 60) * iSurvivalSessionSettings.fatiguedrainmultiplier}");
                fatigue.Decrease((ProcessDrain(player) / 60) * iSurvivalSessionSettings.fatiguedrainmultiplier, null);
            }

            // extra if using stamina
            if (stamina.Value < previousStaminaValueforFatigueUpdate)
            {
                //MyAPIGateway.Utilities.ShowMessage("previousStaminaValue:", $"fatigue Decrease:{(ProcessDrain(player) / 10) * (previousStaminaValue - stamina.Value)}");
                fatigue.Decrease((ProcessDrain(player) / 10) * (previousStaminaValueforFatigueUpdate - stamina.Value) * iSurvivalSessionSettings.fatiguedrainmultiplier, null);

            }
            previousStaminaValueforFatigueUpdate = stamina.Value;


        }
        private void ProcessWaterUpdateEffects(IMyPlayer player, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            // constant fatigue drain
            water.Decrease((ProcessDrain(player) / 60) / GetEnvironmentalFactor(player) * iSurvivalSessionSettings.waterdrainmultiplier, null);

            // extra if over averagestats
            if (water.Value > currentAveragestats)
            {
                //MyAPIGateway.Utilities.ShowMessage("FatigueUpdate:", $"fatigue Decrease:{(ProcessDrain(player) / 60) * iSurvivalSessionSettings.fatiguedrainmultiplier}");
                water.Decrease((ProcessDrain(player) / 60) / GetEnvironmentalFactor(player) * iSurvivalSessionSettings.waterdrainmultiplier, null);
            }

            // extra if using stamina
            if (stamina.Value < previousStaminaValueforWaterUpdate)
            {
                //MyAPIGateway.Utilities.ShowMessage("previousStaminaValue:", $"fatigue Decrease:{(ProcessDrain(player) / 10) * (previousStaminaValue - stamina.Value)}");
                water.Decrease((ProcessDrain(player) / 10) * (previousStaminaValueforWaterUpdate - stamina.Value) / GetEnvironmentalFactor(player) * iSurvivalSessionSettings.waterdrainmultiplier, null);

            }
            previousStaminaValueforWaterUpdate = stamina.Value;


        }
        private void ProcessHungerUpdateEffects(IMyPlayer player, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            // constant fatigue drain
            hunger.Decrease((ProcessDrain(player) / 60) / GetEnvironmentalFactor(player) * iSurvivalSessionSettings.hungerdrainmultiplier, null);

            // extra if over averagestats
            if (hunger.Value > currentAveragestats)
            {
                //MyAPIGateway.Utilities.ShowMessage("FatigueUpdate:", $"fatigue Decrease:{(ProcessDrain(player) / 60) * iSurvivalSessionSettings.fatiguedrainmultiplier}");
                hunger.Decrease((ProcessDrain(player) / 60) / GetEnvironmentalFactor(player) * iSurvivalSessionSettings.hungerdrainmultiplier, null);
            }

            previousStaminaValueforHungerUpdate = stamina.Value;


        }
        private void Blink(IMyPlayer player)
        {
            blinkList.Add(player.IdentityId);
            if (player.IdentityId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                blink(true);
            else if (MyAPIGateway.Multiplayer.IsServer)
                MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("blink"), MyVisualScriptLogicProvider.GetSteamId(player.IdentityId), true);

        }



        public static float GetEnvironmentalFactor(IMyPlayer player)
        {
            Vector3D playerPosition = player.GetPosition();
            string currentWeather = MyVisualScriptLogicProvider.GetWeather(playerPosition);
            //MyAPIGateway.Utilities.ShowMessage("Weather", $"{currentWeather}");

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

        private float OxygenLevelEnvironmentalFactor(IMyPlayer player)
        {
            // Ensure player and its character are valid
            if (player == null || player.Character == null) return 1.0f;

            // Ensure character has oxygen properties
            var character = player.Character;
            var oxygenComponent = character.Components?.Get<MyCharacterOxygenComponent>();

            // Return the oxygen level if available, otherwise return a default factor
            return oxygenComponent?.EnvironmentOxygenLevel ?? 1.0f;
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
