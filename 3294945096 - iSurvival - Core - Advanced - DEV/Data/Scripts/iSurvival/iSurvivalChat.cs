using System;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using PEPCO.iSurvival.Log;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using VRage.Utils;
using PEPCO.iSurvival.settings;
using PEPCO.iSurvival.stats;

namespace PEPCO.iSurvival.Chat
{
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
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
        }

        void MessageEntered(string messageText, ref bool sendToOthers)
        {
            try
            {
                if (!messageText.StartsWith(MainCommand))
                    return;

                sendToOthers = false;
                var thisPlayer = MyAPIGateway.Session.LocalHumanPlayer;

                if (!IsAdmin(thisPlayer))
                {
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "You are not an admin!");
                    return;
                }

                // Clean and process the command
                string[] commandParts = messageText.Substring(MainCommand.Length).Trim().Split(' ');

                // Process the command based on the first keyword
                switch (commandParts[0].ToLower())
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
                    default:
                        UpdateStat(commandParts);
                        break;
                }
            }
            catch (Exception e)
            {
                iSurvivalLog.Error(e);
            }
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

        // Handle the "heal" command
        void HandleHealCommand(string[] parts, IMyPlayer player)
        {
            if (parts.Length != 3)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Usage: /isurvival heal <stat|all> <percentage>");
                return;
            }

            string stat = parts[1];
            float percentage;
            if (!float.TryParse(parts[2], out percentage) || percentage < 0 || percentage > 100)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Invalid percentage. Please specify a value between 0 and 100.");
                return;
            }

            ApplyHealing(player, stat, percentage);
        }

        // Apply healing to the player's stats
        void ApplyHealing(IMyPlayer player, string stat, float percentage)
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

            if (stat == "all")
            {
                foreach (var entityStat in statComponent.Stats)
                {
                    entityStat.Value = entityStat.MaxValue * (percentage / 100f);
                }
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"All stats set to {percentage}%.");
            }
            else
            {
                MyEntityStat targetStat; // Declare the variable separately for C# 6.0
                if (statComponent.TryGetStat(MyStringHash.GetOrCompute(stat), out targetStat))
                {
                    targetStat.Value = targetStat.MaxValue * (percentage / 100f);
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{stat} set to {percentage}%.");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Stat {stat} not found.");
                }
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
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} config: Base: {statSetting.Base} * Multiplier: {statSetting.Multiplier} = Value: {statSetting.Value}");
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
                case "value":
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{statName} value: {statSetting.Value}");
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
                case "value":
                    statSetting.Value = value; // Directly set the value
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
