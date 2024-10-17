using Sandbox.ModAPI;
using VRage.Utils;
using System.Collections.Generic;
using VRage;
using System;
using VRage.ModAPI;
using PEPCO.iSurvival.settings;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using PEPCO.iSurvival.Log;
using VRage.Game.ModAPI;

namespace PEPCO.iSurvival.stats
{
    // Define the StatSetting class if not already defined
    public class StatSetting
    {
        public float Base { get; set; }
        public float Multiplier { get; set; }
        public float IncreaseMultiplier { get; set; }
        public float DecreaseMultiplier { get; set; }

        public StatSetting(float baseValue, float multiplier, float increaseMultiplier, float decreaseMultiplier)
        {
            Base = baseValue;
            Multiplier = multiplier;
            IncreaseMultiplier = increaseMultiplier;
            DecreaseMultiplier = decreaseMultiplier;
        }

        public void UpdateValue()
        {
            // Logic to update the stat value, if necessary
            Base = Base * Multiplier;
        }
    }

    // Static class to store stat settings
    public static class StatManager
    {
        public static Dictionary<string, StatSetting> _statSettings = new Dictionary<string, StatSetting>(StringComparer.OrdinalIgnoreCase)
        {
            { "Sanity", new StatSetting(0f, 1f, 1f, 1f) },
            { "Calories", new StatSetting(0f, 1f, 1f, 1f) },
            { "Fat", new StatSetting(0f, 1f, 1f, 1f) },
            { "Cholesterol", new StatSetting(0f, 1f, 1f, 1f) },
            { "Sodium", new StatSetting(0f, 1f, 1f, 1f) },
            { "Carbohydrates", new StatSetting(0f, 1f, 1f, 1f) },
            { "Sugar", new StatSetting(0f, 1f, 1f, 1f) },
            { "Protein", new StatSetting(0f, 1f, 1f, 1f) },
            { "Vitamins", new StatSetting(0f, 1f, 1f, 1f) },
            { "Hunger", new StatSetting(0f, 1f, 1f, 1f) },
            { "Water", new StatSetting(0f, 1f, 1f, 1f) },
            { "Fatigue", new StatSetting(0f, 1f, 1f, 1f) },
            { "Stamina", new StatSetting(0f, 1f, 1f, 1f) },
            { "Strength", new StatSetting(0f, 1f, 1f, 1f) },
            { "Dexterity", new StatSetting(0f, 1f, 1f, 1f) },
            { "Constitution", new StatSetting(0f, 1f, 1f, 1f) },
            { "Intelligence", new StatSetting(0f, 1f, 1f, 1f) },
            { "Wisdom", new StatSetting(0f, 1f, 1f, 1f) },
            { "Charisma", new StatSetting(0f, 1f, 1f, 1f) }
        };
    }

    // Helper class to fetch all player stats
    public static class PlayerStatsHelper
    {
        public static void GetAllStats(IMyPlayer player)
        {
            // Get the stat component from the player character.
            var statComp = player?.Character?.Components?.Get<MyEntityStatComponent>();

            if (statComp == null)
            {
                MyAPIGateway.Utilities.ShowMessage("Error", "No stat component found for player.");
                return;
            }

            // Retrieve each stat from the component
            foreach (var statName in StatManager._statSettings.Keys)
            {
                MyEntityStat stat;
                if (statComp.TryGetStat(MyStringHash.GetOrCompute(statName), out stat))
                {
                    //MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName}: {stat?.Value}");
                }
            }
        }
    }

    // Base class for HUD stats
    public abstract class BaseHudStat : IMyHudStat
    {
        protected float m_currentValue;
        protected string m_valueStringCache;
        public MyStringHash Id { get; protected set; }

        public virtual float MaxValue => 1f;
        public virtual float MinValue => 0.0f;

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

        public string GetValueString()
        {
            if (m_valueStringCache == null)
                m_valueStringCache = ToString();
            return m_valueStringCache;
        }

        public abstract string StatSubtype { get; }

        public void Update()
        {
            MyEntityStatComponent statComp = MyAPIGateway.Session.Player?.Character?.Components.Get<MyEntityStatComponent>();
            if (statComp == null)
                return;

            MyEntityStat stat;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute(StatSubtype), out stat) && stat != null)
            {
                if (stat.MaxValue == 0)
                {
                    CurrentValue = 0; // Avoid division by zero
                }
                else
                {
                    CurrentValue = stat.Value / stat.MaxValue;
                }
            }

        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // Specific HUD stat classes
    public class MyStatPlayerSanity : BaseHudStat
    {
        public override string StatSubtype => "Sanity";
        public MyStatPlayerSanity()
        {
            Id = MyStringHash.GetOrCompute("player_sanity");
        }
    }

