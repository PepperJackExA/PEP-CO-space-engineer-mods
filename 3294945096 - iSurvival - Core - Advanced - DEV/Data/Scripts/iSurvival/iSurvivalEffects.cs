using PEPCO.iSurvival.factors;
using PEPCO.iSurvival.Log;
using PEPCO.iSurvival.stats;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
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
        public static void ProcessPlayer(IMyPlayer player,MyEntityStatComponent statComp)
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
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Standing");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, 1);

            }
            public static void ProcessSprintingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Sprinting");
                Core.iSurvivalSession.ApplyStatChange(stamina, 1, -3);
            }
            public static void ProcessCrouchingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Crouching");
            }
            public static void ProcessCrouchWalkingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"CrouchWalking");
            }
            public static void ProcessWalkingEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Walking");
            }
            public static void ProcessRunningEffect(IMyPlayer player, MyEntityStat sanity, MyEntityStat calories, MyEntityStat fat, MyEntityStat cholesterol, MyEntityStat sodium, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat water, MyEntityStat fatigue, MyEntityStat stamina)
            {
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Running");
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
                ApplyMetabolicChanges(statComp, metabolicRate, calories, fat, carbohydrates, protein, vitamins, hunger, fatigue, stamina);

                // Update Hunger after applying changes
                UpdateHunger(hunger, calories, fat, cholesterol, sodium, carbohydrates, protein, vitamins);
                HungerTracker.UpdateHungerAndCalculateTimeRemaining(player, hunger);
            }

            private static void ApplyMetabolicChanges(MyEntityStatComponent statComp, float metabolicRate, MyEntityStat calories, MyEntityStat fat, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins, MyEntityStat hunger, MyEntityStat fatigue, MyEntityStat stamina)
            {
                // Apply changes to each stat based on metabolic rate
                if (calories != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(calories, metabolicRate, -1); // Decrease calories
                }
                if (fat != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(fat, metabolicRate, -0.1); // Decrease fat
                }
                if (carbohydrates != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(carbohydrates, metabolicRate, -0.5); // Decrease carbohydrates
                }
                if (protein != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(protein, metabolicRate, -0.1); // Decrease protein
                }
                if (vitamins != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(vitamins, metabolicRate, -0.05); // Decrease vitamins
                }
                if (fatigue != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(fatigue, metabolicRate, 0.5); // Increase fatigue
                }
                if (stamina != null)
                {
                    Core.iSurvivalSession.ApplyStatChange(stamina, metabolicRate, 1); // Increase stamina
                }
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
                // Dictionary to store the previous hunger values for each player
                private static Dictionary<long, double> previousHungerValues = new Dictionary<long, double>();

                // Dictionary to store the previous update times for each player
                private static Dictionary<long, double> previousUpdateTimes = new Dictionary<long, double>();

                // Method to calculate the current hunger decrease rate based on changing hunger values
                public static double CalculateActualHungerDecreaseRate(IMyPlayer player, MyEntityStat hunger)
                {
                    // Get the current time in minutes
                    double currentTime = (MyAPIGateway.Session.GameplayFrameCounter / 60);

                    // Get player ID
                    long playerId = player.IdentityId;

                    // Check if we have a previous hunger value and time stored for this player
                    if (previousHungerValues.ContainsKey(playerId) && previousUpdateTimes.ContainsKey(playerId))
                    {
                        // Calculate time interval since last update
                        double timeInterval = currentTime - previousUpdateTimes[playerId];

                        // Avoid division by zero if time interval is too small
                        if (timeInterval <= 0.001)
                            return 0;

                        // Calculate the difference in hunger value
                        double hungerDifference = previousHungerValues[playerId] - hunger.Value;

                        // Calculate the hunger decrease rate (hunger difference per minute)
                        double hungerDecreaseRate = (hungerDifference / timeInterval)*100;

                        // Update previous hunger and time values
                        previousHungerValues[playerId] = hunger.Value;
                        previousUpdateTimes[playerId] = currentTime;

                        return hungerDecreaseRate; // Return the calculated rate
                    }
                    else
                    {
                        // Initialize previous values for this player if not present
                        previousHungerValues[playerId] = hunger.Value;
                        previousUpdateTimes[playerId] = currentTime;

                        return 0; // No rate calculated on first update
                    }
                }

                // Method to update hunger and calculate time remaining until zero
                public static void UpdateHungerAndCalculateTimeRemaining(IMyPlayer player, MyEntityStat hunger)
                {
                    // Calculate actual hunger decrease rate
                    double actualHungerDecreaseRate = CalculateActualHungerDecreaseRate(player, hunger);

                    // If the decrease rate is very small or zero, set a default rate to avoid infinite time
                    if (Math.Abs(actualHungerDecreaseRate) < 0.0001)
                    {
                        actualHungerDecreaseRate = 0.1; // Set a default minimum rate
                    }

                    // Calculate time remaining until hunger reaches zero
                    double timeRemaining = hunger.Value / Math.Abs(actualHungerDecreaseRate);

                    // Show message with the time remaining to starvation
                    MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Current Hunger Decrease Rate: {actualHungerDecreaseRate:F2} units/min. Time to Starvation: {timeRemaining:F1} min.");
                }
            }
        }
    }
}
