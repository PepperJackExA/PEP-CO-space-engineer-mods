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

namespace PEPCO.iSurvival.Effects
{
    public static class Processes
    {
        public static void ProcessPlayer(IMyPlayer player, MyEntityStatComponent statComp)
        {
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


            // Check if player is in a cryo block
            if (Blocks.ProcessBlockstEffects(player, stats))
            {
                return; // Stop processing if the player is in a cryo block
            }

            // Initialize the player's RPG stats dictionary
            Dictionary<string, MyEntityStat> playerRPGStats = new Dictionary<string, MyEntityStat>();
            
            playerRPGStats["Strength"] = stats["Strength"];
            playerRPGStats["Dexterity"] = stats["Dexterity"];
            playerRPGStats["Constitution"] = stats["Constitution"];
            playerRPGStats["Wisdom"] = stats["Wisdom"];
            playerRPGStats["Intelligence"] = stats["Intelligence"];
            playerRPGStats["Charisma"] = stats["Charisma"];

            // Initialize or retrieve the player's RPG leveling system
            long playerId = player.IdentityId;

            if (!Core.iSurvivalSession.playerRPGSystems.ContainsKey(playerId))
            {
                Core.iSurvivalSession.playerRPGSystems[playerId] = new RPGLeveling(playerRPGStats);
            }

            var rpgLeveling = Core.iSurvivalSession.playerRPGSystems[playerId];

            // Add XP to the player (example value of 10 XP per cycle, customize this)
            //rpgLeveling.AddXP(10f);

            // Process other stats like fatigue, hunger, etc.
            if (fatigue != null)
            {
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
                var starvationCounter = Core.iSurvivalSession.starvationMessageCooldown; // Cooldown timer (in frames)
                if (hunger.Value < 20 && Core.iSurvivalSession.rand.Next((int)fatigue.Value) > 0)
                {
                    Core.iSurvivalSession.starvationMessageCooldown++; // Increase the counter of the display cooldown.
                    float damageAmount = (1 - (hunger.Value / 20)); // Adjust the multiplier as needed
                    health.Decrease(damageAmount, null);

                    if (starvationCounter >= 2) // When the cooldown counter gets to 5 seconds
                    {
                        MyAPIGateway.Utilities.ShowNotification($"You are starving!", 2000, MyFontEnum.Red);
                        Core.iSurvivalSession.starvationMessageCooldown = 0; // Reset the timer
                    }
                }

                // Apply other processes
                Metabolism.ApplyMetabolismEffect(player, stats);
                FatigueAndStamina.ProcessFatigue(player, stats);
                Sanity.ProcessSanity(player, stats);
                Movement.ProcessMovementEffects(player, stats);
            }
        }

