using PEPCO.iSurvival.factors;
using PEPCO.iSurvival.Log;
using PEPCO.iSurvival.stats;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace PEPCO.iSurvival.Effects
{
    public static class Processes
    {
        public static void ProcessPlayer(IMyPlayer player)
        {
            // Skip players in the exception list
            if (PEPCO.iSurvival.settings.iSurvivalSessionSettings.playerExceptions.Contains(player.SteamUserId))
                return;

            var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
            if (statComp == null)
            {
                iSurvivalLog.Error($"Stat Component is null for player: {player.DisplayName}.");
                return;
            }

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
            }
        }

        public static void ResetPlayerStats(IMyPlayer player)
        {
            var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
            if (statComp == null)
            {
                iSurvivalLog.Error($"Failed to reset stats for player: {player.DisplayName}. Stat component is null.");
                return;
            }

            // Reset each stat to default values
            SetStatValue(statComp, "Sanity", 100);
            SetStatValue(statComp, "Calories", 2000f);
            SetStatValue(statComp, "Fat", 70f);
            SetStatValue(statComp, "Cholesterol", 300f);
            SetStatValue(statComp, "Sodium", 1500f);
            SetStatValue(statComp, "Carbohydrates", 275f);
            SetStatValue(statComp, "Protein", 50f);
            SetStatValue(statComp, "Vitamins", 100f);
            SetStatValue(statComp, "Hunger", 100f);
            SetStatValue(statComp, "Water", 100f);
            SetStatValue(statComp, "Fatigue", 100f);
            SetStatValue(statComp, "Stamina", 100f);

            iSurvivalLog.Info($"Reset stats for player: {player.DisplayName}.");
        }

        public static void SetStatValue(MyEntityStatComponent statComp, string statName, float value)
        {
            MyEntityStat stat;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute(statName), out stat))
            {
                stat.Value = value; // Set the stat directly to the desired value
                iSurvivalLog.Info($"Set {statName} to {value} for entity {statComp.Entity?.DisplayName ?? "Unknown Entity"}.");
            }
            else
            {
                iSurvivalLog.Error($"Failed to find stat {statName} in component for entity {statComp.Entity?.DisplayName ?? "Unknown Entity"}.");
            }
        }

        public static class Metabolism
        {
            private static float CalculateMetabolicRate(IMyPlayer player)
            {
                float baseMetabolicRate = 1.0f;

                // Modify based on environmental conditions
                baseMetabolicRate *= EnvironmentalFactors.GetEnvironmentalFactor(player);

                // Further modifications based on other factors (to be added)
                // e.g., activity level, health status, etc.

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

                // Calculate the hunger decrease rate based on activity or environmental factors
                double hungerDecreaseRate = CalculateHungerDecreaseRate(calories, fat, carbohydrates, protein, vitamins);

                // Calculate time remaining until hunger reaches zero
                double timeRemaining = hunger.Value / hungerDecreaseRate;

                // Show message with the time remaining to starvation
                MyAPIGateway.Utilities.ShowMessage("iSurvival", $"Time to Starvation: {timeRemaining:F1} min");

                // Set hunger value directly based on the average percentage
                Core.iSurvivalSession.ApplyStatChange(hunger, 1, averagePercentage - hunger.Value);
            }

            private static double CalculateHungerDecreaseRate(MyEntityStat calories, MyEntityStat fat, MyEntityStat carbohydrates, MyEntityStat protein, MyEntityStat vitamins)
            {
                // Example base decrease rate per minute (adjust as needed)
                double baseDecreaseRate = 1; // Adjust based on your game's mechanics

                // Additional decrease based on calorie deficit
                double calorieDeficitFactor = (calories.MaxValue - calories.Value) / calories.MaxValue;
                double calorieDecreaseRate = baseDecreaseRate * calorieDeficitFactor;

                // Additional decrease based on other factors like fat and protein deficiency
                double fatDeficitFactor = (fat.MaxValue - fat.Value) / fat.MaxValue;
                double proteinDeficitFactor = (protein.MaxValue - protein.Value) / protein.MaxValue;

                // Calculate total decrease rate (you can add more logic here)
                double totalDecreaseRate = baseDecreaseRate + calorieDecreaseRate + (fatDeficitFactor + proteinDeficitFactor) * 0.2;

                // Return total hunger decrease rate per minute
                return totalDecreaseRate;
            }
        }
    }
}
