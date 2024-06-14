using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Weapons;
using System.Runtime.ConstrainedExecution;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.Utils;
using VRageMath;
using Digi;


namespace PEPONE.iSurvival
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class iSurvivalSessionSettings : MySessionComponentBase
    {
        public iSurvivalSettings Settings = new iSurvivalSettings();

        // changable varialbles

        public static float staminadrainmultiplier = 1;
        public static float fatiguedrainmultiplier = 1;
        public static float healthdrainmultiplier = 1;
        public static float hungerdrainmultiplier = 1;

        public static float staminaincreasemultiplier = 1;
        public static float fatigueincreasemultiplier = 1;
        public static float healthincreasemultiplier = 1;
        public static float hungerincreasemultiplier = 1;

        public static List<ulong> playerExceptions = new List<ulong>();

        public ChatCommands ChatCommands;

        public override void LoadData()
        {
            Settings.Load();
            staminadrainmultiplier = Settings.staminadrainmultiplier;
            fatiguedrainmultiplier = Settings.fatiguedrainmultiplier;
            healthdrainmultiplier = Settings.healthdrainmultiplier;
            hungerdrainmultiplier = Settings.hungerdrainmultiplier;

            staminaincreasemultiplier = Settings.staminaincreasemultiplier;
            fatigueincreasemultiplier = Settings.fatigueincreasemultiplier;
            healthincreasemultiplier = Settings.healthincreasemultiplier;
            hungerincreasemultiplier = Settings.hungerincreasemultiplier;

            playerExceptions = Settings.playerExceptions.Distinct().ToList();

            ChatCommands = new ChatCommands(this);
        }

        public void UpdateSettings()
        {
            Settings.SaveConfigAfterChatUpdate();
            staminadrainmultiplier = Settings.staminadrainmultiplier;
            fatiguedrainmultiplier = Settings.fatiguedrainmultiplier;
            healthdrainmultiplier = Settings.healthdrainmultiplier;
            hungerdrainmultiplier = Settings.hungerdrainmultiplier;

            staminaincreasemultiplier = Settings.staminaincreasemultiplier;
            fatigueincreasemultiplier = Settings.fatigueincreasemultiplier;
            healthincreasemultiplier = Settings.healthincreasemultiplier;
            hungerincreasemultiplier = Settings.hungerincreasemultiplier;
            playerExceptions = Settings.playerExceptions.Distinct().ToList();
        }

        protected override void UnloadData()
        {
            ChatCommands?.Dispose();
        }

        public class iSurvivalSettings
        {
            const string VariableId = nameof(iSurvivalSettings); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)
            const string FileName = "iSurvivalSettings.ini"; // the file that gets saved to world storage under your mod's folder
            const string IniSection = "Config";

            public float staminadrainmultiplier = 1;
            public float fatiguedrainmultiplier = 1;
            public float healthdrainmultiplier = 1;
            public float hungerdrainmultiplier = 1;

            public float staminaincreasemultiplier = 1;
            public float fatigueincreasemultiplier = 1;
            public float healthincreasemultiplier = 1;
            public float hungerincreasemultiplier = 1;

            public List<ulong> playerExceptions = new List<ulong>();
            public ulong playerRemovedExceptions;

            public iSurvivalSettings()
            {

            }

            public void LoadConfig(MyIni iniParser)
            {
                staminadrainmultiplier = (float)iniParser.Get(IniSection, nameof(staminadrainmultiplier)).ToDouble(staminadrainmultiplier);
                fatiguedrainmultiplier = (float)iniParser.Get(IniSection, nameof(fatiguedrainmultiplier)).ToDouble(fatiguedrainmultiplier);
                healthdrainmultiplier = (float)iniParser.Get(IniSection, nameof(healthdrainmultiplier)).ToDouble(healthdrainmultiplier);
                hungerdrainmultiplier = (float)iniParser.Get(IniSection, nameof(hungerdrainmultiplier)).ToDouble(hungerdrainmultiplier);

                staminaincreasemultiplier = (float)iniParser.Get(IniSection, nameof(staminaincreasemultiplier)).ToDouble(staminaincreasemultiplier);
                fatigueincreasemultiplier = (float)iniParser.Get(IniSection, nameof(fatigueincreasemultiplier)).ToDouble(fatigueincreasemultiplier);
                healthincreasemultiplier = (float)iniParser.Get(IniSection, nameof(healthincreasemultiplier)).ToDouble(healthincreasemultiplier);
                hungerincreasemultiplier = (float)iniParser.Get(IniSection, nameof(hungerincreasemultiplier)).ToDouble(hungerincreasemultiplier);

                //Get the player list from the ini and split it to an array
                var configList = iniParser.Get(IniSection, nameof(playerExceptions)).ToString().Trim().Split('\n');
                foreach ( var config in configList)
                {
                    ulong playerId;
                    if (ulong.TryParse(config, out playerId) && playerId > 0)
                    {
                        playerExceptions.Add(playerId);
                    }
                }
            }

            public void SaveConfig(MyIni iniParser)
            {
                iniParser.Set(IniSection, nameof(staminadrainmultiplier), staminadrainmultiplier);
                iniParser.Set(IniSection, nameof(fatiguedrainmultiplier), fatiguedrainmultiplier);
                iniParser.Set(IniSection, nameof(healthdrainmultiplier), healthdrainmultiplier);
                iniParser.Set(IniSection, nameof(hungerdrainmultiplier), hungerdrainmultiplier);

                iniParser.Set(IniSection, nameof(staminaincreasemultiplier), staminaincreasemultiplier);
                iniParser.Set(IniSection, nameof(fatigueincreasemultiplier), fatigueincreasemultiplier);
                iniParser.Set(IniSection, nameof(healthincreasemultiplier), healthincreasemultiplier);
                iniParser.Set(IniSection, nameof(hungerincreasemultiplier), hungerincreasemultiplier);

                var myarray = playerExceptions.Distinct().ToArray();



                iniParser.Set(IniSection, nameof(playerExceptions), string.Join("\n", myarray));
                iniParser.SetComment(IniSection, nameof(playerExceptions), "Add the IDs of players who should be exempt from the iSurvival mod");

            }

            public void SaveConfigAfterChatUpdate()
            {

                MyIni iniParser = new MyIni();

                iniParser.Set(IniSection, nameof(staminadrainmultiplier), staminadrainmultiplier);
                iniParser.Set(IniSection, nameof(fatiguedrainmultiplier), fatiguedrainmultiplier);
                iniParser.Set(IniSection, nameof(healthdrainmultiplier), healthdrainmultiplier);
                iniParser.Set(IniSection, nameof(hungerdrainmultiplier), hungerdrainmultiplier);

                iniParser.Set(IniSection, nameof(staminaincreasemultiplier), staminaincreasemultiplier);
                iniParser.Set(IniSection, nameof(fatigueincreasemultiplier), fatigueincreasemultiplier);
                iniParser.Set(IniSection, nameof(healthincreasemultiplier), healthincreasemultiplier);
                iniParser.Set(IniSection, nameof(hungerincreasemultiplier), hungerincreasemultiplier);

                var myarray = playerExceptions.Distinct().ToArray();

                //// In case a chat command is given to remove a player
                //if (playerRemovedExceptions != null && playerRemovedExceptions != 0)
                //{
                //    playerExceptions = playerExceptions.RemoveAll(x => (ulong)x == playerRemovedExceptions);
                //    myarray = playerExceptions.Distinct().ToArray();
                //    Log.Info($"playerRemovedExceptions: {playerRemovedExceptions}");
                //    playerRemovedExceptions = 0;
                //}

                iniParser.Set(IniSection, nameof(playerExceptions), string.Join("\n", myarray));
                iniParser.SetComment(IniSection, nameof(playerExceptions), "Add the IDs of players who should be exempt from the iSurvival mod");

                string saveText = iniParser.ToString();
                MyAPIGateway.Utilities.SetVariable<string>(VariableId, saveText);

                using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(iSurvivalSettings)))
                {
                    file.Write(saveText);
                }

            }


            public void Load()
            {
                if (MyAPIGateway.Session.IsServer)
                    LoadOnHost();
                else
                    LoadOnClient();
            }

            public void LoadOnHost()
            {
                MyIni iniParser = new MyIni();

                // load file if exists then save it regardless so that it can be sanitized and updated
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(iSurvivalSettings)))
                {
                    using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(iSurvivalSettings)))
                    {
                        string text = file.ReadToEnd();

                        MyIniParseResult result;
                        if (!iniParser.TryParse(text, out result))
                            throw new Exception($"Config error: {result.ToString()}");

                        LoadConfig(iniParser);
                    }
                }


                iniParser.Clear(); // remove any existing settings that might no longer exist
                SaveConfig(iniParser);

                string saveText = iniParser.ToString();
                MyAPIGateway.Utilities.SetVariable<string>(VariableId, saveText);

                using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(iSurvivalSettings)))
                {
                    file.Write(saveText);
                }
            }

            public void LoadOnClient()
            {
                string text;
                if (!MyAPIGateway.Utilities.GetVariable<string>(VariableId, out text))
                    throw new Exception("No config found in sandbox.sbc!");

                MyIni iniParser = new MyIni();
                MyIniParseResult result;
                if (!iniParser.TryParse(text, out result))
                    throw new Exception($"Config error: {result.ToString()}");

                LoadConfig(iniParser);
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

        //public static float staminadrainmultiplier = iSurvivalSessionSettings.staminadrainmultiplier;
        //public static float fatiguedrainmultiplier = iSurvivalSessionSettings.fatiguedrainmultiplier;
        //public static float healthdrainmultiplier = iSurvivalSessionSettings.healthdrainmultiplier;
        //public static float hungerdrainmultiplier = iSurvivalSessionSettings.hungerdrainmultiplier;

        //public static float staminaincreasemultiplier = iSurvivalSessionSettings.staminaincreasemultiplier;
        //public static float fatigueincreasemultiplier = iSurvivalSessionSettings.fatigueincreasemultiplier;
        //public static float healthincreasemultiplier = iSurvivalSessionSettings.healthincreasemultiplier;
        //public static float hungerincreasemultiplier = iSurvivalSessionSettings.hungerincreasemultiplier;




        //public static float teststamina = 0; //can be removed when we remove debug text
        //public static float testfatigue = 0; //can be removed when we remove debug text


        protected override void UnloadData()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(modId, getPoke);
        }

        public override void UpdateAfterSimulation()
        {

            try
            {
                // Only run every quarter of a second
                if (++runCount % 15 > 0)
                    return;

                // Blinking during load screen causes crash, don't load messagehandler on clients for 30s
                if (!MyAPIGateway.Multiplayer.IsServer && loadWait > 0)
                {
                    if (--loadWait == 0)
                        MyAPIGateway.Multiplayer.RegisterMessageHandler(modId, getPoke);
                    return;
                }

                foreach (var playerId in blinkList)
                    if (playerId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                        blink(false);
                    else
                        MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("unblink"), MyVisualScriptLogicProvider.GetSteamId(playerId), true);
                blinkList.Clear();

                // Check for recharging or using toilet every second
                if (runCount % 60 > 0)
                    return;

                var players = new List<IMyPlayer>();
                MyAPIGateway.Multiplayer.Players.GetPlayers(players, p => p.Character != null && p.Character.ToString().Contains("Astronaut"));

                // Loop through all players
                foreach (IMyPlayer player in players)
                {
                    
                    if (iSurvivalSessionSettings.playerExceptions.FindIndex(x => x == MyAPIGateway.Session.LocalHumanPlayer.SteamUserId) != -1) continue;

                        var statComp = player.Character?.Components.Get<MyEntityStatComponent>();
                    if (statComp == null)
                        continue;

                    MyEntityStat fatigue = GetPlayerStat(statComp, "Fatigue");
                    MyEntityStat hunger = GetPlayerStat(statComp, "Hunger");
                    MyEntityStat stamina = GetPlayerStat(statComp, "Stamina");
                    MyEntityStat health = GetPlayerStat(statComp, "Health");

                    // Skip player if any of the stat's is missing
                    if (fatigue == null || hunger == null || stamina == null)
                        continue;

                    // Checks if player is sitting in a block
                    var block = player.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
                    if (block != null)
                    {
                        var blockDef = block.BlockDefinition.ToString();
                        if (blockDef.Contains("CryoChamber"))
                        {
                            //MyAPIGateway.Utilities.ShowMessage("stamina", "cryo");
                            stamina.Increase(5f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                            fatigue.Increase(5f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                            return;
                        }
                        else if (blockDef.Contains("Toilet") || blockDef.Contains("Bathroom"))
                        {
                            fatigue.Increase(10f, null); // Because the throne is a place for relaxation
                            stamina.Increase(10f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                            if (hunger.Value > 20)
                            {
                                hunger.Decrease(2f * iSurvivalSessionSettings.hungerdrainmultiplier, null); // Drains your hunger stat -> Makes you more hungry
                                player.Character.GetInventory(0).AddItems((MyFixedPoint)0.1f, (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                            }

                        }
                    }

                    // Check for running state every second

                    if (player.Character.CurrentMovementState == MyCharacterMovementEnum.Sitting)
                    {
                        //Not sure we need it. I think regen is managed below so long as hunger isn't too low.
                        stamina.Increase(4f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                        fatigue.Increase(0.5f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                        //MyAPIGateway.Utilities.ShowMessage("stamina", " " + 2.5f * staminaincreasemultiplier);


                    }
                    else if (player.Character.CurrentMovementState == MyCharacterMovementEnum.Standing)
                    {
                        stamina.Increase(1.5f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                        //MyAPIGateway.Utilities.ShowMessage("stamina", "Standing" + (1 * staminaincreasemultiplier));
                    }
                    else if (player.Character.CurrentMovementState == MyCharacterMovementEnum.Sprinting) // @PepperJack let me know what other activities you want to be included
                    {
                        //Drain stamina while running
                        if (stamina.Value > 0)
                        {
                            IMyPlayer thisPlayer = player as IMyPlayer;
                            playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;

                            stamina.Decrease((5 * playerinventoryfillfactor + 1) * iSurvivalSessionSettings.staminadrainmultiplier, null);
                            //MyAPIGateway.Utilities.ShowMessage("stamina", "sprinting" + ((5 * playerinventoryfillfactor + 1) * staminadrainmultiplier));
                        }

                    }
                    else if (player.Character.CurrentMovementState == MyCharacterMovementEnum.Crouching)
                    {
                        stamina.Increase(2.5f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                        //MyAPIGateway.Utilities.ShowMessage("stamina", "Crouching" + (2.5f * staminaincreasemultiplier));
                        fatigue.Increase(0.25f * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                        //MyAPIGateway.Utilities.ShowMessage("stamina", " " + 2.5f * staminaincreasemultiplier);
                        if (runCount < 300)
                            continue;
                        fatigue.Increase(0.01f * iSurvivalSessionSettings.fatigueincreasemultiplier, null);
                        //MyAPIGateway.Utilities.ShowMessage("Rest", " " + 0.01f * fatigueincreasemultiplier);
                        //MyAPIGateway.Utilities.ShowMessage("Fatigue", "Current Value: " + fatigue.Value);

                    }
                    else if (player.Character.CurrentMovementState == MyCharacterMovementEnum.Jump) // @PepperJack let me know what other activities you want to be included
                    {
                        //Drain stamina while running
                        if (stamina.Value > 0)
                        {
                            IMyPlayer thisPlayer = player as IMyPlayer;
                            playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;

                            stamina.Decrease((10 * playerinventoryfillfactor + 1) * iSurvivalSessionSettings.staminadrainmultiplier, null);
                            //MyAPIGateway.Utilities.ShowMessage("stamina", "Jump" + ((10 * playerinventoryfillfactor + 1f) * staminadrainmultiplier));
                        }

                    }
                    else if (stamina.Value > 0)
                    {
                        IMyPlayer thisPlayer = player as IMyPlayer;
                        playerinventoryfillfactor = player.Character.GetInventory().VolumeFillFactor;
                        stamina.Decrease((playerinventoryfillfactor + 0.5f) * iSurvivalSessionSettings.staminadrainmultiplier, null);
                        //MyAPIGateway.Utilities.ShowMessage("stamina", "standard drain" + (playerinventoryfillfactor + 0.5f) * staminadrainmultiplier);
                    }


                    // This is where we remove health if applicable
                    if (hunger.Value < 1 && fatigue.Value < 1) // Both hunger and fatigue are 0
                    {
                        health.Decrease(2 * iSurvivalSessionSettings.healthdrainmultiplier, null);
                    }
                    else if (hunger.Value < 1 || fatigue.Value < 1) // Either hunger or fatigue are 0
                    {
                        health.Decrease(1 * iSurvivalSessionSettings.healthdrainmultiplier, null);
                    }
                    // Damage from stamina level
                    if (stamina.Value < 10)
                    {
                        fatigue.Decrease(1 * iSurvivalSessionSettings.fatiguedrainmultiplier, null);
                        if (stamina.Value < 1)
                        {
                            health.Decrease(5 * iSurvivalSessionSettings.healthdrainmultiplier, null);
                        }
                    }

                    // Heal from food
                    if (hunger.Value > 20 && health.Value < 100)
                    {
                        health.Increase(1 * iSurvivalSessionSettings.healthincreasemultiplier, null);
                        hunger.Decrease(1 * iSurvivalSessionSettings.hungerdrainmultiplier, null);
                        //MyAPIGateway.Utilities.ShowMessage("hunger", "Healing Drain" + (1 * hungerdrainmultiplier));//can be removed when we remove debug text
                    }
                    if (fatigue.Value > 0 && stamina.Value < 90)
                    {
                        fatigue.Decrease(((100 - hunger.Value) / 100) * iSurvivalSessionSettings.fatiguedrainmultiplier, null);
                        stamina.Increase(2 * ((100 - hunger.Value) / 100) * iSurvivalSessionSettings.staminaincreasemultiplier, null);
                        //MyAPIGateway.Utilities.ShowMessage("fatigue", "0 > 0" + ((100 - hunger.Value) / 100));//can be removed when we remove debug text
                        //MyAPIGateway.Utilities.ShowMessage("stamina", "&& < 90" + (2 * ((100 - hunger.Value) / 100)));//can be removed when we remove debug text
                    }
                    // Do remaining checks & stat updates every 5s
                    if (runCount < 300)
                        continue;


                    // Normal hunger loss
                    hunger.Decrease((((100 / (100 - (stamina.Value + fatigue.Value))) / 2) / 10 + 0.001f) * -1 * iSurvivalSessionSettings.hungerdrainmultiplier, null); // Normal hunger drain
                                                                                                                                               //MyAPIGateway.Utilities.ShowMessage("hunger", "normal drain " + (((100 / (100 - (stamina.Value + fatigue.Value))) / 2) / 10 + 0.001f) * -1 * hungerdrainmultiplier);

                    if (stamina.Value < 100 && fatigue.Value < 100)
                    {
                        hunger.Decrease(((100 - fatigue.Value) / 100) * iSurvivalSessionSettings.hungerdrainmultiplier, null);
                        //MyAPIGateway.Utilities.ShowMessage("hunger", "stam and fatigue drain" + ((100 - fatigue.Value) / 100) * hungerdrainmultiplier);//can be removed when we remove debug text

                    }

                    if (fatigue.Value > 20 || rand.Next((int)fatigue.Value) > 0)
                        continue;

                    blinkList.Add(player.IdentityId);
                    if (player.IdentityId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                        blink(true);
                    else if (MyAPIGateway.Multiplayer.IsServer)
                        MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("blink"), MyVisualScriptLogicProvider.GetSteamId(player.IdentityId), true);
                }

                if (runCount > 299)
                    runCount = 0;
            }
            catch (Exception ex)
            {
                Echo("Fatigue exception", ex.ToString());
            }
        }


        #region Background processes
        public void getPoke(byte[] poke)
        {
            // To call blink action on clients
            try
            {
                var msg = ASCIIEncoding.ASCII.GetString(poke);
                blink(msg == "blink");
            }
            catch (Exception ex)
            {
                Echo("Fatigue exception", ex.ToString());
            }
        }

        public void blink(bool blink)
        {
            MyVisualScriptLogicProvider.ScreenColorFadingSetColor(Color.Black, 0L);
            MyVisualScriptLogicProvider.ScreenColorFadingStart(0.25f, blink, 0L);
        }

        private MyEntityStat GetPlayerStat(MyEntityStatComponent statComp, string statName)
        {
            MyEntityStat stat;
            statComp.TryGetStat(MyStringHash.GetOrCompute(statName), out stat);
            return stat;
        }

        public static void Echo(string msg1, string msg2 = "")
        {
            //      MyAPIGateway.Utilities.ShowMessage(msg1, msg2);
            MyLog.Default.WriteLineAndConsole(msg1 + ": " + msg2);
        }

    }

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

        public override string ToString() => string.Format("{0:0}", (float)(CurrentValue * 100.0));
    }

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

        public override string ToString() => string.Format("{0:0}", (float)(CurrentValue * 100.0));
    }
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

        public override string ToString() => string.Format("{0:0}", (float)(CurrentValue * 100.0));
    }
}
#endregion