        public static class Blocks
        {
            public static bool ProcessBlockstEffects(IMyPlayer player, Dictionary<string, MyEntityStat> stats, string blockName = null)
            {
                var block = player.Controller?.ControlledEntity?.Entity as IMyCubeBlock;
                if (block == null) return false; // Player not in a block

                var fatigue = stats["Fatigue"];
                var sanity = stats["Sanity"];
                var hunger = stats["Hunger"];
                var calories = stats["Calories"];
                var water = stats["Water"];
                var stamina = stats["Stamina"];
                var strength = stats["Strength"];
                var dexterity = stats["Dexterity"];
                var constitution = stats["Constitution"];

                float averageStats = (fatigue.Value + sanity.Value + hunger.Value + water.Value) / 4;

                var blockDef = block.BlockDefinition.SubtypeId.ToString();
                var blockDisplayName = block.DisplayNameText; // Get block's display name

                // Check if blockName matches the display name of the block (if blockName is provided)
                if (!string.IsNullOrEmpty(blockName) && blockDisplayName != blockName)
                {
                    return false; // Block name doesn't match
                }

                if (blockDef.Contains("Cryo"))
                {
                    return true; // Player is in a cryo block, stop processing
                }

                // Process effects for other block types
                if (blockDef.Contains("Bed"))
                {
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, 2);
                    Core.iSurvivalSession.ApplyStatChange(player,fatigue, 1, 2);
                    if (sanity.Value < averageStats)
                    {
                        Core.iSurvivalSession.ApplyStatChange(player,sanity, 1, 0.25);
                    }
                    return true;
                }
                else if (blockDef.Contains("Toilet") || blockDef.Contains("Bathroom"))
                {
                    Core.iSurvivalSession.ApplyStatChange(player, fatigue, 1, 1);
                    Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, 2);
                    if (hunger.Value > 20 && calories.Value > 1)
                    {
                        Core.iSurvivalSession.ApplyStatChange(player,sanity, 1, 0.5);
                        Core.iSurvivalSession.ApplyStatChange(player,calories, 1, -10);
                        player.Character.GetInventory(0).AddItems((MyFixedPoint)(((100 - averageStats) / 100)), (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                    }
                }
                if (fatigue.Value < 25) Core.iSurvivalSession.ApplyStatChange(player,fatigue, 1, 0.25);
                if (stamina.Value < 25) Core.iSurvivalSession.ApplyStatChange(player,stamina, 1, 1);
                if (strength.Value > 10) Core.iSurvivalSession.ApplyStatChange(player,strength, 1, -0.001f);
                if (dexterity.Value > 10) Core.iSurvivalSession.ApplyStatChange(player,dexterity, 1, -0.001f);
                if (constitution.Value > 10) Core.iSurvivalSession.ApplyStatChange(player,constitution, 1, -0.001f);


                return false; // Player is not in a cryo block or named block
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

                float sanityChangeRate = CalculateSanityChangeRate(player, stats);
                Core.iSurvivalSession.ApplyStatChange(player, sanity, 1, sanityChangeRate / 60f);
            }

            private static float CalculateSanityChangeRate(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                float sanityChangeRate = 0f;

                var fatigue = stats["Fatigue"];
                var wisdom = stats.ContainsKey("Wisdom") ? stats["Wisdom"] : null;
                var charisma = stats.ContainsKey("Charisma") ? stats["Charisma"] : null;

                // Environmental factors
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

                return sanityChangeRate;
            }

            private static float GetWisdomImpactOnSanity(MyEntityStat wisdom)
            {
                if (wisdom == null) return 0f;
                float wisdomLevel = wisdom.Value / wisdom.MaxValue;
                // Wisdom provides mental resilience, so higher wisdom reduces sanity loss
                return wisdomLevel > 0.5f ? wisdomLevel * 1.2f : wisdomLevel * 0.5f;
            }

            private static float GetCharismaImpactOnSanity(MyEntityStat charisma)
            {
                if (charisma == null) return 0f;
                float charismaLevel = charisma.Value / charisma.MaxValue;
                // Charisma helps in social resilience, slightly affecting sanity
                return charismaLevel > 0.5f ? charismaLevel * 0.8f : charismaLevel * 0.4f;
            }

            private static float GetFatigueImpactOnSanity(MyEntityStat fatigue)
            {
                if (fatigue == null) return 0f;

                float fatigueLevel = fatigue.Value / fatigue.MaxValue;
                // More nuanced impact based on multiple fatigue ranges
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
                var fatigue = stats["Fatigue"];
                var stamina = stats["Stamina"];
                if (fatigue == null || stamina == null) return;

                // Calculate fatigue change rate (based on nutrition, RPG stats like Constitution, environment)
                float fatigueChangeRate = CalculateFatigueChangeRate(player, stats);

                // Apply the fatigue change over time
                Core.iSurvivalSession.ApplyStatChange(player, fatigue, 1, fatigueChangeRate / 60f);

                // Adjust stamina using RPG stats like Strength and Dexterity, environmental factors
                ProcessStamina(player, stats);
            }

