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

namespace PEPCO.iSurvival.Effects
{
    public static class Processes
    {
        public static void ProcessPlayer(IMyPlayer player, MyEntityStatComponent statComp)
        {

            // Use GetAllStats to retrieve and display the player's stats
            PlayerStatsHelper.GetAllStats(player);

            // Now retrieve individual stats that we specifically need
            MyEntityStat health;
            statComp.TryGetStat(MyStringHash.GetOrCompute("Health"), out health);

            // Use TryGetStat for each stat and store them in a dictionary
            var stats = new Dictionary<string, MyEntityStat>();
            foreach (var statName in StatManager._statSettings.Keys)
            {
                MyEntityStat stat;
                if (statComp.TryGetStat(MyStringHash.GetOrCompute(statName), out stat))
                {
                    stats[statName] = stat;
                }
            }
            var fatigue = stats["Fatigue"];
            var hunger = stats["Hunger"];
            var stamina = stats["Stamina"];

            if (fatigue != null)
            {
                // BLINK STUFF
                if (fatigue.Value < 20 && Core.iSurvivalSession.rand.Next((int)fatigue.Value) < 5)
                {
                    Core.iSurvivalSession.blinkList.Add(player.IdentityId);
                    if (player.IdentityId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                        Effects.Processes.Blink.blink(true);
                    else if (MyAPIGateway.Multiplayer.IsServer)
                        MyAPIGateway.Multiplayer.SendMessageTo(Core.iSurvivalSession.modId, Encoding.ASCII.GetBytes("blink"), MyVisualScriptLogicProvider.GetSteamId(player.IdentityId), true);
                }
            }
            // If the hunger stat is valid, apply various effects
            if (hunger != null)
            {
                // Starvation Stuff
                if (hunger.Value < 20 && Core.iSurvivalSession.rand.Next((int)fatigue.Value) >0)
                {
                    
                    health.Value = health.Value - (1-(hunger.Value/20));
                    //MyAPIGateway.Utilities.ShowMessage("sitting:", $"Health Damage:{hunger.Value}  | {(1-(hunger.Value / 20))}");
                }
                // Apply other processes
                Metabolism.ApplyMetabolismEffect(player, stats);
                FatigueAndStamina.ProcessFatigue(player, stats);
                Sanity.ProcessSanity(player, stats);
                Movement.ProcessMovementEffects(player, stats);
                Blocks.ProcessBlockstEffects(player, stats);
            }

            
        
        }
        public static class Blocks
        {
            public static void ProcessBlockstEffects(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var block = player.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
                if (block == null) return;

                var fatigue = stats["Fatigue"];
                var sanity = stats["Sanity"];
                var hunger = stats["Hunger"];
                var water = stats["Water"];
                var stamina = stats["Stamina"];

                float averageStats = (fatigue.Value + sanity.Value + hunger.Value + water.Value) / 4;

                var blockDef = block.BlockDefinition.SubtypeId.ToString();
                if (blockDef.Contains("Cryo")) return;

                if (blockDef.Contains("Bed"))
                {
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, 2);
                    Core.iSurvivalSession.ApplyStatChange(fatigue, 1, 2);
                    if (sanity.Value < averageStats)
                    {
                        Core.iSurvivalSession.ApplyStatChange(sanity, 1, 0.25);
                    }
                    return;
                }
                else if (blockDef.Contains("Toilet") || blockDef.Contains("Bathroom"))
                {
                    Core.iSurvivalSession.ApplyStatChange(fatigue, 1, 1);
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, 2);
                    if (hunger.Value > 20)
                    {
                        Core.iSurvivalSession.ApplyStatChange(sanity, 1, 0.5);
                        player.Character.GetInventory(0).AddItems((MyFixedPoint)(((100 - averageStats) / 100)), (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                    }
                }
                else
                {
                    if (fatigue.Value < 25)
                    {
                        Core.iSurvivalSession.ApplyStatChange(fatigue, 1, 0.25);
                    }
                    if (stamina.Value < 25)
                    {
                        Core.iSurvivalSession.ApplyStatChange(stamina, 1, 1);
                    }
                }
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
                var sanity = stats["Sanity"];
                if (sanity == null) return;

                float sanityChangeRate = CalculateSanityChangeRate(stats);
                Core.iSurvivalSession.ApplyStatChange(sanity, 1, sanityChangeRate / 60f);
            }

            private static float CalculateSanityChangeRate(Dictionary<string, MyEntityStat> stats)
            {
                float sanityChangeRate = 0f;
                var fatigue = stats["Fatigue"];
                var environmentFactor = EnvironmentalFactors.GetEnvironmentalFactor(null);
                float oxygenFactor = EnvironmentalFactors.OxygenLevelEnvironmentalFactor(null);
                oxygenFactor = MathHelper.Clamp(oxygenFactor, 0.1f, 2f);

                sanityChangeRate += oxygenFactor;
                sanityChangeRate -= environmentFactor;
                sanityChangeRate += GetFatigueImpactOnSanity(fatigue);

                return sanityChangeRate;
            }

            private static float GetFatigueImpactOnSanity(MyEntityStat fatigue)
            {
                if (fatigue == null) return 0f;
                float fatigueLevel = fatigue.Value / fatigue.MaxValue;
                return fatigueLevel > 0.8f ? 1.5f : (fatigueLevel < 0.2f ? -2f : -0.5f);
            }
        }
        public static class FatigueAndStamina
        {
            public static void ProcessFatigue(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var fatigue = stats["Fatigue"];
                if (fatigue == null) return;

                float fatigueChangeRate = CalculateFatigueChangeRate(stats);
                Core.iSurvivalSession.ApplyStatChange(fatigue, 1, fatigueChangeRate / 60f);
            }

            public static float CalculateFatigueChangeRate(Dictionary<string, MyEntityStat> stats)
            {
                float changeRate = 0f;
                foreach (var stat in stats)
                {
                    changeRate += GetFatigueChangeRate(stat.Value);
                }
                return changeRate;
            }

            private static float GetFatigueChangeRate(MyEntityStat nutrient)
            {
                if (nutrient == null) return 0f;
                float nutrientPercentage = nutrient.Value / nutrient.MaxValue;
                return nutrientPercentage < 0.2f ? -1f : (nutrientPercentage < 0.4f ? -0.75f : -0.5f);
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
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, 1);
                }
            }

