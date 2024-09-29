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
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace PEPCO.iSurvival.Effects
{
    public static class Processes
    {
        public static void ProcessPlayer(IMyPlayer player, MyEntityStatComponent statComp)
        {

            // Initialize the stats
            MyEntityStat sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina;

            // Retrieve each stat from the component
            statComp.TryGetStat(MyStringHash.GetOrCompute("Sanity"), out sanity);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Calories"), out calories);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Fat"), out fat);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Cholesterol"), out cholesterol);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Sodium"), out sodium);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Carbohydrates"), out carbohydrates);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Protein"), out protein);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Vitamins"), out vitamins);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Hunger"), out hunger);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Water"), out water);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Fatigue"), out fatigue);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Stamina"), out stamina);

            if (hunger != null)
            {
                // Apply metabolism effect to the player using all the retrieved stats
                Metabolism.ApplyMetabolismEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                Movement.ProcessMovementEffects(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
            }
        }

        public static class Movement
        {
            public static void ProcessMovementEffects(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                var movementState = player.Character.CurrentMovementState;
                switch (movementState)
                {
                    case MyCharacterMovementEnum.Standing:
                    case MyCharacterMovementEnum.RotatingLeft:
                    case MyCharacterMovementEnum.RotatingRight:
                        ProcessStandingEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                        break;
                    case MyCharacterMovementEnum.Sprinting:
                        ProcessSprintingEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                        break;
                    case MyCharacterMovementEnum.Crouching:
                    case MyCharacterMovementEnum.CrouchRotatingLeft:
                    case MyCharacterMovementEnum.CrouchRotatingRight:
                        ProcessCrouchingEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                        break;
                    case MyCharacterMovementEnum.CrouchWalking:
                    case MyCharacterMovementEnum.CrouchBackWalking:
                    case MyCharacterMovementEnum.CrouchWalkingLeftBack:
                    case MyCharacterMovementEnum.CrouchWalkingLeftFront:
                    case MyCharacterMovementEnum.CrouchWalkingRightBack:
                    case MyCharacterMovementEnum.CrouchWalkingRightFront:
                    case MyCharacterMovementEnum.CrouchStrafingLeft:
                    case MyCharacterMovementEnum.CrouchStrafingRight:
                        ProcessCrouchWalkingEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
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
                        ProcessWalkingEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                        switch (movementState)
                        {
                            case MyCharacterMovementEnum.Running:
                            case MyCharacterMovementEnum.Backrunning:
                            case MyCharacterMovementEnum.RunningLeftBack:
                            case MyCharacterMovementEnum.RunningLeftFront:
                            case MyCharacterMovementEnum.RunningRightBack:
                            case MyCharacterMovementEnum.RunningRightFront:
                            case MyCharacterMovementEnum.RunStrafingLeft:
                            case MyCharacterMovementEnum.RunStrafingRight:
                                ProcessRunningEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                                break;
                        }
                        break;
                    case MyCharacterMovementEnum.LadderUp:
                    case MyCharacterMovementEnum.LadderDown:
                        ProcessLadderEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                        break;
                    case MyCharacterMovementEnum.Flying:
                        ProcessFlyingEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                        break;
                    case MyCharacterMovementEnum.Falling:
                        ProcessFallingEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                        break;
                    case MyCharacterMovementEnum.Jump:
                        ProcessJumpingEffect(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                        break;
                    default:
                        // Handle other movement states or do nothing
                        break;
                }
            }
            public static void ProcessStandingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Standing");
                //Core.iSurvivalSession.ApplyStatChange(stamina, 1, 1);

            }
            public static void ProcessSprintingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Sprinting");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -5);
            }
            public static void ProcessCrouchingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Crouching");
                //Core.iSurvivalSession.ApplyStatChange(stamina, 1, 3);
            }
            public static void ProcessCrouchWalkingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"CrouchWalking");
                //Core.iSurvivalSession.ApplyStatChange(stamina, 1, 2);
            }
            public static void ProcessWalkingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Walking");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -1);
            }
            public static void ProcessRunningEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Running");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -2);
            }
            public static void ProcessLadderEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Ladder");
            }
            public static void ProcessFlyingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Flying");
            }
            public static void ProcessFallingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Falling");
            }
            public static void ProcessJumpingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Jumping");
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

                // Ensure oxygenFactor is within a reasonable range, just in case
                oxygenFactor = MathHelper.Clamp(oxygenFactor, 0.1f, 2f);

                // Apply environmental factors to base metabolic rate
                baseMetabolicRate *= environmentFactor;

                // Adjust metabolic rate based on oxygen level
                if (oxygenFactor > 1.0f)
                {
                    // If oxygen level is high, reduce the metabolic rate (good thing)
                    baseMetabolicRate /= oxygenFactor;
                }
                else
                {
                    // If oxygen level is low, increase metabolic rate (bad thing)
                    baseMetabolicRate *= (2.0f - oxygenFactor); // Making the impact more severe
                }

                // Adjust metabolic rate based on stamina level
                var statComp = player.Character?.Components?.Get<MyEntityStatComponent>();
                MyEntityStat staminaStat;

                if (statComp != null && statComp.TryGetStat(MyStringHash.GetOrCompute("Stamina"), out staminaStat) && staminaStat != null)
                {
                    // Normalize stamina value to a range of 0 to 1 (where 1 is full stamina and 0 is no stamina)
                    float staminaLevel = staminaStat.Value / staminaStat.MaxValue;

                    // Apply inverse effect: lower stamina leads to higher metabolic rate
                    if (staminaLevel < 1.0f)
                    {
                        baseMetabolicRate *= (2.0f - staminaLevel); // Increase burn rate as stamina decreases
                    }
                }
                else
                {
                    iSurvivalLog.Error("CalculateMetabolicRate: Stamina stat is missing or null.");
                }

                return baseMetabolicRate;
            }

            public static void ApplyMetabolismEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                if (player?.Character == null)
                    return;

                var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
                if (statComp == null)
                    return;

                // Determine the metabolic rate multiplier based on environmental and activity factors
                float metabolicRate = CalculateMetabolicRate(player);

                // Apply metabolism effects
                ApplyMetabolicChanges(statComp, metabolicRate, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);

                // Update Hunger after applying changes
                UpdateHunger(hunger, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins);

            }

            private static void ApplyMetabolicChanges(MyEntityStatComponent statComp, float metabolicRate, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                // Recommended daily values
                const float dailyCalories = 2000f; // kcal
                const float dailyFat = 70f; // grams
                const float dailyCholesterol = 300f; // mg
                const float dailySodium = 2300f; // mg
                const float dailyCarbohydrates = 275f; // grams
                const float dailyProtein = 50f; // grams
                const float dailyVitamins = 100f; // arbitrary units
                const float dailyWater = 3.7f; // liters

                // Convert daily values to per-minute burn rates (assuming a 24-hour day)
                float caloriesBurnRate = dailyCalories / 1440f; // kcal/min
                float fatBurnRate = dailyFat / 1440f; // grams/min
                float cholesterolBurnRate = dailyCholesterol / 1440f; // mg/min
                float sodiumBurnRate = dailySodium / 1440f; // mg/min
                float carbBurnRate = dailyCarbohydrates / 1440f; // grams/min
                float proteinBurnRate = dailyProtein / 1440f; // grams/min
                float vitaminBurnRate = dailyVitamins / 1440f; // units/min
                float waterBurnRate = dailyWater / 1440f; // liters/min

                // Apply changes to each stat based on the metabolic rate and burn rates
                if (calories != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(calories, metabolicRate, -caloriesBurnRate); // Decrease calories
                }
                if (fat != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(fat, metabolicRate, -fatBurnRate); // Decrease fat
                }
                if (cholesterol != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(cholesterol, metabolicRate, -cholesterolBurnRate); // Decrease cholesterol
                }
                if (sodium != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(sodium, metabolicRate, -sodiumBurnRate); // Decrease sodium
                }
                if (carbohydrates != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(carbohydrates, metabolicRate, -carbBurnRate); // Decrease carbohydrates
                }
                if (protein != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(protein, metabolicRate, -proteinBurnRate); // Decrease protein
                }
                if (vitamins != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(vitamins, metabolicRate, -vitaminBurnRate); // Decrease vitamins
                }
                if (water != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(water, metabolicRate, -waterBurnRate); // Decrease water
                }
                // Restore stamina using nutrient stats and adjust based on nutrient fullness
                RestoreStaminaUsingNutrients(calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, stamina, fatigue);
                // Apply fatigue effects dynamically based on nutrient levels and stamina
                AdjustFatigueLevels(calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, stamina, fatigue);
            }
            private static void RestoreStaminaUsingNutrients(MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat stamina, MyEntityStat fatigue)
            {
                // Define nutrient contribution factors (how much each nutrient contributes to stamina restoration)
                const float calorieFactor = 0.01f;
                const float fatFactor = 0.02f;
                const float cholesterolFactor = 0.01f;
                const float sodiumFactor = 0.005f;
                const float carbFactor = 0.015f;
                const float proteinFactor = 0.02f;
                const float vitaminFactor = 0.005f;

                // Check the current value of each nutrient and calculate the contribution to stamina restoration
                float staminaRestored = 0f;
                float fatigueMultiplier = CalculateFatigueEffect(fatigue);

                // Apply nutrient contributions to stamina, considering fatigue multiplier
                staminaRestored += ConsumeNutrient(calories, 1, calorieFactor * fatigueMultiplier);
                staminaRestored += ConsumeNutrient(fat, 1, fatFactor * fatigueMultiplier);
                staminaRestored += ConsumeNutrient(cholesterol, 1, cholesterolFactor * fatigueMultiplier);
                staminaRestored += ConsumeNutrient(sodium, 1, sodiumFactor * fatigueMultiplier);
                staminaRestored += ConsumeNutrient(carbohydrates, 1, carbFactor * fatigueMultiplier);
                staminaRestored += ConsumeNutrient(protein, 1, proteinFactor * fatigueMultiplier);
                staminaRestored += ConsumeNutrient(vitamins, 1, vitaminFactor * fatigueMultiplier);

                // Apply the total restored stamina
                if (stamina != null && staminaRestored > 0)
                {
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, staminaRestored); // Increase stamina
                }
            }
            private static float ConsumeNutrient(MyEntityStat nutrient, float maxConsume, float factor)
            {
                if (nutrient != null && nutrient.Value > 0)
                {
                    float consumeAmount = Math.Min(nutrient.Value, maxConsume);
                    Core.iSurvivalSession.ApplyStatChange(nutrient, 1, -consumeAmount); // Deplete some nutrient
                    return consumeAmount * factor;
                }
                return 0;
            }
            private static float CalculateFatigueEffect(MyEntityStat fatigue)
            {
                if (fatigue == null)
                    return 1.0f; // No fatigue impact if fatigue stat is missing

                float fatigueLevel = fatigue.Value / fatigue.MaxValue;

                // Higher fatigue reduces stamina gain, lower fatigue enhances it
                return MathHelper.Clamp(1.0f - fatigueLevel, 0.5f, 1.5f); // Adjust the range as needed
            }
            private static void AdjustFatigueLevels(MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat stamina, MyEntityStat fatigue)
            {
                if (fatigue == null || stamina == null)
                    return;

                float staminaLevel = stamina.Value / stamina.MaxValue;

                // Increase fatigue if stamina is low and nutrients are insufficient
                if (staminaLevel < 0.3f && !AreNutrientsSufficient(calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins))
                {
                    Core.iSurvivalSession.ApplyStatChange(fatigue, 1, 0.5f); // Increase fatigue
                }
                else if (staminaLevel > 0.7f && AreNutrientsSufficient(calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins))
                {
                    Core.iSurvivalSession.ApplyStatChange(fatigue, 1, -0.5f); // Decrease fatigue
                }
            }
            private static bool AreNutrientsSufficient(MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins)
            {
                return calories.Value / calories.MaxValue > 0.5f &&
                       fat.Value / fat.MaxValue > 0.5f &&
                       cholesterol.Value / cholesterol.MaxValue > 0.5f &&
                       sodium.Value / sodium.MaxValue > 0.5f &&
                       carbohydrates.Value / carbohydrates.MaxValue > 0.5f &&
                       protein.Value / protein.MaxValue > 0.5f &&
                       vitamins.Value / vitamins.MaxValue > 0.5f;
            }

            public static void UpdateHunger(MyEntityStat hunger, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins)
            {
                if (hunger == null || calories == null || fat == null || cholesterol == null || sodium == null || carbohydrates == null || protein == null || vitamins == null)
                {
                    iSurvivalLog.Error("One or more stats required for hunger update are null.");
                    return;
                }

                // Calculate the direct percentage of each nutrient
                double caloriePercentage = (calories.Value / calories.MaxValue) * 100;
                double fatPercentage = (fat.Value / fat.MaxValue) * 100;
                double cholesterolPercentage = (cholesterol.Value / cholesterol.MaxValue) * 100;
                double sodiumPercentage = (sodium.Value / sodium.MaxValue) * 100;
                double carbPercentage = (carbohydrates.Value / carbohydrates.MaxValue) * 100;
                double proteinPercentage = (protein.Value / protein.MaxValue) * 100;
                double vitaminPercentage = (vitamins.Value / vitamins.MaxValue) * 100;

                // Calculate the average of these values to get the hunger level
                double averagePercentage = (caloriePercentage + fatPercentage + cholesterolPercentage + sodiumPercentage + carbPercentage + proteinPercentage + vitaminPercentage) / 7.0;

                // Set hunger value directly based on the average percentage
                Core.iSurvivalSession.ApplyStatChange(hunger, 1, averagePercentage - hunger.Value);
            }


            public static class HungerTracker
            {
                private static readonly Dictionary<long, double> previousHungerValues = new Dictionary<long, double>();
                private static readonly Dictionary<long, double> previousUpdateTimes = new Dictionary<long, double>();

                public static double CalculateActualHungerDecreaseRate(IMyPlayer player, MyEntityStat hunger)
                {
                    // Get the current time in minutes using a more accurate method
                    double currentTime = MyAPIGateway.Session.ElapsedPlayTime.TotalMinutes;

                    long playerId = player.IdentityId;

                    double previousHunger;
                    double previousTime;

                    if (previousHungerValues.TryGetValue(playerId, out previousHunger) &&
                        previousUpdateTimes.TryGetValue(playerId, out previousTime))
                    {
                        double timeInterval = currentTime - previousTime;
                        if (timeInterval <= 0.001) return 0; // Avoid division by zero

                        double hungerDifference = previousHunger - hunger.Value;
                        double hungerDecreaseRate = (hungerDifference / timeInterval) * 100; // Hunger decrease per minute

                        previousHungerValues[playerId] = hunger.Value;
                        previousUpdateTimes[playerId] = currentTime;

                        return hungerDecreaseRate;
                    }

                    else
                    {
                        previousHungerValues[playerId] = hunger.Value;
                        previousUpdateTimes[playerId] = currentTime;

                        return 0; // No rate calculated on first update
                    }
                }

                public static void UpdateHungerAndCalculateTimeRemaining(IMyPlayer player)
                {
                    var statComponent = player.Character?.Components?.Get<MyEntityStatComponent>();
                    if (statComponent == null)
                    {
                        iSurvivalLog.Info("Stat component not found.");
                        return;
                    }
                    MyEntityStat targetHunger;
                    if (statComponent.TryGetStat(MyStringHash.GetOrCompute("Hunger"), out targetHunger))
                    {
                        double actualHungerDecreaseRate = CalculateActualHungerDecreaseRate(player, targetHunger);
                        if (Math.Abs(actualHungerDecreaseRate) < 0.0001)
                        {
                            actualHungerDecreaseRate = 0.1; // Set a default minimum rate
                        }

                        double timeRemaining = targetHunger.Value / Math.Abs(actualHungerDecreaseRate);

                        // Example logging message
                        MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Current Hunger Decrease Rate: {actualHungerDecreaseRate:F2} units/min. Time to Starvation: {timeRemaining:F1} min.");
                        iSurvivalLog.Info($"Player {player.DisplayName}: Current Hunger Decrease Rate: {actualHungerDecreaseRate:F2} units/min. Time to Starvation: {timeRemaining:F1} min.");
                    }
                }
            }

        }
    }
}