            public static float CalculateFatigueChangeRate(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                float changeRate = 0f;

                // Check nutritional stats (Calories, Protein, etc.) and adjust fatigue based on these
                if (stats.ContainsKey("Calories"))
                {
                    var caloriesStat = stats["Calories"];
                    changeRate += GetFatigueChangeRateFromNutrition(caloriesStat);
                }

                if (stats.ContainsKey("Protein"))
                {
                    var proteinStat = stats["Protein"];
                    changeRate += GetFatigueChangeRateFromNutrition(proteinStat);
                }

                // Apply Constitution's impact on fatigue
                var statComp = player.Character?.Components?.Get<MyEntityStatComponent>();
                MyEntityStat constitution;
                if (statComp != null && statComp.TryGetStat(MyStringHash.GetOrCompute("Constitution"), out constitution))
                {
                    float constitutionEffect = CalculateRPGStatEffect(constitution.Value, 10f, 0.75f); // Max 15% effect
                    changeRate *= (1.0f + constitutionEffect); // Constitution reduces/increases fatigue change rate
                }

                // Apply environmental and movement factors
                changeRate += EnvironmentalFactors.GetEnvironmentalFactor(player);  // Example: Storm increases fatigue

                return changeRate;
            }

            private static float GetFatigueChangeRateFromNutrition(MyEntityStat nutritionStat)
            {
                if (nutritionStat == null) return 0f;
                float nutritionPercentage = nutritionStat.Value / nutritionStat.MaxValue;

                // Fatigue drains faster when nutritional values are low
                return nutritionPercentage < 0.2f ? -1f : (nutritionPercentage < 0.4f ? -0.75f : -0.5f);
            }

            // Adjust stamina change rate based on RPG stats (Strength, Dexterity), environmental factors
            public static void ProcessStamina(IMyPlayer player, Dictionary<string, MyEntityStat> stats)
            {
                var stamina = stats["Stamina"];
                if (stamina == null) return;

                // Default stamina change rate
                float staminaChangeRate = 0f;

                // Apply RPG stat effects (Strength for stamina drain, Dexterity for stamina recovery)
                var statComp = player.Character?.Components?.Get<MyEntityStatComponent>();
                MyEntityStat strength, dexterity;
                if (statComp != null)
                {
                    // Strength reduces stamina drain during physical activities
                    statComp.TryGetStat(MyStringHash.GetOrCompute("Strength"), out strength);
                    if (strength != null)
                    {
                        float strengthEffect = CalculateRPGStatEffect(strength.Value, 10f, 0.75f); // Max 20% effect
                        staminaChangeRate += strengthEffect * 5f; // Apply to reduce stamina drain
                    }

                    // Dexterity improves stamina recovery when moving efficiently
                    statComp.TryGetStat(MyStringHash.GetOrCompute("Dexterity"), out dexterity);
                    if (dexterity != null)
                    {
                        float dexterityEffect = CalculateRPGStatEffect(dexterity.Value, 10f, 0.75f); // Max 10% effect
                        staminaChangeRate += dexterityEffect * 3f; // Apply to increase stamina recovery
                    }
                }

                // Environmental factors such as weather or terrain affect stamina recovery/drain
                staminaChangeRate += EnvironmentalFactors.GetEnvironmentalFactor(player);

                // Apply the stamina change rate
                Core.iSurvivalSession.ApplyStatChange(player, stamina, 1, staminaChangeRate);
            }

            // Calculate the effect of an RPG stat
            private static float CalculateRPGStatEffect(float currentStatValue, float baseStat, float maxEffect)
            {
                float normalizedStat = MathHelper.Clamp(currentStatValue, 1f, 20f); // Ensure stat is between 1 and 20
                float statDifference = normalizedStat - baseStat;
                return MathHelper.Clamp(statDifference / 10f, -maxEffect, maxEffect); // Scale based on difference from baseStat
            }

            // Function to display HUD messages only when a significant change happens
            public static void ShowEffectMessage(IMyPlayer player, string statName, float statValue, float effect)
            {
                float percentageEffect = Math.Abs(effect); // Get absolute effect value

                // Show the message only if the effect is significant (greater than 1%)
                if (percentageEffect > 0.01f)
                {
                    string impactType = effect >= 0 ? "bonus" : "penalty";
                    string message = $"{statName} ({statValue:F1}): {impactType} of {percentageEffect:F1}%";

                    // Show the message on HUD
                    MyAPIGateway.Utilities.ShowMessage("RPG Stat", message);

                    // Optionally log the message as well
                    iSurvivalLog.Info($"{player.DisplayName}: {message}");
                }
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
                float fatigueMultiplier = CalculateFatigueEffect(fatigue);

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
