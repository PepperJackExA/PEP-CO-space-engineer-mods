using System;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using PEPCO.iSurvival.Log;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using VRage.Utils;
using PEPCO.iSurvival.settings;
using PEPCO.iSurvival.stats;
using System.Collections.Generic;
using System.Net;

namespace PEPCO.iSurvival.Chat
{
    public class CommandSetting
    {
        public bool IsEnabled { get; set; } // Is the command enabled
        public bool AdminOnly { get; set; } // Is the command restricted to admins only
        public bool ServerOnly { get; set; } // Is the command restricted to server execution only

        public CommandSetting(bool isEnabled, bool adminOnly, bool serverOnly)
        {
            IsEnabled = isEnabled;
            AdminOnly = adminOnly;
            ServerOnly = serverOnly;
        }
        public static Dictionary<string, CommandSetting> Chat = new Dictionary<string, CommandSetting>
{
    { "exempt", new CommandSetting(true, true, false) }, // Admin only, not restricted to server
    { "addexemption", new CommandSetting(true, true, true) }, // Admin only, server only
    { "removeexemption", new CommandSetting(true, true, true) }, // Admin only, server only
    { "reloadconfig", new CommandSetting(true, true, true) }, // Admin only, server only
    { "heal", new CommandSetting(true, true, true) }, // Admin only, server only
    { "help", new CommandSetting(true, false, false) }, // Everyone can use, not server restricted
    { "liststats", new CommandSetting(true, false, false) }, // Everyone can use, not server restricted
    { "showhunger", new CommandSetting(true, false, false) }, // Admin only, server only
};
    }


    public class ChatCommands : IDisposable
    {
        const string MainCommand = "/isurvival";
        readonly iSurvivalSessionSettings Mod;

        public ChatCommands(iSurvivalSessionSettings mod)
        {
            Mod = mod;
            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
        }

        public void Dispose()
        {
            if (MyAPIGateway.Utilities != null)
            {
                MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
            }
        }


        void MessageEntered(string messageText, ref bool sendToOthers)
        {
            try
            {
                if (!messageText.StartsWith(MainCommand))
                    return;

                sendToOthers = false;
                var thisPlayer = MyAPIGateway.Session.LocalHumanPlayer;
                var command = messageText.Substring(MainCommand.Length).Trim().Split(' ')[0].ToLower();

                // Check if command is in the settings dictionary
                if (!CommandSetting.Chat.ContainsKey(command))
                {
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Unknown command: {command}");
                    return;
                }

                var setting = CommandSetting.Chat[command];

                // Check if the command is enabled
                if (!setting.IsEnabled)
                {
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{command} command is currently disabled.");
                    return;
                }

                // Check if admin-only command and player is not admin
                if (setting.AdminOnly && !IsAdmin(thisPlayer))
                {
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "You are not an admin!");
                    return;
                }

                // Check if server-only command and not running on the server
                if (setting.ServerOnly && !MyAPIGateway.Multiplayer.IsServer)
                {
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{command} can only be executed in single player games.");
                    return;
                }

                // Process the command based on the first keyword
                string[] commandParts = messageText.Substring(MainCommand.Length).Trim().Split(' ');
                switch (command)
                {
                    case "exempt":
                        ShowPlayerExemptions();
                        break;
                    case "addexemption":
                        AddPlayerExemption(thisPlayer.SteamUserId);
                        break;
                    case "removeexemption":
                        RemovePlayerExemption(thisPlayer.SteamUserId);
                        break;
                    case "reloadconfig":
                        ReloadConfig();
                        break;
                    case "heal":
                        HandleHealCommand(commandParts, thisPlayer);
                        break;
                    case "help":
                        DisplayAvailableCommands();
                        break;
                    case "liststats":
                        ListPlayerStats(thisPlayer);
                        break;
                    case "showhunger":
                        ShowHunger(thisPlayer);
                        break;
                    default:
                        //UpdateStat(commandParts);
                        MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Unknown command: {command}");
                        break;
                }
            }
            catch (Exception e)
            {
                iSurvivalLog.Error(e);
            }
        }

