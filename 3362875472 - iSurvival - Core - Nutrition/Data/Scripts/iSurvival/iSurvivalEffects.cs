using PEPCO.iSurvival.factors;
using PEPCO.iSurvival.Log;
using PEPCO.iSurvival.stats;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

using Sandbox.Game;
using PEPCO.iSurvival.settings;
using VRage.ObjectBuilders;
using VRage;
using Sandbox.Game.World;
using static PEPCO.iSurvival.Effects.Processes;

namespace PEPCO.iSurvival.Effects
{
    public static class Processes
    {
        private static Dictionary<long, Dictionary<string, int>> playerStatLastValues = new Dictionary<long, Dictionary<string, int>>();
        public static void ProcessPlayer(IMyPlayer player, MyEntityStatComponent statComp)
        {
            if (!playerStatLastValues.ContainsKey(player.IdentityId))
            {
                playerStatLastValues[player.IdentityId] = new Dictionary<string, int>();
            }
            var playerStatValues = playerStatLastValues[player.IdentityId];


            // Retrieve all relevant stats at once
            var stats = StatManager._statSettings.Keys.ToDictionary(
                statName => statName,
                statName =>
                {
                    MyEntityStat stat;
                    statComp.TryGetStat(MyStringHash.GetOrCompute(statName), out stat);
                    return stat;
                }
            );



            if (Blocks.ProcessBlockstEffects(player, stats))
                return; // Stop processing if the player is in a cryo block

            var fatigue = stats.GetValueOrDefault("Fatigue");
            var hunger = stats.GetValueOrDefault("Hunger");
            var sanity = stats.GetValueOrDefault("Sanity");

            // Initialize or retrieve the player's RPG leveling system
            long playerId = player.IdentityId;
            RPGLeveling rpgLeveling;
            if (!Core.iSurvivalSession.playerRPGSystems.TryGetValue(playerId, out rpgLeveling))
            {
                // Filter stats to only include non-null values and initialize the RPG leveling system
                rpgLeveling = new RPGLeveling(
                    stats.Where(statEntry => statEntry.Value != null)
                         .ToDictionary(kv => kv.Key, kv => kv.Value)
                );
                Core.iSurvivalSession.playerRPGSystems[playerId] = rpgLeveling;
            }


            // Process Fatigue
            ProcessStatEffect(player, "Fatigue", fatigue, playerStatValues,
                chanceCalculation: val => (val.MaxValue / Math.Max(val.Value, 1)) / 100.0,
                criticalThreshold: 10,
                warningThreshold: 30,
                criticalMessage: "You are severely fatigued. Rest to recover.",
                warningMessage: "You are tired. Consider resting soon.",
                warningCooldown: ref Core.iSurvivalSession.insanityMessageCooldown
            );

            // Process Sanity
            ProcessStatEffect(player, "Sanity", sanity, playerStatValues,
                chanceCalculation: val => (val.MaxValue / Math.Max(val.Value, 1)) / 200.0,
                criticalThreshold: 5,
                warningThreshold: 20,
                criticalMessage: "You are going insane!",
                warningMessage: "Your sanity is critically low! Seek comfort or rest.",
                warningCooldown: ref Core.iSurvivalSession.insanityMessageCooldown
            );

            // Process Hunger
            ProcessHunger(player, hunger, playerStatValues);

            // Apply other processes
            Metabolism.ApplyMetabolismEffect(player, stats);
            FatigueAndStamina.ProcessFatigue(player, stats);
            Sanity.ProcessSanity(player, stats);
            Movement.ProcessMovementEffects(player, stats);
        }
        private static void ProcessStatEffect(IMyPlayer player, string statName, MyEntityStat stat, Dictionary<string, int> playerStatValues,
            Func<MyEntityStat, double> chanceCalculation, int criticalThreshold, int warningThreshold, string criticalMessage, string warningMessage, ref int warningCooldown)
        {
            if (stat == null) return;

            double chance = chanceCalculation(stat);
            if (stat.Value < criticalThreshold && Core.iSurvivalSession.rand.NextDouble() < chance)
            {
                Core.iSurvivalSession.blinkList.Add(player.IdentityId);
                if (player.IdentityId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                    Effects.Processes.Blink.blink(true);
                else if (MyAPIGateway.Multiplayer.IsServer)
                    MyAPIGateway.Multiplayer.SendMessageTo(Core.iSurvivalSession.modId, Encoding.ASCII.GetBytes("blink"), MyVisualScriptLogicProvider.GetSteamId(player.IdentityId), true);
            }

            int currentValue = (int)Math.Floor(stat.Value);

            // Check if the current value has changed or decreased from the last recorded value
            int lastValue;
            if (!playerStatValues.TryGetValue(statName, out lastValue) || lastValue > currentValue)

            {
                // Update the last recorded value for this stat
                playerStatValues[statName] = currentValue;

                // Check for critical thresholds and display appropriate notifications
                if (currentValue < criticalThreshold)
                {
                    // Trigger a critical notification if the cooldown has elapsed
                    if (++warningCooldown >= 120)
                    {
                        MyAPIGateway.Utilities.ShowNotification(criticalMessage, 2000, MyFontEnum.Red);
                        warningCooldown = 0; // Reset the cooldown
                    }
                }
                else if (currentValue < warningThreshold)
                {
                    // Display a warning notification
                    MyAPIGateway.Utilities.ShowNotification(warningMessage, 10000, MyFontEnum.White);
                }
            }

        }
        private static void ProcessHunger(IMyPlayer player, MyEntityStat hunger, Dictionary<string, int> playerStatValues)
        {
            // Exit early if the hunger stat is null
            if (hunger == null) return;

            // Calculate the current hunger value as an integer
            int currentValue = (int)Math.Floor(hunger.Value);

            // Check if the hunger value has decreased or if it's being tracked for the first time
            int lastValue;
            if (!playerStatValues.TryGetValue("Hunger", out lastValue) || lastValue > currentValue)

            {
                // Update the tracked hunger value
                playerStatValues["Hunger"] = currentValue;

                // Handle starvation notification
                if (currentValue < 5)
                {
                    // Increment the starvation cooldown and show a notification if the cooldown threshold is reached
                    if (++Core.iSurvivalSession.starvationMessageCooldown >= 120)
                    {
                        MyAPIGateway.Utilities.ShowNotification("You are starving!", 10000, MyFontEnum.Red);
                        Core.iSurvivalSession.starvationMessageCooldown = 0; // Reset the cooldown
                    }
                }
                // Handle hunger warning notification
                else if (currentValue < 20)
                {
                    MyAPIGateway.Utilities.ShowNotification("You are hungry. Should think of eating soon.", 10000, MyFontEnum.White);
                }
            }
        }
        public static class Blocks
        {
            public static bool ProcessBlockstEffects(IMyPlayer player, Dictionary<string, MyEntityStat> stats, string blockName = null)
            {
                var block = player.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
                if (block == null) return false; // Player not in a block

                // Safely retrieve required stats with default values for null cases
                var requiredStats = new[] { "Fatigue", "Sanity", "Hunger", "Calories", "Water", "Stamina", "Strength", "Dexterity", "Constitution" };
                var statValues = requiredStats.ToDictionary(
                    stat => stat,
                    stat => stats.ContainsKey(stat) && stats[stat] != null ? stats[stat].Value : 0f // Handle null stats gracefully
                );

                // Average stats calculation (ensure valid stats)
                float averageStats = (statValues["Fatigue"] + statValues["Sanity"] + statValues["Hunger"] + statValues["Water"]) / 4;

                // Get block details
                string blockDef = block.BlockDefinition.SubtypeId.ToString();
                string blockDisplayName = block.DisplayNameText;

                // Check if blockName matches the display name (if provided)
                if (!string.IsNullOrEmpty(blockName) && blockDisplayName != blockName)
                {
                    return false;
                }

                // Cryo block behavior
                if (blockDef.Contains("Cryo"))
                {
                    return true; // Player is in a cryo block, stop processing
                }

                // Bed behavior
                if (blockDef.Contains("Bed"))
                {
                    ApplyChangesForBed(player, stats);
                    return true;
                }

                // Toilet or Bathroom behavior
                if (blockDef.Contains("Toilet") || blockDef.Contains("Bathroom"))
                {
                    ApplyChangesForToilet(player, stats, statValues, averageStats);
                }

                // General behavior
                ApplyGeneralStatChanges(player, stats, statValues);

                return false;
            }


            private static void ApplyChangesForBed(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                Core.iSurvivalSession.ApplyStatChange(player, stats["Stamina"], 1, 2);
                Core.iSurvivalSession.ApplyStatChange(player, stats["Fatigue"], 1, 2);
                Core.iSurvivalSession.ApplyStatChange(player, stats["Sanity"], 1, 1); // Beds provide a moderate sanity boost
            }

            private static void ApplyChangesForToilet(IMyPlayer player, Dictionary<string, MyEntityStat> stats, Dictionary<string, float> statValues, float averageStats)
            {
                Core.iSurvivalSession.ApplyStatChange(player, stats["Fatigue"], 1, 1);
                Core.iSurvivalSession.ApplyStatChange(player, stats["Stamina"], 1, 2);

                if (statValues["Hunger"] > 20 && statValues["Calories"] > 1)
                {
                    Core.iSurvivalSession.ApplyStatChange(player, stats["Sanity"], 1, 0.5);
                    Core.iSurvivalSession.ApplyStatChange(player, stats["Calories"], 1, -10);
                    player.Character.GetInventory(0).AddItems(
                        (MyFixedPoint)((100 - averageStats) / 100),
                        (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic"))
                    );
                }
            }

            private static void ApplyGeneralStatChanges(IMyPlayer player, Dictionary<string, MyEntityStat> stats, Dictionary<string, float> statValues)
            {
                if (statValues["Fatigue"] < 25)
                    Core.iSurvivalSession.ApplyStatChange(player, stats["Fatigue"], 1, 0.25);

                if (statValues["Stamina"] < 100)
                    Core.iSurvivalSession.ApplyStatChange(player, stats["Stamina"], 1, 2);

                if (statValues["Strength"] > 10)
                    Core.iSurvivalSession.ApplyStatChange(player, stats["Strength"], 1, -0.001f);

                if (statValues["Dexterity"] > 10)
                    Core.iSurvivalSession.ApplyStatChange(player, stats["Dexterity"], 1, -0.001f);

                if (statValues["Constitution"] > 10)
                    Core.iSurvivalSession.ApplyStatChange(player, stats["Constitution"], 1, -0.001f);
            }
        }
        public static class Blink
        {
            public static void getPoke(byte[] poke)
            {
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

            public static void blink(bool blink)
            {
                MyVisualScriptLogicProvider.ScreenColorFadingSetColor(Color.Black, 0L);
                MyVisualScriptLogicProvider.ScreenColorFadingStart(0.25f, blink, 0L);
            }

            public static void Echo(string msg1, string msg2 = "")
            {
                MyLog.Default.WriteLineAndConsole(msg1 + ": " + msg2);
            }
        }
        public static class Sanity
        {
            public static void ProcessSanity(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var sanity = stats.ContainsKey("Sanity") ? stats["Sanity"] : null;
                if (sanity == null) return;

                float sanityChangeRate = CalculateSanityChangeRate(player, stats);

                // Stabilize sanity toward 50% of max
                sanityChangeRate += StabilizeStat(sanity, stats);

                Core.iSurvivalSession.ApplyStatChange(player, sanity, 1, sanityChangeRate / 60f);
            }

            private static float CalculateSanityChangeRate(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                float sanityChangeRate = 0f;

                var fatigue = stats.ContainsKey("Fatigue") ? stats["Fatigue"] : null;
                var wisdom = stats.ContainsKey("Wisdom") ? stats["Wisdom"] : null;
                var charisma = stats.ContainsKey("Charisma") ? stats["Charisma"] : null;
                var hunger = stats.ContainsKey("Hunger") ? stats["Hunger"] : null;
                var environmentFactor = EnvironmentalFactors.GetEnvironmentalFactor(player);
                float oxygenFactor = EnvironmentalFactors.OxygenLevelEnvironmentalFactor(player);
                oxygenFactor = MathHelper.Clamp(oxygenFactor, 0.1f, 2f);

                // Apply RPG-based influences on sanity
                sanityChangeRate += GetWisdomImpactOnSanity(wisdom);
                sanityChangeRate += GetCharismaImpactOnSanity(charisma);

                // Environmental and oxygen influence
                sanityChangeRate += oxygenFactor;     // Higher oxygen improves sanity
                sanityChangeRate -= environmentFactor; // Harsh environment reduces sanity

                // Fatigue’s impact on sanity
                sanityChangeRate += GetFatigueImpactOnSanity(fatigue);

                // Hunger's impact on sanity
                if (hunger != null && hunger.Value < 20)
                {
                    sanityChangeRate -= 2f; // Starvation significantly reduces sanity
                }

                return sanityChangeRate;
            }

            private static float StabilizeStat(MyEntityStat stat, Dictionary<string, MyEntityStat> stats)
            {
                float target = stat.MaxValue * 0.5f; // Target 50% of max
                float currentValue = stat.Value;

                // If already near target, minimal adjustment
                if (Math.Abs(currentValue - target) < 0.01f) return 0f;

                float changeRate = 0f;

                if (currentValue < target)
                {
                    // Below target: nutrients are consumed to restore
                    changeRate += ConsumeNutrientsForStat(stats, stat);
                }
                else
                {
                    // Above target: slow decay toward equilibrium
                    changeRate -= 0.1f;
                }

                return changeRate;
            }

            private static float ConsumeNutrientsForStat(Dictionary<string, MyEntityStat> stats, MyEntityStat stat)
            {
                float nutrientImpact = 0f;

                // Use Sugar
                if (stats.ContainsKey("Sugar"))
                {
                    var sugar = stats["Sugar"];
                    if (sugar.Value > 0)
                    {
                        float sugarConsumed = Math.Min(0.05f, sugar.Value); // Consume up to 0.05 per tick
                        sugar.Decrease(sugarConsumed, null);
                        nutrientImpact += sugarConsumed * 10f; // High short-term impact
                    }
                }

                // Use Vitamins
                if (stats.ContainsKey("Vitamins"))
                {
                    var vitamins = stats["Vitamins"];
                    if (vitamins.Value > 0)
                    {
                        float vitaminsConsumed = Math.Min(0.02f, vitamins.Value); // Consume up to 0.02 per tick
                        vitamins.Decrease(vitaminsConsumed, null);
                        nutrientImpact += vitaminsConsumed * 15f; // Long-term stabilization
                    }
                }

                // Use Starches
                if (stats.ContainsKey("Starches"))
                {
                    var starches = stats["Starches"];
                    if (starches.Value > 0)
                    {
                        float starchesConsumed = Math.Min(0.03f, starches.Value); // Consume up to 0.03 per tick
                        starches.Decrease(starchesConsumed, null);
                        nutrientImpact += starchesConsumed * 8f; // Medium-term impact
                    }
                }

                return nutrientImpact;
            }

            private static float GetWisdomImpactOnSanity(MyEntityStat wisdom)
            {
                if (wisdom == null) return 0f;
                float wisdomLevel = wisdom.Value / wisdom.MaxValue;
                return wisdomLevel > 0.5f ? wisdomLevel * 1.2f : wisdomLevel * 0.5f;
            }

            private static float GetCharismaImpactOnSanity(MyEntityStat charisma)
            {
                if (charisma == null) return 0f;
                float charismaLevel = charisma.Value / charisma.MaxValue;
                return charismaLevel > 0.5f ? charismaLevel * 0.8f : charismaLevel * 0.4f;
            }

            private static float GetFatigueImpactOnSanity(MyEntityStat fatigue)
            {
                if (fatigue == null) return 0f;

                float fatigueLevel = fatigue.Value / fatigue.MaxValue;
                if (fatigueLevel > 0.8f)
                {
                    return 1.5f; // Very low fatigue, sanity stays high
                }
                else if (fatigueLevel > 0.5f)
                {
                    return 0.5f; // Moderate fatigue, small impact
                }
                else if (fatigueLevel > 0.2f)
                {
                    return -1.0f; // Higher impact as fatigue worsens
                }
                else
                {
                    return -2.5f; // Critical fatigue, large sanity penalty
                }
            }
        }
        public static class FatigueAndStamina
        {
            public static void ProcessFatigue(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                if (stats == null || player?.Character == null) return;

                var fatigue = stats.GetValueOrDefault("Fatigue");
                var stamina = stats.GetValueOrDefault("Stamina");
                if (fatigue == null || stamina == null) return;

                // Calculate dynamic fatigue impact based on current stamina usage and environment
                float dynamicFatigueImpact = CalculateDynamicFatigueImpact(player, stats, stamina);

                // Update fatigue directly
                MyAPIGateway.Utilities.ShowMessage("Fatigue", $"{dynamicFatigueImpact/10}");
                Core.iSurvivalSession.ApplyStatChange(player, fatigue, 1, dynamicFatigueImpact / 10f);

                // Process stamina changes
                ProcessStamina(player, stats, fatigue);
            }

            private static float CalculateDynamicFatigueImpact(IMyPlayer player, Dictionary<string, MyEntityStat> stats, MyEntityStat stamina)
            {
                float fatigueChangeRate = 0f;

                // Stamina usage contributes directly to fatigue
                float staminaUsageRate = (stamina.MaxValue - stamina.Value) / stamina.MaxValue; // Normalize stamina usage (0-1)
                fatigueChangeRate += staminaUsageRate * 1f; // Scale fatigue increase based on stamina usage
                MyAPIGateway.Utilities.ShowMessage("Fatigue Weather", $"{fatigueChangeRate} * {EnvironmentalFactors.GetEnvironmentalFactor(player)}");

                // Environmental factors dynamically affect fatigue
                fatigueChangeRate *= EnvironmentalFactors.GetEnvironmentalFactor(player);
                MyAPIGateway.Utilities.ShowMessage("Fatigue Post Weather", $"{EnvironmentalFactors.GetEnvironmentalFactor(player)}");


                // Sanity dynamically impacts fatigue
                var sanity = stats.GetValueOrDefault("Sanity");
                if (sanity != null && sanity.MaxValue > 0)
                {
                    float sanityMultiplier = GetSanityEfficiency(sanity);
                    fatigueChangeRate *= sanityMultiplier;
                    MyAPIGateway.Utilities.ShowMessage("Fatigue Sanity Factor", $"{fatigueChangeRate} (Sanity Multiplier: {sanityMultiplier})");
                }

                // Nutrition dynamically influences fatigue recovery
                //foreach (var stat in stats)
                //{
                //    fatigueChangeRate += CalculateNutritionImpactOnFatigue(stat.Value);
                //}

                return -fatigueChangeRate;
            }
            private static float GetSanityEfficiency(MyEntityStat sanity)
            {
                if (sanity == null || sanity.MaxValue <= 0)
                    return 1.0f; // Default multiplier if sanity stat is unavailable

                // Normalize sanity value (0 to 1 scale)
                float normalizedSanity = sanity.Value / sanity.MaxValue;

                // Dynamic multiplier based on sanity
                if (normalizedSanity >= 0.5f)
                {
                    // Higher sanity decreases multiplier (less fatigue impact)
                    return MathHelper.Clamp(1.0f - (normalizedSanity - 0.5f) * 0.5f, 0.75f, 1.0f);
                }
                else
                {
                    // Lower sanity increases multiplier (greater fatigue impact)
                    return MathHelper.Clamp(1.0f + (0.5f - normalizedSanity) * 0.5f, 1.0f, 1.5f);
                }
            }

            private static float CalculateNutritionImpactOnFatigue(MyEntityStat stat)
            {
                if (stat == null || stat.MaxValue == 0) return 0f;

                // Higher values of the stat reduce fatigue dynamically
                float normalizedStat = stat.Value / stat.MaxValue; // Normalize between 0-1
                return (1.0f - normalizedStat) * 0.1f; // Scale fatigue reduction inversely with stat value
            }

            public static void ProcessStamina(IMyPlayer player, Dictionary<string, MyEntityStat> stats, MyEntityStat fatigue)
            {
                var stamina = stats.GetValueOrDefault("Stamina");
                if (stamina == null) return;

                float staminaChangeRate = 0f;

                // RPG stats dynamically influence stamina recovery
                var statComp = player.Character?.Components?.Get<MyEntityStatComponent>();
                if (statComp != null)
                {
                    staminaChangeRate += CalculateDynamicRPGStatEffects(statComp);
                }

                // Fatigue dynamically impacts stamina recovery efficiency
                float fatigueFactor = 1.0f - (fatigue.Value / fatigue.MaxValue); // Higher fatigue reduces recovery
                staminaChangeRate *= fatigueFactor;

                // Environmental effects dynamically adjust stamina recovery
                staminaChangeRate += EnvironmentalFactors.GetEnvironmentalFactor(player);

                // Nutrients dynamically enhance stamina recovery
                staminaChangeRate += CalculateDynamicNutrientImpactOnStamina(stats);

                // Prevent over-recovery
                float staminaLevel = stamina.Value / stamina.MaxValue;
                if (staminaLevel > 0.9f)
                {
                    staminaChangeRate *= 0.5f; // Slow recovery near max stamina
                }

                if (stamina.Value >= stamina.MaxValue)
                {
                    staminaChangeRate = 0f; // Prevent recovery above max
                }

                Core.iSurvivalSession.ApplyStatChange(player, stamina, 1, staminaChangeRate);
            }

            private static float CalculateDynamicRPGStatEffects(MyEntityStatComponent statComp)
            {
                float staminaChangeRate = 0f;

                // Strength dynamically reduces stamina drain
                MyEntityStat strength;
                if (statComp.TryGetStat(MyStringHash.GetOrCompute("Strength"), out strength))
                {
                    staminaChangeRate += (strength.Value / strength.MaxValue) * 5f; // Scale recovery with strength
                }

                // Dexterity dynamically increases stamina recovery
                MyEntityStat dexterity;
                if (statComp.TryGetStat(MyStringHash.GetOrCompute("Dexterity"), out dexterity))
                {
                    staminaChangeRate += (dexterity.Value / dexterity.MaxValue) * 3f; // Scale recovery with dexterity
                }


                return staminaChangeRate;
            }

            private static float CalculateDynamicNutrientImpactOnStamina(Dictionary<string, MyEntityStat> stats)
            {
                float nutrientImpact = 0f;

                foreach (var stat in stats)
                {
                    if (stat.Value == null || stat.Value.MaxValue == 0) continue;

                    // Higher nutrient levels dynamically boost stamina recovery
                    float normalizedStat = stat.Value.Value / stat.Value.MaxValue; // Normalize between 0-1
                    nutrientImpact += normalizedStat * 0.2f; // Adjust scale as needed
                }

                return nutrientImpact;
            }
        }


        public class RPGLeveling
        {
            // Base RPG stats for leveling
            public int Level { get; private set; }
            public float CurrentXP { get; private set; }
            public float XPToNextLevel { get; private set; }
            public int StatPoints { get; set; }


            // Reference to the player's stats (from effects.sbc)
            private Dictionary<string, MyEntityStat> stats;

            // Constructor: Initialize with player's stats
            public RPGLeveling(Dictionary<string, MyEntityStat> playerStats, int startingLevel = 1, float startingXP = 0)
            {
                Level = startingLevel;
                CurrentXP = startingXP;
                XPToNextLevel = CalculateXPForNextLevel();
                stats = playerStats;  // Load the player's stats (from MyEntityStat)
                StatPoints = 0;
            }

            // Calculate the XP required for the next level
            private float CalculateXPForNextLevel()
            {
                // Example XP scaling: exponential growth with level (customize as needed)
                return 100 * (float)Math.Pow(Level, 1.5f); // XP required grows as level increases
            }

            // Add XP method
            public void AddXP(float xpGained)
            {
                CurrentXP += xpGained;

                // Check for level-up
                while (CurrentXP >= XPToNextLevel)
                {
                    CurrentXP -= XPToNextLevel;
                    LevelUp();
                }
            }

            // Handle leveling up
            private void LevelUp()
            {
                Level++;
                XPToNextLevel = CalculateXPForNextLevel(); // Recalculate XP for the next level
                StatPoints += 5; // Award 5 stat points per level up (you can customize this)

                MyAPIGateway.Utilities.ShowMessage("Level Up", $"You reached Level {Level}!");
            }

            // Allocate stat points to a specific RPG stat
            public void AllocateStatPoints(IMyPlayer player, string statName, int points)
            {
                if (StatPoints < points)
                {
                    MyAPIGateway.Utilities.ShowMessage("Error", "Not enough stat points.");
                    return;
                }

                // Check if the stat exists in the player's stats (MyEntityStat)
                if (stats.ContainsKey(statName))
                {
                    var stat = stats[statName];
                    Core.iSurvivalSession.ApplyStatChange(player, stat, 1, points);
                    StatPoints -= points; // Subtract points from the available pool

                    MyAPIGateway.Utilities.ShowMessage("Stat Allocated", $"{points} points added to {statName}.");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("Error", "Invalid stat name.");
                }
            }

            // Optionally display all RPG stats
            public void DisplayStats(IMyPlayer player)
            {
                MyAPIGateway.Utilities.ShowMessage("RPG Stats", $"Level: {Level} | XP: {CurrentXP}/{XPToNextLevel} | Available Stat Points: {StatPoints}");

                // Display each stat from MyEntityStat
                foreach (var entry in stats)
                {
                    MyAPIGateway.Utilities.ShowMessage(entry.Key, $"{entry.Key}: {entry.Value.Value}/{entry.Value.MaxValue}");
                }
            }

            // Get current stats as a formatted string for logging or UI display
            public string GetStatsString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Level: {Level}, XP: {CurrentXP}/{XPToNextLevel}, Stat Points: {StatPoints}");
                foreach (var entry in stats)
                {
                    sb.AppendLine($"{entry.Key}: {entry.Value.Value}/{entry.Value.MaxValue}");
                }
                return sb.ToString();
            }
        }
        public static class Movement
        {
            public static void ProcessMovementEffects(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var movementState = player.Character.CurrentMovementState;
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina == null) return; // If stamina is null, we can't apply any changes related to stamina

                switch (movementState)
                {
                    case MyCharacterMovementEnum.Standing:
                    case MyCharacterMovementEnum.RotatingLeft:
                    case MyCharacterMovementEnum.RotatingRight:
                        ProcessStandingEffect(player, stats);
                        break;
                    case MyCharacterMovementEnum.Sprinting:
                        ProcessSprintingEffect(player, stats);
                        break;
                    case MyCharacterMovementEnum.Crouching:
                    case MyCharacterMovementEnum.CrouchRotatingLeft:
                    case MyCharacterMovementEnum.CrouchRotatingRight:
                        ProcessCrouchingEffect(player, stats);
                        break;
                    case MyCharacterMovementEnum.CrouchWalking:
                    case MyCharacterMovementEnum.CrouchBackWalking:
                    case MyCharacterMovementEnum.CrouchWalkingLeftBack:
                    case MyCharacterMovementEnum.CrouchWalkingLeftFront:
                    case MyCharacterMovementEnum.CrouchWalkingRightBack:
                    case MyCharacterMovementEnum.CrouchWalkingRightFront:
                    case MyCharacterMovementEnum.CrouchStrafingLeft:
                    case MyCharacterMovementEnum.CrouchStrafingRight:
                        ProcessCrouchWalkingEffect(player, stats);
                        break;
                    case MyCharacterMovementEnum.Walking:
                    case MyCharacterMovementEnum.BackWalking:
                    case MyCharacterMovementEnum.WalkStrafingLeft:
                    case MyCharacterMovementEnum.WalkStrafingRight:
                    case MyCharacterMovementEnum.WalkingRightBack:
                    case MyCharacterMovementEnum.WalkingRightFront:
                    case MyCharacterMovementEnum.Running:
                    case MyCharacterMovementEnum.Backrunning:
                    case MyCharacterMovementEnum.RunningLeftBack:
                    case MyCharacterMovementEnum.RunningLeftFront:
                    case MyCharacterMovementEnum.RunningRightBack:
                    case MyCharacterMovementEnum.RunningRightFront:
                    case MyCharacterMovementEnum.RunStrafingLeft:
                    case MyCharacterMovementEnum.RunStrafingRight:
                        ProcessWalkingEffect(player, stats);
                        if (movementState.ToString().Contains("Running"))
                        {
                            ProcessRunningEffect(player, stats);
                        }
                        break;
                    case MyCharacterMovementEnum.LadderUp:
                    case MyCharacterMovementEnum.LadderDown:
                        ProcessLadderEffect(player, stats);
                        break;
                    case MyCharacterMovementEnum.Flying:
                        ProcessFlyingEffect(player, stats);
                        break;
                    case MyCharacterMovementEnum.Falling:
                        ProcessFallingEffect(player, stats);
                        break;
                    case MyCharacterMovementEnum.Jump:
                        ProcessJumpingEffect(player, stats);
                        break;
                    default:
                        // Handle other movement states or do nothing
                        break;
                }
            }

            public static void ProcessStandingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, 1);
                }
            }

