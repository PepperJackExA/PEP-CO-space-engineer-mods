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
                FatigueAndStamina.ProcessFatigue(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
                Sanity.ProcessSanity(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);
            }
        }
        public static class Sanity
        {
            // Method to process sanity changes based on player stats and environment
            public static void ProcessSanity(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                if (player?.Character == null || sanity == null)
                    return;

                // Calculate the sanity change rate based on factors like nutrients, fatigue, and environmental conditions
                float sanityChangeRate = CalculateSanityChangeRate(player, sanity, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, hunger, water, fatigue, stamina);

                // Apply the calculated sanity change per minute (the rate is divided by 60 to apply it per second)
                Core.iSurvivalSession.ApplyStatChange(sanity, 1, sanityChangeRate / 60f);

                
            }

            // Calculate the sanity change rate based on multiple factors
            private static float CalculateSanityChangeRate(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                float sanityChangeRate = 0f;

                // Calculate sanity change based on nutrient levels
                sanityChangeRate += GetNutrientSanityEffect(calories);
                sanityChangeRate += GetNutrientSanityEffect(fat);
                sanityChangeRate += GetNutrientSanityEffect(cholesterol);
                sanityChangeRate += GetNutrientSanityEffect(sodium);
                sanityChangeRate += GetNutrientSanityEffect(carbohydrates);
                sanityChangeRate += GetNutrientSanityEffect(protein);
                sanityChangeRate += GetNutrientSanityEffect(vitamins);

                // Apply additional sanity change due to fatigue level
                sanityChangeRate += GetFatigueImpactOnSanity(fatigue);

                // Consider fatigue levels
                sanityChangeRate += GetFatigueSanityEffect(fatigue);
                // Consider environmental factors
                float environmentFactor = EnvironmentalFactors.GetEnvironmentalFactor(player);
                float oxygenFactor = EnvironmentalFactors.OxygenLevelEnvironmentalFactor(player);

                // Ensure oxygenFactor is within a reasonable range
                oxygenFactor = MathHelper.Clamp(oxygenFactor, 0.1f, 2f);

                // Apply environmental factors to base Sanity rate
                sanityChangeRate += oxygenFactor;
                sanityChangeRate -= environmentFactor;
                return sanityChangeRate;
            }

            // Calculate sanity change based on nutrient levels
            private static float GetNutrientSanityEffect(MyEntityStat nutrient)
            {
                if (nutrient == null)
                    return 0f;

                float nutrientPercentage = nutrient.Value / nutrient.MaxValue;

                if (nutrientPercentage < 0.1f)
                {
                    // If nutrient is below 10%, decrease sanity faster
                    return -1f;
                }
                else if (nutrientPercentage < 0.4f)
                {
                    // If nutrient is below 40%, decrease sanity moderately
                    return -0.75f;
                }
                else if (nutrientPercentage >= 0.6f)
                {
                    // If nutrient is above 60%, increase sanity
                    return -0.25f;
                }
                else
                {
                    // If nutrient is between 40% and 60%, no change to sanity
                    return -0.5f;
                }
            }
            // Determine the impact of fatigue on sanity
            private static float GetFatigueImpactOnSanity(MyEntityStat fatigue)
            {
                if (fatigue == null)
                    return 0f;

                float fatigueLevel = fatigue.Value / fatigue.MaxValue;

                if (fatigueLevel > 0.8f)
                {
                    // If fatigue is high (well-rested), sanity increases
                    return 1.5f;
                }
                else if (fatigueLevel < 0.2f)
                {
                    // If fatigue is low (very tired), sanity decreases significantly
                    return -2f;
                }
                else
                {
                    // Moderate fatigue has a small negative effect on sanity
                    return -0.5f;
                }
            }

            // Calculate sanity change based on fatigue levels
            private static float GetFatigueSanityEffect(MyEntityStat fatigue)
            {
                if (fatigue == null)
                    return 0f;

                float fatigueLevel = fatigue.Value / fatigue.MaxValue;

                // If fatigue is high (well-rested), increase sanity slightly
                if (fatigueLevel > 0.7f)
                {
                    return 0.5f;
                }
                // If fatigue is low (meaning low rest), decrease sanity
                else if (fatigueLevel < 0.3f)
                {
                    return -1f;
                }

                // Neutral fatigue levels have no impact
                return 0f;
            }           
        }

        public static class FatigueAndStamina
        {
            public static void ProcessFatigue(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                if (player?.Character == null || fatigue == null || stamina == null)
                    return;

                float fatigueChangeRate = CalculateFatigueChangeRate(calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins);

                // Adjust fatigue level based on nutrient levels
                Core.iSurvivalSession.ApplyStatChange(fatigue, 1, fatigueChangeRate / 60f); // Change fatigue per minute (1 point per hour rate)

                // Replenish stamina based on fatigue and nutrient levels
                ReplenishStamina(calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins, fatigue, stamina);
            }

            // Calculate the rate of fatigue change based on nutrient levels
            private static float CalculateFatigueChangeRate(MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins)
            {
                float changeRate = 0f;

                // Check each nutrient and adjust fatigue change rate accordingly
                changeRate += GetFatigueChangeRate(calories);
                changeRate += GetFatigueChangeRate(fat);
                changeRate += GetFatigueChangeRate(cholesterol);
                changeRate += GetFatigueChangeRate(sodium);
                changeRate += GetFatigueChangeRate(carbohydrates);
                changeRate += GetFatigueChangeRate(protein);
                changeRate += GetFatigueChangeRate(vitamins);

                return changeRate;
            }

            // Determine fatigue change rate for each nutrient
            private static float GetFatigueChangeRate(MyEntityStat nutrient)
            {
                if (nutrient == null)
                    return 0f;

                float nutrientPercentage = nutrient.Value / nutrient.MaxValue;

                if (nutrientPercentage < 0.2f)
                {
                    // If nutrient is below 20%, reduce fatigue faster
                    return -1f;
                }
                else if (nutrientPercentage < 0.4f)
                {
                    // If nutrient is below 40%, reduce fatigue slowly
                    return -0.75f;
                }
                else if (nutrientPercentage >= 0.6f)
                {
                    // If nutrient is above 60%, increase fatigue slowly
                    return -0.25f;
                }
                else
                {
                    // If nutrient is between 40% and 60%, no change to fatigue
                    return -0.50f;
                }
            }

            // Replenish stamina based on current fatigue and nutrient levels
            private static void ReplenishStamina(MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat fatigue, MyEntityStat stamina)
            {
                if (fatigue.Value <= 0 || stamina == null)
                    return;

                float staminaRestored = 0f;

                // Calculate how much stamina to restore based on nutrient levels and fatigue
                staminaRestored += GetStaminaRestoredFromNutrient(calories);
                staminaRestored += GetStaminaRestoredFromNutrient(fat);
                staminaRestored += GetStaminaRestoredFromNutrient(cholesterol);
                staminaRestored += GetStaminaRestoredFromNutrient(sodium);
                staminaRestored += GetStaminaRestoredFromNutrient(carbohydrates);
                staminaRestored += GetStaminaRestoredFromNutrient(protein);
                staminaRestored += GetStaminaRestoredFromNutrient(vitamins);

                // Convert fatigue to stamina: 1 fatigue point restores 100 stamina points
                float staminaRestoredFromFatigue = Math.Min(staminaRestored, fatigue.Value * 100f);

                // Apply stamina restoration
                if (staminaRestoredFromFatigue > 0)
                {
                    if (stamina.Value >= 100) return;
                    
                    Core.iSurvivalSession.ApplyStatChange(stamina, 1, staminaRestoredFromFatigue);
                    Core.iSurvivalSession.ApplyStatChange(fatigue, 1, -staminaRestoredFromFatigue / 100); // Deduct the used fatigue
                    
                }
            }

            // Calculate the stamina restored from a single nutrient based on its percentage
            private static float GetStaminaRestoredFromNutrient(MyEntityStat nutrient)
            {
                if (nutrient == null)
                    return 0f;

                // Calculate the stamina contribution based on current nutrient level
                return MathHelper.Clamp(nutrient.Value / nutrient.MaxValue, 0f, 1f);
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
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Sprinting");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -10);
            }
            public static void ProcessCrouchingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Crouching");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -3);
            }
            public static void ProcessCrouchWalkingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"CrouchWalking");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -2);
            }
            public static void ProcessWalkingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Walking");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -1);
            }
            public static void ProcessRunningEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Running");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -5);
            }
            public static void ProcessLadderEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Ladder");
            }
            public static void ProcessFlyingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Flying");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -5);
            }
            public static void ProcessFallingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Falling");
            }
            public static void ProcessJumpingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                //MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Jumping");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -10);
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
                // Recommended daily values (based on a 24-hour day)
                const float dailyCalories = 2000f; // kcal
                const float dailyFat = 70f; // grams
                const float dailyCholesterol = 300f; // mg
                const float dailySodium = 2300f; // mg
                const float dailyCarbohydrates = 275f; // grams
                const float dailyProtein = 50f; // grams
                const float dailyVitamins = 100f; // arbitrary units
                const float dailyWater = 3.7f; // liters

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
                        

        }
    }
}
