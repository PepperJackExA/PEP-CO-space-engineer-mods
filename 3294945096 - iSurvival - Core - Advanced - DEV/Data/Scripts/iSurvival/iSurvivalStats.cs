using Sandbox.ModAPI;
using VRage.Utils;
using System.Collections.Generic;
using VRage;
using System;
using VRage.ModAPI;
using PEPCO.iSurvival.settings;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;

namespace PEPCO.iSurvival.stats
{
    // Define the StatSetting class if not already defined
    public class StatSetting
    {
        public float Base { get; set; }
        public float Multiplier { get; set; }
        public float Value { get; set; }

        public StatSetting(float baseValue, float multiplier)
        {
            Base = baseValue;
            Multiplier = multiplier;
            Value = Base * Multiplier;
        }

        public void UpdateValue()
        {
            Value = Base * Multiplier;
        }
    }

    // Static class to store stat settings
    public static class StatManager
    {
        public static Dictionary<string, StatSetting> _statSettings = new Dictionary<string, StatSetting>(StringComparer.OrdinalIgnoreCase)
    {
        { "Sanity", new StatSetting(1, 1) },
        { "Calories", new StatSetting(1, 1) },
        { "Fat", new StatSetting(1, 1) },
        { "Cholesterol", new StatSetting(1, 1) },
        { "Sodium", new StatSetting(1, 1) },
        { "Carbohydrates", new StatSetting(1, 1) },
        { "Protein", new StatSetting(1, 1) },
        { "Vitamins", new StatSetting(1, 1) },
        { "Hunger", new StatSetting(1, 1) },
        { "Water", new StatSetting(1, 1) },
        { "Fatigue", new StatSetting(1, 1) },
        { "Stamina", new StatSetting(1, 1) }
    };
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

    // HUD Stat for Player Calories
    public class MyStatPlayerCalories : IMyHudStat
    {
        public MyStatPlayerCalories()
        {
            Id = MyStringHash.GetOrCompute("player_calories");
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
            MyEntityStat Calories;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Calories"), out Calories))
                CurrentValue = Calories.Value / Calories.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Fat
    public class MyStatPlayerFat : IMyHudStat
    {
        public MyStatPlayerFat()
        {
            Id = MyStringHash.GetOrCompute("player_fat");
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
            MyEntityStat Fat;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Fat"), out Fat))
                CurrentValue = Fat.Value / Fat.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Cholesterol
    public class MyStatPlayerCholesterol : IMyHudStat
    {
        public MyStatPlayerCholesterol()
        {
            Id = MyStringHash.GetOrCompute("player_cholesterol");
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
            MyEntityStat Cholesterol;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Cholesterol"), out Cholesterol))
                CurrentValue = Cholesterol.Value / Cholesterol.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Sodium
    public class MyStatPlayerSodium : IMyHudStat
    {
        public MyStatPlayerSodium()
        {
            Id = MyStringHash.GetOrCompute("player_sodium");
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
            MyEntityStat Sodium;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Sodium"), out Sodium))
                CurrentValue = Sodium.Value / Sodium.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Carbohydrates
    public class MyStatPlayerCarbohydrates : IMyHudStat
    {
        public MyStatPlayerCarbohydrates()
        {
            Id = MyStringHash.GetOrCompute("player_carbohydrates");
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
            MyEntityStat Carbohydrates;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Carbohydrates"), out Carbohydrates))
                CurrentValue = Carbohydrates.Value / Carbohydrates.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Protein
    public class MyStatPlayerProtein : IMyHudStat
    {
        public MyStatPlayerProtein()
        {
            Id = MyStringHash.GetOrCompute("player_protein");
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
            MyEntityStat Protein;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Protein"), out Protein))
                CurrentValue = Protein.Value / Protein.MaxValue;
        }

        public override string ToString() => $"{CurrentValue * 100.0:0}";
    }

    // HUD Stat for Player Vitamins
    public class MyStatPlayerVitamins : IMyHudStat
    {
        public MyStatPlayerVitamins()
        {
            Id = MyStringHash.GetOrCompute("player_vitamins");
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
            MyEntityStat Vitamins;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute("Vitamins"), out Vitamins))
                CurrentValue = Vitamins.Value / Vitamins.MaxValue;
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
}