    public class MyStatPlayerCalories : BaseHudStat
    {
        public override string StatSubtype => "Calories";
        public MyStatPlayerCalories()
        {
            Id = MyStringHash.GetOrCompute("player_calories");
        }
    }

    public class MyStatPlayerFat : BaseHudStat
    {
        public override string StatSubtype => "Fat";
        public MyStatPlayerFat()
        {
            Id = MyStringHash.GetOrCompute("player_fat");
        }
    }

    public class MyStatPlayerCholesterol : BaseHudStat
    {
        public override string StatSubtype => "Cholesterol";
        public MyStatPlayerCholesterol()
        {
            Id = MyStringHash.GetOrCompute("player_cholesterol");
        }
    }

    public class MyStatPlayerSodium : BaseHudStat
    {
        public override string StatSubtype => "Sodium";
        public MyStatPlayerSodium()
        {
            Id = MyStringHash.GetOrCompute("player_sodium");
        }
    }

    public class MyStatPlayerCarbohydrates : BaseHudStat
    {
        public override string StatSubtype => "Carbohydrates";
        public MyStatPlayerCarbohydrates()
        {
            Id = MyStringHash.GetOrCompute("player_carbohydrates");
        }
    }
    public class MyStatPlayerSugar : BaseHudStat
    {
        public override string StatSubtype => "Sugar";
        public MyStatPlayerSugar()
        {
            Id = MyStringHash.GetOrCompute("player_sugar");
        }
    }

    public class MyStatPlayerProtein : BaseHudStat
    {
        public override string StatSubtype => "Protein";
        public MyStatPlayerProtein()
        {
            Id = MyStringHash.GetOrCompute("player_protein");
        }
    }

    public class MyStatPlayerVitamins : BaseHudStat
    {
        public override string StatSubtype => "Vitamins";
        public MyStatPlayerVitamins()
        {
            Id = MyStringHash.GetOrCompute("player_vitamins");
        }
    }

    public class MyStatPlayerHunger : BaseHudStat
    {
        public override string StatSubtype => "Hunger";
        public MyStatPlayerHunger()
        {
            Id = MyStringHash.GetOrCompute("player_hunger");
        }
    }

    public class MyStatPlayerWater : BaseHudStat
    {
        public override string StatSubtype => "Water";
        public MyStatPlayerWater()
        {
            Id = MyStringHash.GetOrCompute("player_water");
        }
    }

    public class MyStatPlayerFatigue : BaseHudStat
    {
        public override string StatSubtype => "Fatigue";
        public MyStatPlayerFatigue()
        {
            Id = MyStringHash.GetOrCompute("player_fatigue");
        }
    }

    public class MyStatPlayerStamina : BaseHudStat
    {
        public override string StatSubtype => "Stamina";
        public MyStatPlayerStamina()
        {
            Id = MyStringHash.GetOrCompute("player_stamina");
        }
    }

    public class MyStatPlayerStrength : BaseHudStat
    {
        public override string StatSubtype => "Strength";
        public MyStatPlayerStrength()
        {
            Id = MyStringHash.GetOrCompute("player_strength");
        }
    }

    public class MyStatPlayerDexterity : BaseHudStat
    {
        public override string StatSubtype => "Dexterity";
        public MyStatPlayerDexterity()
        {
            Id = MyStringHash.GetOrCompute("player_dexterity");
        }
    }

    public class MyStatPlayerConstitution : BaseHudStat
    {
        public override string StatSubtype => "Constitution";
        public MyStatPlayerConstitution()
        {
            Id = MyStringHash.GetOrCompute("player_constitution");
        }
    }

    public class MyStatPlayerIntelligence : BaseHudStat
    {
        public override string StatSubtype => "Intelligence";
        public MyStatPlayerIntelligence()
        {
            Id = MyStringHash.GetOrCompute("player_intelligence");
        }
    }

    public class MyStatPlayerWisdom : BaseHudStat
    {
        public override string StatSubtype => "Wisdom";
        public MyStatPlayerWisdom()
        {
            Id = MyStringHash.GetOrCompute("player_wisdom");
        }
    }

    public class MyStatPlayerCharisma : BaseHudStat
    {
        public override string StatSubtype => "Charisma";
        public MyStatPlayerCharisma()
        {
            Id = MyStringHash.GetOrCompute("player_charisma");
        }
    }
}
