using System;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using PEPCO.iSurvival.Core;
using PEPCO.iSurvival.Log;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using VRage.Utils;

namespace PEPCO.iSurvival.Chat
{
    public class ChatCommands : IDisposable
    {
        const string MainCommand = "/pepco";
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

                bool isAdmin = thisPlayer.PromoteLevel == MyPromoteLevel.Admin || thisPlayer.PromoteLevel == MyPromoteLevel.Owner;

                if (!isAdmin)
                {
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "You are not an admin!");
                    return;
                }

                // Clean and process the command
                string[] commandParts = messageText.Substring(MainCommand.Length).Trim().Split(' ');

                switch (commandParts[0].ToLower())
                {
                    case "exempt":
                        MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Exempt player ids: {String.Join(",", iSurvivalSessionSettings.playerExceptions)}");
                        return;
                    case "addexemption":
                        Mod.Settings.playerExceptions.Add(thisPlayer.SteamUserId);
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Exempt player ids: {String.Join(",", iSurvivalSessionSettings.playerExceptions)}");
                        return;
                    case "removeexemption":
                        Mod.Settings.playerExceptions.RemoveAll(x => x == thisPlayer.SteamUserId);
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Exempt player ids: {String.Join(",", iSurvivalSessionSettings.playerExceptions)}");
                        return;
                    case "reloadconfig":
                        Mod.LoadData();
                        MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Reloaded configuration from file.");
                        return;
                    case "heal":
                        HandleHealCommand(commandParts, thisPlayer);
                        return;
                    case "help":
                        DisplayAvailableCommands();
                        return;
                    case "liststats":
                        ListPlayerStats(thisPlayer);
                        return;
                    default:
                        UpdateMultiplier(commandParts);
                        return;
                }
            }
            catch (Exception e)
            {
                iSurvivalLog.Error(e);
            }
        }

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

            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Player stat IDs:");
            foreach (var stat in statComponent.Stats)
            {
                MyAPIGateway.Utilities.ShowMessage("stat", $"{stat.ToString()}: {stat.Value}");
            }
        }



        void HandleHealCommand(string[] parts, IMyPlayer player)
        {
            if (parts.Length != 3)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Usage: /pepco heal <stat|all> <percentage>");
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
                MyEntityStat targetStat;
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

        void UpdateMultiplier(string[] commandParts)
        {
            if (commandParts.Length != 2)
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Invalid multiplier command.");
                return;
            }

            string command = commandParts[0].ToLower();
            float value;
            if (!float.TryParse(commandParts[1], out value))
            {
                MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Invalid multiplier value.");
                return;
            }

            switch (command)
            {
                case "staminadrainmultiplier":
                    Mod.Settings.staminadrainmultiplier = value;
                    break;
                case "fatiguedrainmultiplier":
                    Mod.Settings.fatiguedrainmultiplier = value;
                    break;
                case "healthdrainmultiplier":
                    Mod.Settings.healthdrainmultiplier = value;
                    break;
                case "hungerdrainmultiplier":
                    Mod.Settings.hungerdrainmultiplier = value;
                    break;
                case "waterdrainmultiplier":
                    Mod.Settings.waterdrainmultiplier = value;
                    break;
                case "sanitydrainmultiplier":
                    Mod.Settings.sanitydrainmultiplier = value;
                    break;
                case "staminaincreasemultiplier":
                    Mod.Settings.staminaincreasemultiplier = value;
                    break;
                case "fatigueincreasemultiplier":
                    Mod.Settings.fatigueincreasemultiplier = value;
                    break;
                case "healthincreasemultiplier":
                    Mod.Settings.healthincreasemultiplier = value;
                    break;
                case "hungerincreasemultiplier":
                    Mod.Settings.hungerincreasemultiplier = value;
                    break;
                case "waterincreasemultiplier":
                    Mod.Settings.waterincreasemultiplier = value;
                    break;
                case "sanityincreasemultiplier":
                    Mod.Settings.sanityincreasemultiplier = value;
                    break;
                default:
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Unknown multiplier: {command}");
                    return;
            }

            Mod.UpdateSettings();
            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"{command} set to {value}");
        }

        void DisplayAvailableCommands()
        {
            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Available commands:");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} heal <stat|all> <percentage>", "Sets the specified stat or all stats to the given percentage.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} exempt", "Shows the SteamIDs of all exempt players.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} addexemption", "Adds the SteamID of the current user to the list of exempt players.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} removeexemption", "Removes the SteamID of the current user from the list of exempt players.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} reloadconfig", "Reloads the configuration from the file.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} <config_option> <value>", "Sets the specified multiplier to the given value (e.g., staminadrainmultiplier, fatiguedrainmultiplier, etc.).");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} liststats", "Lists all player stat IDs.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} help", "Displays this help message.");
        }

    }
}