            public static void ProcessSprintingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, -10);
                }
            }

            public static void ProcessCrouchingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, -3);
                }
            }

            public static void ProcessCrouchWalkingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, -2);
                }
            }

            public static void ProcessWalkingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, -1);
                }
            }

            public static void ProcessRunningEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, -5);
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
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, -5);
                }
            }

            public static void ProcessFallingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                // Add your logic for falling effects
            }

            public static void ProcessJumpingEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats.ContainsKey("Stamina") ? stats["Stamina"] : null;
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, -10);
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

                // Adjust metabolic rate based on stamina level
                var statComp = player.Character?.Components?.Get<MyEntityStatComponent>();
                MyEntityStat staminaStat;

                if (statComp != null && statComp.TryGetStat(MyStringHash.GetOrCompute("Stamina"), out staminaStat) && staminaStat != null)
                {
                    float staminaLevel = staminaStat.Value / staminaStat.MaxValue;
                    baseMetabolicRate *= (2.0f - staminaLevel); // Increase burn rate as stamina decreases
                }
                else
                {
                    iSurvivalLog.Error("CalculateMetabolicRate: Stamina stat is missing or null.");
                }

                return baseMetabolicRate;
            }

            public static void ApplyMetabolismEffect(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                if (player?.Character == null)
                    return;

                var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
                if (statComp == null)
                    return;

                // Determine the metabolic rate multiplier based on environmental and activity factors
                float metabolicRate = CalculateMetabolicRate(player);

                // Apply metabolism effects
                ApplyMetabolicChanges(stats, metabolicRate);

                // Update Hunger after applying changes
                UpdateHunger(stats["Hunger"], stats);
            }

            private static void ApplyMetabolicChanges(Dictionary<string, MyEntityStat> stats, float metabolicRate)
            {
                // Recommended daily values (based on a 24-hour day)
                const float dailyCalories = 2000f; // kcal
                const float dailyFat = 70f; // grams
                const float dailyCholesterol = 300f; // mg
                const float dailySodium = 2300f; // mg
                const float dailyCarbohydrates = 275f; // grams
                const float dailyProtein = 50f; // grams
                const float dailyVitamins = 100f; // arbitrary units
                const float dailyWater = 3.7f; // liters

                var fatigue = stats.ContainsKey("Fatigue") ? stats["Fatigue"] : null;
                float fatigueMultiplier = CalculateFatigueEffect(fatigue);

                // Adjust for 2-hour day (120 minutes)
                float dayAdjustmentFactor = 1440f / 120f;
                float multiplier = fatigueMultiplier * dayAdjustmentFactor;

                // Convert daily values to per-minute burn rates for a 2-hour day
                float caloriesBurnRate = (dailyCalories / 1440f) * multiplier; // kcal/min
                float fatBurnRate = (dailyFat / 1440f) * multiplier; // grams/min
                float cholesterolBurnRate = (dailyCholesterol / 1440f) * multiplier; // mg/min
                float sodiumBurnRate = (dailySodium / 1440f) * multiplier; // mg/min
                float carbBurnRate = (dailyCarbohydrates / 1440f) * multiplier; // grams/min
                float proteinBurnRate = (dailyProtein / 1440f) * multiplier; // grams/min
                float vitaminBurnRate = (dailyVitamins / 1440f) * multiplier; // units/min
                float waterBurnRate = (dailyWater / 1440f) * multiplier; // liters/min

                // Apply changes to each stat based on the metabolic rate and burn rates
                ApplyStatChange(stats, "Calories", metabolicRate, -caloriesBurnRate);
                ApplyStatChange(stats, "Fat", metabolicRate, -fatBurnRate);
                ApplyStatChange(stats, "Cholesterol", metabolicRate, -cholesterolBurnRate);
                ApplyStatChange(stats, "Sodium", metabolicRate, -sodiumBurnRate);
                ApplyStatChange(stats, "Carbohydrates", metabolicRate, -carbBurnRate);
                ApplyStatChange(stats, "Protein", metabolicRate, -proteinBurnRate);
                ApplyStatChange(stats, "Vitamins", metabolicRate, -vitaminBurnRate);
                ApplyStatChange(stats, "Water", metabolicRate, -waterBurnRate);
            }

            private static void ApplyStatChange(Dictionary<string, MyEntityStat> stats, string statName, float rate, float change)
            {
                if (stats.ContainsKey(statName) && stats[statName] != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(stats[statName], rate, change);
                }
            }

            private static float CalculateFatigueEffect(MyEntityStat fatigue)
            {
                if (fatigue == null)
                    return 1.0f; // No fatigue impact if fatigue stat is missing

                // Calculate fatigue level: higher values mean more rested, so more efficient
                float fatigueLevel = fatigue.Value / fatigue.MaxValue;

                // Lower fatigue reduces stamina gain, higher fatigue enhances it
                // When fatigue is high (well-rested), the effect is closer to 1.5
                // When fatigue is low (tired), the effect is closer to 0.3
                return MathHelper.Clamp(fatigueLevel, 0.3f, 1.5f);
            }

            public static void UpdateHunger(MyEntityStat hunger, Dictionary<string, MyEntityStat> stats)
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
                double carbPercentage = (stats["Carbohydrates"].Value / stats["Carbohydrates"].MaxValue) * 100;
                double proteinPercentage = (stats["Protein"].Value / stats["Protein"].MaxValue) * 100;
                double vitaminPercentage = (stats["Vitamins"].Value / stats["Vitamins"].MaxValue) * 100;

                // Calculate the average of these values to get the hunger level
                double averagePercentage = (caloriePercentage + fatPercentage + cholesterolPercentage + sodiumPercentage + carbPercentage + proteinPercentage + vitaminPercentage) / 7.0;

                // Set hunger value directly based on the average percentage
                Core.iSurvivalSession.ApplyStatChange(hunger, 1, averagePercentage - hunger.Value);
            }
        }

    }
}