            public static void ProcessSprintingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                var dexterity = stats.ContainsKey("Dexterity") ? stats["Dexterity"] : null;
                var constitution = stats.ContainsKey("Constitution") ? stats["Constitution"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, -5);
                    if (dexterity.Value < 16) Core.iSurvivalSession.ApplyStatChange(player,dexterity, 1, 0.001);
                    if (constitution.Value < 16) Core.iSurvivalSession.ApplyStatChange(player,constitution, 1, 0.001);
                }
            }

            public static void ProcessCrouchingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, 1);
                }
            }

            public static void ProcessCrouchWalkingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                var dexterity = stats.ContainsKey("Dexterity") ? stats["Dexterity"] : null;
                var constitution = stats.ContainsKey("Constitution") ? stats["Constitution"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, 1);
                    if (dexterity.Value < 16) Core.iSurvivalSession.ApplyStatChange(player,dexterity, 1, 0.0001);
                    if (constitution.Value < 16) Core.iSurvivalSession.ApplyStatChange(player,constitution, 1, 0.0001);
                }
            }

            public static void ProcessWalkingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                var dexterity = stats.ContainsKey("Dexterity") ? stats["Dexterity"] : null;
                var constitution = stats.ContainsKey("Constitution") ? stats["Constitution"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, -1);
                    if (dexterity.Value < 16) Core.iSurvivalSession.ApplyStatChange(player, dexterity, 1, 0.0001);
                    if (constitution.Value < 16) Core.iSurvivalSession.ApplyStatChange(player, constitution, 1, 0.0001);
                }
            }

            public static void ProcessRunningEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                var dexterity = stats.ContainsKey("Dexterity") ? stats["Dexterity"] : null;
                var constitution = stats.ContainsKey("Constitution") ? stats["Constitution"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, -2);
                    if (dexterity.Value < 16) Core.iSurvivalSession.ApplyStatChange(player, dexterity, 1, 0.0001);
                    if (constitution.Value < 16) Core.iSurvivalSession.ApplyStatChange(player, constitution, 1, 0.0001);
                }
            }

            public static void ProcessLadderEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                // Add your logic for ladder effects
            }

            public static void ProcessFlyingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, -2.5);
                }
            }

            public static void ProcessFallingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                // Add your logic for falling effects
            }

            public static void ProcessJumpingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                var strength = stats.ContainsKey("Strength") ? stats["Strength"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, -5);
                    if (strength.Value < 16) Core.iSurvivalSession.ApplyStatChange(player, strength, 1, 0.0001);
                }
            }
        }
        public static class Metabolism
        {
            public static float CalculateMetabolicRate(IMyPlayer player)
            {
                float baseMetabolicRate = 0.1f; // Base metabolic rate

                // Get environmental factors
                float environmentFactor = EnvironmentalFactors.GetEnvironmentalFactor(player);
                float oxygenFactor = EnvironmentalFactors.OxygenLevelEnvironmentalFactor(player);

                // Ensure oxygenFactor is within a reasonable range
                oxygenFactor = MathHelper.Clamp(oxygenFactor, 0.1f, 2f);

                // Apply environmental factors to base metabolic rate
                baseMetabolicRate *= environmentFactor;

                // Adjust metabolic rate based on oxygen level
                if (oxygenFactor > 1.0f)
                {
                    baseMetabolicRate /= oxygenFactor; // Reduce rate when oxygen is high
                }
                else
                {
                    baseMetabolicRate *= (2.0f - oxygenFactor); // Increase rate when oxygen is low
                }

                // Adjust metabolic rate based on RPG stats
                ApplyRPGStatEffects(player, ref baseMetabolicRate);

                return baseMetabolicRate;
            }

            public static void ApplyRPGStatEffects(IMyPlayer player, ref float baseMetabolicRate)
            {
                var statComp = player.Character?.Components?.Get<MyEntityStatComponent>();
                MyEntityStat strength, dexterity, constitution;

                const float BASE_STAT = 10.0f; // D&D baseline for neutral stats
                const float MAX_EFFECT = 0.2f; // Maximum effect (20%) for extreme stats (e.g., 20 or 1)
                const float MIN_EFFECT = -0.2f; // Maximum penalty (20%) for extreme low stats

                if (statComp != null)
                {
                    // Get RPG stats
                    statComp.TryGetStat(MyStringHash.GetOrCompute("Strength"), out strength);
                    statComp.TryGetStat(MyStringHash.GetOrCompute("Dexterity"), out dexterity);
                    statComp.TryGetStat(MyStringHash.GetOrCompute("Constitution"), out constitution);

                    // Apply Strength impact: Higher strength reduces metabolic burn during physical activities
                    if (strength != null)
                    {
                        float strengthEffect = CalculateRPGEffect(strength.Value, strength.MaxValue, BASE_STAT, MAX_EFFECT, MIN_EFFECT);
                        baseMetabolicRate *= (1.0f - strengthEffect); // Reduce or increase burn rate

                        // Show message for strength effect
                        //MyAPIGateway.Utilities.ShowMessage("strength:", $"{strength.Value}, {strengthEffect}");
                    }

                    // Apply Dexterity impact: Higher dexterity makes movement more efficient, lowering stamina consumption
                    if (dexterity != null)
                    {
                        float dexterityEffect = CalculateRPGEffect(dexterity.Value, dexterity.MaxValue, BASE_STAT, MAX_EFFECT * 0.5f, MIN_EFFECT * 0.5f); // Max 10% impact
                        baseMetabolicRate *= (1.0f - dexterityEffect); // Reduce or increase stamina consumption

                        // Show message for dexterity effect
                        //MyAPIGateway.Utilities.ShowMessage("dexterity:", $"{dexterity.Value}, {dexterityEffect}");
                    }

                    // Apply Constitution impact: Higher constitution reduces fatigue and hunger
                    if (constitution != null)
                    {
                        float constitutionEffect = CalculateRPGEffect(constitution.Value, constitution.MaxValue, BASE_STAT, MAX_EFFECT * 0.75f, MIN_EFFECT * 0.75f); // Max 15% impact
                        baseMetabolicRate *= (1.0f - constitutionEffect); // Reduce or increase fatigue/hunger impact

                        // Show message for constitution effect
                        //MyAPIGateway.Utilities.ShowMessage("Constitution:",$"{constitution.Value}, {constitutionEffect}");
                    }
                }
                else
                {
                    iSurvivalLog.Error("RPG stats are missing or null.");
                }
            }

            // Helper function to calculate RPG effects based on stat values
            private static float CalculateRPGEffect(float currentStatValue, float maxValue, float baseStat, float maxEffect, float minEffect)
            {
                float normalizedStat = MathHelper.Clamp(currentStatValue, 1f, 20f); // Ensure stat is between 1 and 20
                float statDifference = normalizedStat - baseStat;
                float effect = statDifference / 10f; // Scale effect based on difference from baseStat (10)
                return MathHelper.Clamp(effect, minEffect, maxEffect); // Clamp to min/max effect
            }




            public static void ApplyMetabolismEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                if (player?.Character == null)
                    return;

                // Check if player is in a cryo block
                if (Blocks.ProcessBlockstEffects(player, stats, "Bed"))
                {
                    return; // Stop processing if the player is in a cryo block
                }
                var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
                if (statComp == null)
                    return;

                // Determine the metabolic rate multiplier based on environmental and activity factors
                float metabolicRate = CalculateMetabolicRate(player);

                // Apply metabolism effects
                ApplyMetabolicChanges(player, stats, metabolicRate);

                // Update Hunger after applying changes
                UpdateHunger(player, stats["Hunger"], stats);
            }

            private static void ApplyMetabolicChanges(IMyPlayer player, Dictionary<string, MyEntityStat> stats, float metabolicRate)
            {
                // Recommended daily values (based on a 24-hour day)
                const float dailyCalories = 2000f; // kcal
                const float dailyFat = 70f; // grams
                const float dailyCholesterol = 300f; // mg
                const float dailySodium = 2300f; // mg
                const float dailyFiber = 30f; // grams
                const float dailySugar = 36f; // grams
                const float dailyStarches = 220f; // grams
                const float dailyProtein = 50f; // grams
                const float dailyVitamins = 100f; // arbitrary units
                const float dailyWater = 3.7f; // liters

                var fatigue = stats.ContainsKey("Fatigue") ? stats["Fatigue"] : null;
                var sanity = stats.ContainsKey("Sanity") ? stats["Sanity"] : null;
                float fatigueMultiplier = CalculateFatigueEffect(fatigue, sanity);

                // Adjust for 2-hour day (120 minutes)
                float dayAdjustmentFactor = 1440f / 120f;
                float multiplier = fatigueMultiplier * dayAdjustmentFactor;

                // Convert daily values to per-minute burn rates for a 2-hour day
                float caloriesBurnRate = (dailyCalories / 1440f) * multiplier; // kcal/min
                float fatBurnRate = (dailyFat / 1440f) * multiplier; // grams/min
                float cholesterolBurnRate = (dailyCholesterol / 1440f) * multiplier; // mg/min                
                float sodiumBurnRate = (dailySodium / 1440f) * multiplier; // mg/min
                float fiberBurnRate = (dailyFiber / 1440f) * multiplier; // grams/min
                float sugarBurnRate = (dailySugar / 1440f) * multiplier; // grams/min
                float starchesBurnRate = (dailyStarches / 1440f) * multiplier; // grams/min
                float proteinBurnRate = (dailyProtein / 1440f) * multiplier; // grams/min
                float vitaminBurnRate = (dailyVitamins / 1440f) * multiplier; // units/min
                float waterBurnRate = (dailyWater / 1440f) * multiplier; // liters/min

                // Apply changes to each stat based on the metabolic rate and burn rates
                ApplyStatChange(player,stats, "Calories", metabolicRate, -caloriesBurnRate);
                ApplyStatChange(player,stats, "Fat", metabolicRate, -fatBurnRate);
                ApplyStatChange(player,stats, "Cholesterol", metabolicRate, -cholesterolBurnRate);
                ApplyStatChange(player,stats, "Sodium", metabolicRate, -sodiumBurnRate);
                ApplyStatChange(player,stats, "Fiber", metabolicRate, -fiberBurnRate);
                ApplyStatChange(player,stats, "Sugar", metabolicRate, -sugarBurnRate);
                ApplyStatChange(player,stats, "Starches", metabolicRate, -starchesBurnRate);
                ApplyStatChange(player,stats, "Protein", metabolicRate, -proteinBurnRate);
                ApplyStatChange(player,stats, "Vitamins", metabolicRate, -vitaminBurnRate);
                ApplyStatChange(player,stats, "Water", metabolicRate, -waterBurnRate);
            }

            private static void ApplyStatChange(IMyPlayer player, Dictionary<string, MyEntityStat> stats, string statName, float rate, float change)
            {
                if (stats.ContainsKey(statName) && stats[statName] != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stats[statName], rate, change);
                }
            }
            private static float CalculateFatigueEffect(MyEntityStat fatigue, MyEntityStat sanity)
            {
                if (fatigue == null || sanity == null)
                    return 1.0f;

                float fatigueLevel = fatigue.Value / fatigue.MaxValue;
                float sanityLevel = sanity.Value / sanity.MaxValue;

                // Lower sanity reduces fatigue recovery efficiency
                float sanityFactor = sanityLevel > 0.5f ? 1.0f : MathHelper.Lerp(0.5f, 1.0f, sanityLevel / 0.5f);

                // Adjust fatigue recovery multiplier
                return MathHelper.Clamp(fatigueLevel * sanityFactor, 0.3f, 1.5f);
            }

            public static void UpdateHunger(IMyPlayer player, MyEntityStat hunger, Dictionary<string, MyEntityStat> stats)
            {
                if (hunger == null || stats == null)
                {
                    iSurvivalLog.Error("Hunger or stats dictionary is null.");
                    return;
                }

                // Calculate the direct percentage of each nutrient
                double caloriePercentage = (stats["Calories"].Value / stats["Calories"].MaxValue) * 100;
                double fatPercentage = (stats["Fat"].Value / stats["Fat"].MaxValue) * 100;
                double cholesterolPercentage = (stats["Cholesterol"].Value / stats["Cholesterol"].MaxValue) * 100;
                double sodiumPercentage = (stats["Sodium"].Value / stats["Sodium"].MaxValue) * 100;
                double fiberPercentage = (stats["Fiber"].Value / stats["Fiber"].MaxValue) * 100;
                double sugarPercentage = (stats["Sugar"].Value / stats["Sugar"].MaxValue) * 100;
                double starchesPercentage = (stats["Starches"].Value / stats["Starches"].MaxValue) * 100;
                double proteinPercentage = (stats["Protein"].Value / stats["Protein"].MaxValue) * 100;
                double vitaminPercentage = (stats["Vitamins"].Value / stats["Vitamins"].MaxValue) * 100;

                // Calculate the average of these values to get the hunger level
                double averagePercentage = (caloriePercentage + fatPercentage + cholesterolPercentage + sodiumPercentage + fiberPercentage + sugarPercentage + starchesPercentage + proteinPercentage + vitaminPercentage + sugarPercentage) / 10.0;

                // Set hunger value directly based on the average percentage
                Core.iSurvivalSession.ApplyStatChange(player,hunger, 1, averagePercentage - hunger.Value);
            }
        }

    }
}
