using System;
using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using IMyProgrammableBlock = Sandbox.ModAPI.Ingame.IMyProgrammableBlock;
using PEPCO.iSurvival.Core;
using VRage.Game.ModAPI;
using PEPCO.iSurvival.Log;

namespace PEPCO.iSurvival.Chat
{
    public class ChatCommands : IDisposable
    {
        const string MainCommand = "/iSurvival";
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
                    MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, "BRAAAAA! You no admin!");
                    return;
                }

                // Clean and process the command
                string cleanCommand = messageText.Replace(MainCommand, "").ToLower().Replace(" ", "");

                if (cleanCommand.StartsWith("exempt"))
                {
                    MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Exempt player ids: {String.Join(",", iSurvivalSessionSettings.playerExceptions)}");
                    return;
                }
                else if (cleanCommand.StartsWith("addexemption"))
                {
                    Mod.Settings.playerExceptions.Add(thisPlayer.SteamUserId);
                    Mod.UpdateSettings();
                    MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Exempt player ids: {String.Join(",", iSurvivalSessionSettings.playerExceptions)}");
                    return;
                }
                else if (cleanCommand.StartsWith("removeexemption"))
                {
                    Mod.Settings.playerExceptions.RemoveAll(x => x == thisPlayer.SteamUserId);
                    Mod.UpdateSettings();
                    MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Exempt player ids: {String.Join(",", iSurvivalSessionSettings.playerExceptions)}");
                    return;
                }
                else if (cleanCommand.StartsWith("reloadconfig"))
                {
                    Mod.LoadData();
                    MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, "Reloaded configuration from file.");
                    return;
                }
                else if (cleanCommand.StartsWith("staminadrainmultiplier"))
                {
                    string temp = cleanCommand.Replace("staminadrainmultiplier", "");
                    float staminadrainmultiplier;
                    if (float.TryParse(temp, out staminadrainmultiplier))
                    {
                        Mod.Settings.staminadrainmultiplier = staminadrainmultiplier;
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"New staminadrainmultiplier: {iSurvivalSessionSettings.staminadrainmultiplier}");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Not a valid command: {messageText}");
                    }
                    return;
                }
                else if (cleanCommand.StartsWith("fatiguedrainmultiplier"))
                {
                    string temp = cleanCommand.Replace("fatiguedrainmultiplier", "");
                    float value;
                    if (float.TryParse(temp, out value))
                    {
                        Mod.Settings.fatiguedrainmultiplier = value;
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"New fatiguedrainmultiplier: {iSurvivalSessionSettings.fatiguedrainmultiplier}");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Not a valid command: {messageText}");
                    }
                    return;
                }
                else if (cleanCommand.StartsWith("healthdrainmultiplier"))
                {
                    string temp = cleanCommand.Replace("healthdrainmultiplier", "");
                    float value;
                    if (float.TryParse(temp, out value))
                    {
                        Mod.Settings.healthdrainmultiplier = value;
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"New healthdrainmultiplier: {iSurvivalSessionSettings.healthdrainmultiplier}");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Not a valid command: {messageText}");
                    }
                    return;
                }
                else if (cleanCommand.StartsWith("hungerdrainmultiplier"))
                {
                    string temp = cleanCommand.Replace("hungerdrainmultiplier", "");
                    float value;
                    if (float.TryParse(temp, out value))
                    {
                        Mod.Settings.hungerdrainmultiplier = value;
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"New hungerdrainmultiplier: {iSurvivalSessionSettings.hungerdrainmultiplier}");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Not a valid command: {messageText}");
                    }
                    return;
                }
                else if (cleanCommand.StartsWith("staminaincreasemultiplier"))
                {
                    string temp = cleanCommand.Replace("staminaincreasemultiplier", "");
                    float value;
                    if (float.TryParse(temp, out value))
                    {
                        Mod.Settings.staminaincreasemultiplier = value;
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"New staminaincreasemultiplier: {iSurvivalSessionSettings.staminaincreasemultiplier}");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Not a valid command: {messageText}");
                    }
                    return;
                }
                else if (cleanCommand.StartsWith("fatigueincreasemultiplier"))
                {
                    string temp = cleanCommand.Replace("fatigueincreasemultiplier", "");
                    float value;
                    if (float.TryParse(temp, out value))
                    {
                        Mod.Settings.fatigueincreasemultiplier = value;
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"New fatigueincreasemultiplier: {iSurvivalSessionSettings.fatigueincreasemultiplier}");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Not a valid command: {messageText}");
                    }
                    return;
                }
                else if (cleanCommand.StartsWith("healthincreasemultiplier"))
                {
                    string temp = cleanCommand.Replace("healthincreasemultiplier", "");
                    float value;
                    if (float.TryParse(temp, out value))
                    {
                        Mod.Settings.healthincreasemultiplier = value;
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"New healthincreasemultiplier: {iSurvivalSessionSettings.healthincreasemultiplier}");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Not a valid command: {messageText}");
                    }
                    return;
                }
                else if (cleanCommand.StartsWith("hungerincreasemultiplier"))
                {
                    string temp = cleanCommand.Replace("hungerincreasemultiplier", "");
                    float value;
                    if (float.TryParse(temp, out value))
                    {
                        Mod.Settings.hungerincreasemultiplier = value;
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"New hungerincreasemultiplier: {iSurvivalSessionSettings.hungerincreasemultiplier}");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, $"Not a valid command: {messageText}");
                    }
                    return;
                }

                MyAPIGateway.Utilities.ShowMessage(PEPCO.iSurvival.Log.iSurvivalLog.ModName, "Available commands:");
                MyAPIGateway.Utilities.ShowMessage($"{MainCommand} reloadconfig", "hot reloads the config file so you don't need to restart");
                MyAPIGateway.Utilities.ShowMessage($"{MainCommand} exempt", "shows the SteamIDs of all exempt players");
                MyAPIGateway.Utilities.ShowMessage($"{MainCommand} addexemption", "adds the SteamID of the current user to the list of exempt players");
                MyAPIGateway.Utilities.ShowMessage($"{MainCommand} removeexemption", "removes the SteamID of the current user from the list of exempt players");
                MyAPIGateway.Utilities.ShowMessage($"{MainCommand} *multiplier <float>", "changes the corresponding multiplier:" +
                    "\nstaminadrainmultiplier\nfatiguedrainmultiplier\nhealthdrainmultiplier\nhungerdrainmultiplier\nstaminaincreasemultiplier\nfatigueincreasemultiplier\nhealthincreasemultiplier\nhungerincreasemultiplier\n");

            }
            catch (Exception e)
            {
                PEPCO.iSurvival.Log.iSurvivalLog.Error(e);
            }
        }
    }
}
