using System;
using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using PEPCO.iSurvival.Core;
using PEPCO.iSurvival.Log;
using VRage.Game.ModAPI;

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
                string cleanCommand = messageText.Replace(MainCommand, "").ToLower().Replace(" ", "");

                if (cleanCommand.StartsWith("exempt"))
                {
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Exempt player ids: {String.Join(",", iSurvivalSessionSettings.playerExceptions)}");
                    return;
                }
                else if (cleanCommand.StartsWith("addexemption"))
                {
                    Mod.Settings.playerExceptions.Add(thisPlayer.SteamUserId);
                    Mod.UpdateSettings();
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Exempt player ids: {String.Join(",", iSurvivalSessionSettings.playerExceptions)}");
                    return;
                }
                else if (cleanCommand.StartsWith("removeexemption"))
                {
                    Mod.Settings.playerExceptions.RemoveAll(x => x == thisPlayer.SteamUserId);
                    Mod.UpdateSettings();
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Exempt player ids: {String.Join(",", iSurvivalSessionSettings.playerExceptions)}");
                    return;
                }
                else if (cleanCommand.StartsWith("reloadconfig"))
                {
                    Mod.LoadData();
                    MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Reloaded configuration from file.");
                    return;
                }
                else if (cleanCommand.StartsWith("help"))
                {
                    DisplayHelp();
                    return;
                }
                else
                {
                    UpdateMultiplier(cleanCommand, messageText);
                }

                DisplayAvailableCommands();
            }
            catch (Exception e)
            {
                iSurvivalLog.Error(e);
            }
        }

        void UpdateMultiplier(string cleanCommand, string messageText)
        {
            string[] multipliers = new[]
            {
                "staminadrainmultiplier", "fatiguedrainmultiplier", "healthdrainmultiplier",
                "hungerdrainmultiplier", "staminaincreasemultiplier", "fatigueincreasemultiplier",
                "healthincreasemultiplier", "hungerincreasemultiplier", "waterdrainmultiplier",
                "waterincreasemultiplier", "sanitydrainmultiplier", "sanityincreasemultiplier",
                "processsittingeffectmultiplier", "processstandingeffectmultiplier", "processcrouchingeffectmultiplier",
                "processcrouchwalkingeffectmultiplier", "processwalkingeffectmultiplier", "processrunningeffectmultiplier",
                "processladdereffectmultiplier", "processflyingeffectmultiplier", "processfallingeffectmultiplier",
                "processsprintingeffectmultiplier", "processjumpingeffectmultiplier", "processdefaultmovementeffectmultiplier",
                "processhealthandsanityeffectsmultiplier", "processorganiccollectionmultiplier"
            };

            foreach (var multiplier in multipliers)
            {
                if (cleanCommand.StartsWith(multiplier))
                {
                    string temp = cleanCommand.Replace(multiplier, "");
                    float value;
                    if (float.TryParse(temp, out value))
                    {
                        SetMultiplier(multiplier, value);
                        Mod.UpdateSettings();
                        MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"New {multiplier}: {value}");
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Not a valid command: {messageText}");
                    }
                    return;
                }
            }
        }

        void SetMultiplier(string multiplierName, float value)
        {
            if (multiplierName == "staminadrainmultiplier")
            {
                Mod.Settings.staminadrainmultiplier = value;
            }
            else if (multiplierName == "fatiguedrainmultiplier")
            {
                Mod.Settings.fatiguedrainmultiplier = value;
            }
            else if (multiplierName == "healthdrainmultiplier")
            {
                Mod.Settings.healthdrainmultiplier = value;
            }
            else if (multiplierName == "hungerdrainmultiplier")
            {
                Mod.Settings.hungerdrainmultiplier = value;
            }
            else if (multiplierName == "staminaincreasemultiplier")
            {
                Mod.Settings.staminaincreasemultiplier = value;
            }
            else if (multiplierName == "fatigueincreasemultiplier")
            {
                Mod.Settings.fatigueincreasemultiplier = value;
            }
            else if (multiplierName == "healthincreasemultiplier")
            {
                Mod.Settings.healthincreasemultiplier = value;
            }
            else if (multiplierName == "hungerincreasemultiplier")
            {
                Mod.Settings.hungerincreasemultiplier = value;
            }
            else if (multiplierName == "waterdrainmultiplier")
            {
                Mod.Settings.waterdrainmultiplier = value;
            }
            else if (multiplierName == "waterincreasemultiplier")
            {
                Mod.Settings.waterincreasemultiplier = value;
            }
            else if (multiplierName == "sanitydrainmultiplier")
            {
                Mod.Settings.sanitydrainmultiplier = value;
            }
            else if (multiplierName == "sanityincreasemultiplier")
            {
                Mod.Settings.sanityincreasemultiplier = value;
            }
            else if (multiplierName == "processsittingeffectmultiplier")
            {
                Mod.Settings.ProcessSittingEffectMultiplier = value;
            }
            else if (multiplierName == "processstandingeffectmultiplier")
            {
                Mod.Settings.ProcessStandingEffectMultiplier = value;
            }
            else if (multiplierName == "processcrouchingeffectmultiplier")
            {
                Mod.Settings.ProcessCrouchingEffectMultiplier = value;
            }
            else if (multiplierName == "processcrouchwalkingeffectmultiplier")
            {
                Mod.Settings.ProcessCrouchWalkingEffectMultiplier = value;
            }
            else if (multiplierName == "processwalkingeffectmultiplier")
            {
                Mod.Settings.ProcessWalkingEffectMultiplier = value;
            }
            else if (multiplierName == "processrunningeffectmultiplier")
            {
                Mod.Settings.ProcessRunningEffectMultiplier = value;
            }
            else if (multiplierName == "processladdereffectmultiplier")
            {
                Mod.Settings.ProcessLadderEffectMultiplier = value;
            }
            else if (multiplierName == "processflyingeffectmultiplier")
            {
                Mod.Settings.ProcessFlyingEffectMultiplier = value;
            }
            else if (multiplierName == "processfallingeffectmultiplier")
            {
                Mod.Settings.ProcessFallingEffectMultiplier = value;
            }
            else if (multiplierName == "processsprintingeffectmultiplier")
            {
                Mod.Settings.ProcessSprintingEffectMultiplier = value;
            }
            else if (multiplierName == "processjumpingeffectmultiplier")
            {
                Mod.Settings.ProcessJumpingEffectMultiplier = value;
            }
            else if (multiplierName == "processdefaultmovementeffectmultiplier")
            {
                Mod.Settings.ProcessDefaultMovementEffectMultiplier = value;
            }
            else if (multiplierName == "processhealthandsanityeffectsmultiplier")
            {
                Mod.Settings.ProcessHealthAndSanityEffectsMultiplier = value;
            }
            else if (multiplierName == "processorganiccollectionmultiplier")
            {
                Mod.Settings.ProcessOrganicCollectionMultiplier = value;
            }
        }

        void DisplayAvailableCommands()
        {
            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Available commands:");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} reloadconfig", "Hot reloads the config file so you don't need to restart");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} exempt", "Shows the SteamIDs of all exempt players");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} addexemption", "Adds the SteamID of the current user to the list of exempt players");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} removeexemption", "Removes the SteamID of the current user from the list of exempt players");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} help", "Displays detailed information about available commands");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} *multiplier <float>", "Changes the corresponding multiplier:" +
                "\nstaminadrainmultiplier\nfatiguedrainmultiplier\nhealthdrainmultiplier\nhungerdrainmultiplier\nstaminaincreasemultiplier\nfatigueincreasemultiplier\nhealthincreasemultiplier\nhungerincreasemultiplier\nwaterdrainmultiplier\nwaterincreasemultiplier\nsanitydrainmultiplier\nsanityincreasemultiplier\nprocesssittingeffectmultiplier\nprocessstandingeffectmultiplier\nprocesscrouchingeffectmultiplier\nprocesscrouchwalkingeffectmultiplier\nprocesswalkingeffectmultiplier\nprocessrunningeffectmultiplier\nprocessladdereffectmultiplier\nprocessflyingeffectmultiplier\nprocessfallingeffectmultiplier\nprocesssprintingeffectmultiplier\nprocessjumpingeffectmultiplier\nprocessdefaultmovementeffectmultiplier\nprocesshealthandsanityeffectsmultiplier\nprocessorganiccollectionmultiplier\n");
        }

        void DisplayHelp()
        {
            MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, "Detailed command information:");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} reloadconfig", "Reloads the configuration file without restarting the server.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} exempt", "Shows the SteamIDs of all players exempt from the iSurvival mod.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} addexemption", "Adds the SteamID of the current user to the list of exempt players.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} removeexemption", "Removes the SteamID of the current user from the list of exempt players.");
            MyAPIGateway.Utilities.ShowMessage($"{MainCommand} *multiplier <float>", "Changes the value of the specified multiplier. Examples include:" +
                "\nstaminadrainmultiplier\nfatiguedrainmultiplier\nhealthdrainmultiplier\nhungerdrainmultiplier\nstaminaincreasemultiplier\nfatigueincreasemultiplier\nhealthincreasemultiplier\nhungerincreasemultiplier\nwaterdrainmultiplier\nwaterincreasemultiplier\nsanitydrainmultiplier\nsanityincreasemultiplier\nprocesssittingeffectmultiplier\nprocessstandingeffectmultiplier\nprocesscrouchingeffectmultiplier\nprocesscrouchwalkingeffectmultiplier\nprocesswalkingeffectmultiplier\nprocessrunningeffectmultiplier\nprocessladdereffectmultiplier\nprocessflyingeffectmultiplier\nprocessfallingeffectmultiplier\nprocesssprintingeffectmultiplier\nprocessjumpingeffectmultiplier\nprocessdefaultmovementeffectmultiplier\nprocesshealthandsanityeffectsmultiplier\nprocessorganiccollectionmultiplier\n");
        }
    }
}