        private void ShowHunger(IMyPlayer player)
        {
            var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
            
                MyEntityStat calories, fat, cholesterol, sodium, fiber, sugar, starches, protein, vitamins;

                // Retrieve each Food stat from the component
                statComp.TryGetStat(MyStringHash.GetOrCompute("Calories"), out calories);
                statComp.TryGetStat(MyStringHash.GetOrCompute("Fat"), out fat);
                statComp.TryGetStat(MyStringHash.GetOrCompute("Cholesterol"), out cholesterol);
                statComp.TryGetStat(MyStringHash.GetOrCompute("Sodium"), out sodium);
                statComp.TryGetStat(MyStringHash.GetOrCompute("Fiber"), out fiber);
                statComp.TryGetStat(MyStringHash.GetOrCompute("Sugar"), out sugar);
                statComp.TryGetStat(MyStringHash.GetOrCompute("Starches"), out starches);
                statComp.TryGetStat(MyStringHash.GetOrCompute("Protein"), out protein);
                statComp.TryGetStat(MyStringHash.GetOrCompute("Vitamins"), out vitamins);
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"ShowHunger Test");

                //float fatigueChangeRate = Effects.Processes.FatigueAndStamina.CalculateFatigueChangeRate(calories, fat, cholesterol, sodium, fiber, sugar, starches, protein, vitamins);               
            
