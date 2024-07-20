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
                ProcessBlockEffects(player, block, stamina, fatigue, water, sanity, hunger);
                return;
            }

            ProcessMovementEffects(player, statComp, stamina, fatigue, hunger, water, sanity, health);
        }

        // Applies effects if the player is in certain blocks (CryoChamber, Toilet, etc.)
        private void ProcessBlockEffects(IMyPlayer player, IMyCubeBlock block, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat water, MyEntityStat sanity, MyEntityStat hunger)
        {
            var blockDef = block.BlockDefinition.ToString();
            if (blockDef.Contains("CryoChamber"))
            {
                stamina.Increase(5f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                fatigue.Increase(5f * iSurvivalSessionSettings.fatigueincreasemultiplier, null);
                sanity.Increase(5f * iSurvivalSessionSettings.sanityincreasemultiplier, null);
            }
            else if (blockDef.Contains("Toilet") || blockDef.Contains("Bathroom"))
            {
                fatigue.Increase(10f, null);
                stamina.Increase(10f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                water.Increase(10f * iSurvivalSessionSettings.waterincreasemultiplier, null);
                sanity.Increase(10f * iSurvivalSessionSettings.sanityincreasemultiplier, null);
                if (hunger.Value > 20)
                {
                    hunger.Decrease(2f * iSurvivalSessionSettings.hungerdrainmultiplier, null);
                    player.Character.GetInventory(0).AddItems((MyFixedPoint)0.1f, (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                }
            }
        }

        // Applies effects based on the player's movement state
        private void ProcessMovementEffects(IMyPlayer player, MyEntityStatComponent statComp, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            var movementState = player.Character.CurrentMovementState;
            switch (movementState)
            {
                case MyCharacterMovementEnum.Sitting:
                    stamina.Increase(4f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                    fatigue.Increase(0.5f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                    break;
                case MyCharacterMovementEnum.Standing:
                    stamina.Increase(1.5f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                    break;
                case MyCharacterMovementEnum.Sprinting:
                    ProcessSprintingEffect(player, stamina, water);
                    break;
                case MyCharacterMovementEnum.Crouching:
                    stamina.Increase(2.5f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                    fatigue.Increase(0.25f * iSurvivalSessionSettings.fatigueincreasemultiplier, null);
                    sanity.Increase(0.25f * iSurvivalSessionSettings.sanityincreasemultiplier, null);
                    if (runCount < 300)
                        return;
                    fatigue.Increase(0.01f * iSurvivalSessionSettings.fatigueincreasemultiplier, null);
                    break;
                case MyCharacterMovementEnum.Jump:
                    ProcessJumpingEffect(player, stamina, water);
                    break;
                default:
                    ProcessDefaultMovementEffect(player, stamina, water);
                    break;
            }

            ProcessHealthAndSanityEffects(player, stamina, fatigue, hunger, water, sanity, health);
        }

        // Processes effects for sprinting
        private void ProcessSprintingEffect(IMyPlayer player, MyEntityStat stamina, MyEntityStat water)
        {
            if (stamina.Value > 0)
            {
                playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
                stamina.Decrease((5 * playerinventoryfillfactor + 1) * iSurvivalSessionSettings.staminadrainmultiplier, null);
                water.Decrease((0.5f * playerinventoryfillfactor + 1) * iSurvivalSessionSettings.waterdrainmultiplier, null);
            }
        }

        // Processes effects for jumping
        private void ProcessJumpingEffect(IMyPlayer player, MyEntityStat stamina, MyEntityStat water)
        {
            if (stamina.Value > 0)
            {
                playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
                stamina.Decrease((10 * playerinventoryfillfactor + 1) * iSurvivalSessionSettings.staminadrainmultiplier, null);
                water.Decrease((1 * playerinventoryfillfactor + 1) * iSurvivalSessionSettings.waterdrainmultiplier, null);
            }
        }

        // Processes effects for default movement state
        private void ProcessDefaultMovementEffect(IMyPlayer player, MyEntityStat stamina, MyEntityStat water)
        {
            if (stamina.Value > 0)
            {
                playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
                stamina.Decrease((playerinventoryfillfactor + 0.5f) * iSurvivalSessionSettings.staminadrainmultiplier, null);
                water.Decrease((playerinventoryfillfactor / 2 + 0.1f) * iSurvivalSessionSettings.waterdrainmultiplier, null);
            }
        }

        // Processes effects on health and sanity based on other stats
        private void ProcessHealthAndSanityEffects(IMyPlayer player, MyEntityStat stamina, MyEntityStat fatigue, MyEntityStat hunger, MyEntityStat water, MyEntityStat sanity, MyEntityStat health)
        {
            if (hunger.Value < 1 && fatigue.Value < 1 && water.Value < 1 && sanity.Value < 1)
            {
                health.Decrease(5 * iSurvivalSessionSettings.healthdrainmultiplier, null);
                sanity.Decrease(5 * iSurvivalSessionSettings.sanityincreasemultiplier, null);
            }
            else if (hunger.Value < 1 || fatigue.Value < 1 || water.Value < 1 || sanity.Value < 1)
            {
                health.Decrease(1 * iSurvivalSessionSettings.healthdrainmultiplier, null);
            }
            if (stamina.Value < 10)
            {
                fatigue.Decrease(1 * iSurvivalSessionSettings.fatiguedrainmultiplier, null);
                if (stamina.Value < 1)
                {
                    health.Decrease(5 * iSurvivalSessionSettings.healthdrainmultiplier, null);
                    sanity.Decrease(5 * iSurvivalSessionSettings.sanityincreasemultiplier, null);
                }
            }

            if (hunger.Value > 20 && health.Value < 100)
            {
                health.Increase(1 * iSurvivalSessionSettings.healthincreasemultiplier, null);
                hunger.Decrease(1 * iSurvivalSessionSettings.hungerdrainmultiplier, null);
            }
            if (fatigue.Value > 0 && stamina.Value < 90)
            {
                fatigue.Decrease(((100 - hunger.Value) / 100) * iSurvivalSessionSettings.fatiguedrainmultiplier, null);
                stamina.Increase(2 * ((100 - hunger.Value) / 100) * iSurvivalSessionSettings.staminaincreasemultiplier, null);
            }
            if (runCount < 300)
                return;

            hunger.Decrease((((100 / (100 - (stamina.Value + fatigue.Value))) / 2) / 10 + 0.001f) * -1 * iSurvivalSessionSettings.hungerdrainmultiplier, null);

            if (stamina.Value < 100 && fatigue.Value < 100)
            {
                hunger.Decrease(((100 - fatigue.Value) / 100) * iSurvivalSessionSettings.hungerdrainmultiplier, null);
            }

            if (fatigue.Value > 20 || rand.Next((int)fatigue.Value) > 0)
                return;

            blinkList.Add(player.IdentityId);
            if (player.IdentityId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                blink(true);
            else if (MyAPIGateway.Multiplayer.IsServer)
                MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("blink"), MyVisualScriptLogicProvider.GetSteamId(player.IdentityId), true);
        }

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
