﻿using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

// Code heavily borrowed from Bačiulis' awesome Drink Water mod. Thanks so much for pointing me towards these methods dude, very much appreciated.

namespace Fatigue
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Session : MySessionComponentBase
    {

        public static ushort modId = 19008;
        public static int runCount = 0;
        public static Random rand = new Random();
        public static List<long> blinkList = new List<long>();
        public static int loadWait = 120;
        public static float teststamina = 0;
        public static float testfatigue = 0;

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
                    var statComp = player.Character?.Components.Get<MyEntityStatComponent>();
                    if (statComp == null)
                        continue;

                    MyEntityStat fatigue = GetPlayerStat(statComp, "Fatigue");
                    MyEntityStat hunger = GetPlayerStat(statComp, "Hunger");
                    MyEntityStat stamina = GetPlayerStat(statComp, "Stamina");
                    MyEntityStat health = GetPlayerStat(statComp, "Health");
                    MyEntityStat speed = GetPlayerStat(statComp, "speed");

                    // Skip player if any of the stat's is missing
                    if (fatigue == null || hunger == null || stamina == null)
                        continue;

                    // Checks if player is sitting in a block
                    var block = player.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
                    if (block != null)
                    {
                        var blockDef = block.BlockDefinition.ToString();
                        if (blockDef.Contains("CryoChamber"))
                            fatigue.Increase(5f, null);
                        else if (blockDef.Contains("Toilet") || blockDef.Contains("Bathroom"))
                        {
                            fatigue.Increase(2f, null); // Because the throne is a place for relaxation
                            if (hunger.Value > 20)
                                hunger.Decrease(5f, null); // Drains your hunger stat -> Makes you more hungry
                            else
                                // Only gives you organic if you are above 20 hunger
                                player.Character.GetInventory(0).AddItems((MyFixedPoint)0.05f, (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                        }
                    }

                    MyAPIGateway.Utilities.ShowMessage("Test", "" + player.Character.CurrentMovementState + " " + "Stamina: " + teststamina);
                    teststamina = 0;
                    // Check for running state every second
                    if (player.Character.CurrentMovementState == MyCharacterMovementEnum.Sitting)
                    {
                        stamina.Increase(2f, null);
                        teststamina = 2f;
                    }
                    else if (player.Character.CurrentMovementState == MyCharacterMovementEnum.Standing)
                    {
                        stamina.Increase(0.5f, null);
                        teststamina = 0.5f;
                    }
                    else if (player.Character.CurrentMovementState == MyCharacterMovementEnum.Sprinting) // @PepperJack let me know what other activities you want to be included
                    {
                        //Drain stamina while running
                        if (stamina.Value > 0)
                        {
                            stamina.Decrease(5, null);
                        }
                        teststamina = -5;
                    }
                    else if (player.Character.CurrentMovementState == MyCharacterMovementEnum.Crouching)
                    {
                        //Not sure we need it. I think regen is managed below so long as hunger isn't  too low.
                        stamina.Increase(2.5f, null);
                        teststamina = 2.5f;
                    }

                    else if (stamina.Value > 0)
                    {
                        stamina.Decrease(0.1f, null);
                        teststamina = -0.1f;
                    }


                    // This is where we remove health if applicable
                    if (hunger.Value == 0 && fatigue.Value == 0) // Both hunger and fatigue are 0
                    {
                        health.Decrease(2, null);
                        MyAPIGateway.Utilities.ShowMessage("Damage", "hunger and fatigue = 0: health -2");
                        //thisPlayer.DoDamage(1, MyStringHash.GetOrCompute("Hunger AND Fatige, both?!"), false); //Double damage THIS WAS NOT WORKING
                    }
                    else if (hunger.Value == 0 || fatigue.Value == 0) // Either hunger or fatigue are 0
                    {
                        health.Decrease(1, null);
                        MyAPIGateway.Utilities.ShowMessage("Damage", "hunger or fatigue = 0: health -1");
                        //thisPlayer.DoDamage(0.5f, MyStringHash.GetOrCompute("Either Hunger or Fatige, maybe both?!"), false); //Basic damage THIS WAS NOT WORKING
                    }
                    // Damage from stamina level
                    if (stamina.Value < 10)
                    {
                        fatigue.Decrease(1, null);
                        MyAPIGateway.Utilities.ShowMessage("stamina", "stamina < 10: fatigue -1");
                        if (stamina.Value == 0)
                        {
                            health.Decrease(5, null);
                        }
                    }

                    // Heal from food
                    if (hunger.Value > 20 && health.Value < 100)
                    {
                        health.Increase(1, null);
                        hunger.Decrease(1, null);
                        MyAPIGateway.Utilities.ShowMessage("Heal", "heal from hunger: health 1 Hunger -1");
                    }

                    MyAPIGateway.Utilities.ShowMessage("Check", "hunger value: " + hunger.Value);
                    // Do remaining checks & stat updates every 5s
                    if (runCount < 300)
                        continue;

                    hunger.Decrease(((100 - fatigue.Value) / 50), null);
                    MyAPIGateway.Utilities.ShowMessage("Check", "Normal hunger loss " + ((100 - fatigue.Value) / 50));
                    MyAPIGateway.Utilities.ShowMessage("Check", "hunger value: " + hunger.Value);

                    if (stamina.Value < 100)
                    {
                        fatigue.Decrease((100 - hunger.Value) / 50, null);
                        MyAPIGateway.Utilities.ShowMessage("Check", "stamina < 100: : fatigue: " + ((100 - hunger.Value) / -50));
                        hunger.Decrease((100 - fatigue.Value)/50, null);
                        MyAPIGateway.Utilities.ShowMessage("Check", "stamina < 100: Hunger: " + (100 - fatigue.Value) / -50);

                    }

                    //if (fatigue.Value < 30)
                    //{
                    //    stamina.Decrease((float)fatigue.Value / 100, null);
                    //    MyAPIGateway.Utilities.ShowMessage("Check", "fatigue < 30: Stamina " + (float)fatigue.Value / -100);
                    //}
                    //else if (fatigue.Value > 50)
                    //{
                    //    stamina.Increase((float)((fatigue.Value) / 20), null);
                    //    MyAPIGateway.Utilities.ShowMessage("Check", "fatigue > 50: Stamina " + (fatigue.Value) / 20);
                    //}

                    //if (hunger.Value < 30)
                    //    fatigue.Decrease((float)Math.Min(fatigue.Value / 100, (50 - hunger.Value) / 20), null);

                    //if (hunger.Value > 50)
                    //   fatigue.Increase((float)Math.Min(fatigue.Value / 100, (hunger.Value - 50) / 20), null);

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