            //MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Hunger Rate: {fatigueChangeRate}");
        }

        // Check if the player is an admin
        private bool IsAdmin(IMyPlayer player)
        {
            return player.PromoteLevel == MyPromoteLevel.Admin || player.PromoteLevel == MyPromoteLevel.Owner;
        }

        // Show the list of exempt players
        void ShowPlayerExemptions()
        {
            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Exempt player IDs: {string.Join(",", iSurvivalSessionSettings.playerExceptions)}");
        }

        // Add the player to the exemption list
        void AddPlayerExemption(ulong steamId)
        {
            if (!iSurvivalSessionSettings.playerExceptions.Contains(steamId))
            {
                Mod.Settings.playerExceptions.Add(steamId);
                Mod.UpdateSettings();
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Player {steamId} added to exemptions.");
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Player {steamId} is already exempt.");
            }
        }

        // Remove the player from the exemption list
        void RemovePlayerExemption(ulong steamId)
        {
            if (iSurvivalSessionSettings.playerExceptions.Contains(steamId))
            {
                Mod.Settings.playerExceptions.Remove(steamId);
                Mod.UpdateSettings();
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Player {steamId} removed from exemptions.");
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Player {steamId} is not in the exemption list.");
            }
        }

        // Reload the configuration
        void ReloadConfig()
        {
            Mod.LoadData();
            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Configuration reloaded.");
        }

        // List the player's stats
        void ListPlayerStats(IMyPlayer player)
        {
            var character = player.Character;
            if (character == null)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Player character not found.");
                return;
            }

            var statComponent = character.Components.Get<MyEntityStatComponent>();
            if (statComponent == null)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Stat component not found.");
                return;
            }

            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Player stats:");
            foreach (var stat in statComponent.Stats)
            {
                MyAPIGateway.Utilities.ShowMessage("Stat", $"{stat.ToString()}: {stat.Value}");
            }
        }

        // Inside your existing Heal Command
        void HandleHealCommand(string[] parts, IMyPlayer player)
        {
            if (parts.Length != 3)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Usage: /isurvival heal <stat|all|hunger|rpg> <percentage>");
                return;
            }

            string stat = parts[1].ToLower();
            float percentage;
            if (!float.TryParse(parts[2], out percentage) || percentage < 0 || percentage > 100)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Invalid percentage. Please specify a value between 0 and 100.");
                return;
            }

            switch (stat)
            {
                case "all":
                    ApplyHealingToAll(player, percentage);
                    break;
                case "hunger":
                    HealHungerStats(player, percentage);
                    break;
                case "rpg":
                    HealRPGStats(player, percentage);
                    break;
                case "stamina":
                    HealStat(player, "Stamina", percentage);
                    break;
                case "sanity":
                    HealStat(player, "Sanity", percentage);
                    break;
                case "fatigue":
                    HealStat(player, "Fatigue", percentage);
                    break;
                case "water":
                    HealStat(player, "Water", percentage);
                    break;
                default:
                    ApplyHealing(player, stat, percentage);
                    break;
            }
        }
        // Heals a specific stat like Stamina, Sanity, Fatigue
        void HealStat(IMyPlayer player, string statName, float percentage)
        {
            var statComp = player.Character.Components.Get<MyEntityStatComponent>();
            MyEntityStat targetStat;

            // Check if the stat exists in the player's stat component
            if (statComp.TryGetStat(MyStringHash.GetOrCompute(statName), out targetStat) && targetStat != null)
            {
                targetStat.Value = targetStat.MaxValue * (percentage / 100f);
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} set to {percentage}%.");
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Stat {statName} not found.");
            }
        }
        // Apply healing to all stats
        void ApplyHealingToAll(IMyPlayer player, float percentage)
        {
            var statComp = player.Character.Components.Get<MyEntityStatComponent>();
            foreach (var stat in statComp.Stats)
            {
                if (stat.MaxValue > 0)
                {
                    stat.Value = stat.MaxValue * (percentage / 100f);
                }
            }
            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"All stats set to {percentage}%.");
        }

        // Heal Hunger (Food Stats)
        void HealHungerStats(IMyPlayer player, float percentage)
        {
            var statComp = player.Character.Components.Get<MyEntityStatComponent>();

            MyEntityStat calories, fat, cholesterol, sodium, fiber, sugar, starches, protein, vitamins;

            // Get all the relevant food stats
            statComp.TryGetStat(MyStringHash.GetOrCompute("Calories"), out calories);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Fat"), out fat);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Cholesterol"), out cholesterol);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Sodium"), out sodium);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Fiber"), out fiber);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Sugar"), out sugar);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Starches"), out starches);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Protein"), out protein);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Vitamins"), out vitamins);

            // Heal each food stat by setting its value based on percentage
            if (calories != null) calories.Value = calories.MaxValue * (percentage / 100f);
            if (fat != null) fat.Value = fat.MaxValue * (percentage / 100f);
            if (cholesterol != null) cholesterol.Value = cholesterol.MaxValue * (percentage / 100f);
            if (sodium != null) sodium.Value = sodium.MaxValue * (percentage / 100f);
            if (fiber != null) fiber.Value = fiber.MaxValue * (percentage / 100f);
            if (sugar != null) sugar.Value = sugar.MaxValue * (percentage / 100f);
            if (starches != null) starches.Value = starches.MaxValue * (percentage / 100f);
            if (protein != null) protein.Value = protein.MaxValue * (percentage / 100f);
            if (vitamins != null) vitamins.Value = vitamins.MaxValue * (percentage / 100f);

            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Hunger-related stats healed to {percentage}%.");
        }
        // Heal RPG stats
        void HealRPGStats(IMyPlayer player, float percentage)
        {
            var statComp = player.Character.Components.Get<MyEntityStatComponent>();

            MyEntityStat strength, dexterity, constitution, intelligence, wisdom, charisma;

            // Get all the relevant RPG stats
            statComp.TryGetStat(MyStringHash.GetOrCompute("Strength"), out strength);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Dexterity"), out dexterity);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Constitution"), out constitution);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Intelligence"), out intelligence);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Wisdom"), out wisdom);
            statComp.TryGetStat(MyStringHash.GetOrCompute("Charisma"), out charisma);

            // Heal each RPG stat by setting its value based on percentage
            if (strength != null) strength.Value = strength.MaxValue * (percentage / 100f);
            if (dexterity != null) dexterity.Value = dexterity.MaxValue * (percentage / 100f);
            if (constitution != null) constitution.Value = constitution.MaxValue * (percentage / 100f);
            if (intelligence != null) intelligence.Value = intelligence.MaxValue * (percentage / 100f);
            if (wisdom != null) wisdom.Value = wisdom.MaxValue * (percentage / 100f);
            if (charisma != null) charisma.Value = charisma.MaxValue * (percentage / 100f);

            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"RPG stats healed to {percentage}%.");
        }

        // Apply healing to a single stat (for other specific stats)
        void ApplyHealing(IMyPlayer player, string statName, float percentage)
        {
            var statComp = player.Character.Components.Get<MyEntityStatComponent>();
            MyEntityStat targetStat;
            if (statComp.TryGetStat(MyStringHash.GetOrCompute(statName), out targetStat) && targetStat != null)
            {
                targetStat.Value = targetStat.MaxValue * (percentage / 100f);
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} set to {percentage}%.");
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Stat {statName} not found.");
            }
        }


        // Update the specified stat value
        void UpdateStat(string[] commandParts)
        {
            if (commandParts.Length < 2 || commandParts.Length > 3)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Invalid command format. Use: /isurvival <stat> <property> [value]");
                return;
            }

            string statName = commandParts[0].ToLower();

            // Check if the stat exists in the dictionary
            StatSetting statSetting;
            if (!StatManager._statSettings.TryGetValue(statName, out statSetting))
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Unknown stat: {statName}");
                return;
            }

            if (commandParts.Length == 1)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} config: Base: {statSetting.Base} * Multiplier: {statSetting.Multiplier}");
                return;
            }

            string propertyName = commandParts[1].ToLower();
            if (commandParts.Length == 2)
            {
                ShowStatProperty(statSetting, statName, propertyName);
                return;
            }

            float value;
            if (commandParts.Length == 3 && float.TryParse(commandParts[2], out value))
            {
                SetStatProperty(statSetting, statName, propertyName, value);
                Mod.UpdateSettings(); // Save the updated settings
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Invalid value. Please enter a valid number.");
            }
        }

        // Show a specific stat property value
        void ShowStatProperty(StatSetting statSetting, string statName, string propertyName)
        {
            switch (propertyName)
            {
                case "base":
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} base: {statSetting.Base}");
                    break;
                case "multiplier":
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} multiplier: {statSetting.Multiplier}");
                    break;
                default:
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Unknown property: {propertyName}. Use 'base', 'multiplier', or 'value'.");
                    break;
            }
        }

        // Set a specific stat property value
        void SetStatProperty(StatSetting statSetting, string statName, string propertyName, float value)
        {
            switch (propertyName)
            {
                case "base":
                    statSetting.Base = value;
                    statSetting.UpdateValue(); // Recalculate the value
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} base set to {value}");
                    break;
                case "multiplier":
                    statSetting.Multiplier = value;
                    statSetting.UpdateValue(); // Recalculate the value
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} multiplier set to {value}");
                    break;
                case "increaseMultiplier":
                    statSetting.IncreaseMultiplier = value; // Directly set the value
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} value set to {value}");
                    break;
                case "decreaseMultiplier":
                    statSetting.DecreaseMultiplier = value; // Directly set the value
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} value set to {value}");
                    break;
                default:
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Unknown property: {propertyName}. Use 'base', 'multiplier', or 'value'.");
                    break;
            }
        }

        // Display available chat commands
        void DisplayAvailableCommands()
        {
            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Available commands:");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} heal <stat|all> <percentage>", "Sets the specified stat or all stats to the given percentage.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} exempt", "Shows the SteamIDs of all exempt players.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} addexemption", "Adds the SteamID of the current user to the list of exempt players.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} removeexemption", "Removes the SteamID of the current user from the list of exempt players.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} reloadconfig", "Reloads the configuration from the file.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} <stat> <property> <value>", "Sets the specified property of a stat (e.g., stamina base 10, hunger multiplier 1.5).");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} liststats", "Lists all player stat IDs.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} help", "Displays this help message.");
        }
    }
}
