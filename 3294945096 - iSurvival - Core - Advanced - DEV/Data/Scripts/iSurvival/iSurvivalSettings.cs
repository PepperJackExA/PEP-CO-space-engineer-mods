using System.IO;
using System.Linq;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using PEPCO.iSurvival.Chat;
using PEPCO.iSurvival.Log;
using Sandbox.Game.GameSystems.Chat;

namespace PEPCO.iSurvival.settings
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class iSurvivalSessionSettings : MySessionComponentBase
    {
        public iSurvivalSettings Settings = new iSurvivalSettings();
        public static List<ulong> playerExceptions = new List<ulong>();
        public ChatCommands chatCommands;

        public override void LoadData()
        {
            try
            {
                Settings.Load(); // Load settings
                ApplySettings(); // Apply once here, not in every class
                chatCommands = new ChatCommands(this); // Initialize only once
            }
            catch (Exception ex)
            {
                iSurvivalLog.Error("Failed to load settings: " + ex.Message);
            }
        }


        protected override void UnloadData()
        {
            base.UnloadData();
            if (chatCommands != null)
            {
                chatCommands.Dispose(); // Dispose chat commands to unregister event handlers
            }
        }

        // Updates settings and applies them
        public void UpdateSettings()
        {
            try
            {
                ApplySettings(); // Reapply settings
                Settings.SaveConfigAfterChatUpdate(); // Save the updated settings
                playerExceptions = Settings.playerExceptions.Distinct().ToList(); // Update player exceptions
                iSurvivalLog.Info("Settings updated successfully.");
            }
            catch (Exception ex)
            {
                iSurvivalLog.Error("Failed to update settings: " + ex.Message);
            }
        }

        // Applies settings from the configuration to the session variables
        private void ApplySettings()
        {
            foreach (var setting in stats.StatManager._statSettings.Values)
            {
                setting.UpdateValue(); // Recalculate the value based on base and multiplier
                iSurvivalLog.Info($"Applied setting: {setting}");
            }
        }

        public class iSurvivalSettings
        {
            const string VariableId = nameof(iSurvivalSettings);
            const string FileName = "iSurvivalSettings.ini";
            const string IniSection = "Config";

            public List<ulong> playerExceptions = new List<ulong>();

            // Loads settings based on whether the session is server or client
            public void Load()
            {
                if (MyAPIGateway.Session.IsServer)
                {
                    LoadOnHost(); // Load settings on the server
                }
                else
                {
                    iSurvivalLog.Info("Settings can only be loaded on the server.");
                }
            }

            // Loads settings on the server
            private void LoadOnHost()
            {
                try
                {
                    MyIni iniParser = new MyIni();

                    if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(iSurvivalSettings)))
                    {
                        using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(iSurvivalSettings)))
                        {
                            string text = file.ReadToEnd();
                            MyIniParseResult result;
                            if (!iniParser.TryParse(text, out result))
                            {
                                iSurvivalLog.Error($"Failed to parse config file: {result.Error}");
                                throw new Exception($"Config error: {result}");
                            }
                            LoadConfig(iniParser); // Load settings from the ini parser
                        }
                    }
                    else
                    {
                        iSurvivalLog.Info("Configuration file not found, creating default configuration.");
                        SaveConfig(iniParser); // Save default configuration if file doesn't exist
                        SaveToStorage(iniParser.ToString());
                    }
                }
                catch (Exception ex)
                {
                    iSurvivalLog.Error("Error loading settings: " + ex.Message);
                }
            }

            // Loads the configuration values from the parsed ini file
            public void LoadConfig(MyIni iniParser)
            {
                foreach (var stat in stats.StatManager._statSettings)
                {
                    try
                    {
                        stat.Value.Base = (float)iniParser.Get(IniSection, $"{stat.Key}.base").ToDouble(stat.Value.Base);
                        stat.Value.Multiplier = (float)iniParser.Get(IniSection, $"{stat.Key}.multiplier").ToDouble(stat.Value.Multiplier);

                        // Load new movement type multipliers
                        stat.Value.IncreaseMultiplier = (float)iniParser.Get(IniSection, $"{stat.Key}.increaseMultiplier").ToDouble(stat.Value.IncreaseMultiplier);
                        stat.Value.DecreaseMultiplier = (float)iniParser.Get(IniSection, $"{stat.Key}.decreaseMultiplier").ToDouble(stat.Value.DecreaseMultiplier);

                        stat.Value.UpdateValue(); // Recalculate the value
                        iSurvivalLog.Info($"Loaded setting: {stat.Key} - Base: {stat.Value.Base}, Multiplier: {stat.Value.Multiplier}");
                    }
                    catch (Exception ex)
                    {
                        iSurvivalLog.Error($"Failed to load setting {stat.Key}: {ex.Message}");
                    }
                }

                // Get the player list from the ini and split it to an array
                playerExceptions = iniParser.Get(IniSection, nameof(playerExceptions)).ToString().Trim().Split('\n')
                    .Select(config =>
                    {
                        ulong playerId;
                        return ulong.TryParse(config, out playerId) ? playerId : 0;
                    })
                    .Where(playerId => playerId > 0)
                    .ToList();

                iSurvivalLog.Info("Loaded player exceptions: " + string.Join(", ", playerExceptions));
            }

            // Saves configuration to the ini file
            public void SaveConfig(MyIni iniParser)
            {
                foreach (var stat in stats.StatManager._statSettings)
                {
                    iniParser.Set(IniSection, $"{stat.Key}.base", stat.Value.Base);
                    iniParser.Set(IniSection, $"{stat.Key}.multiplier", stat.Value.Multiplier);
                    iniParser.Set(IniSection, $"{stat.Key}.increaseMultiplier", stat.Value.IncreaseMultiplier);
                    iniParser.Set(IniSection, $"{stat.Key}.decreaseMultiplier", stat.Value.DecreaseMultiplier);
                }

                iniParser.Set(IniSection, nameof(playerExceptions), string.Join("\n", playerExceptions.Distinct().ToArray()));
                iniParser.SetComment(IniSection, nameof(playerExceptions), "Add the IDs of players who should be exempt from the iSurvival mod");
                iSurvivalLog.Info("Configuration saved.");
            }

            // Saves configuration after chat update
            public void SaveConfigAfterChatUpdate()
            {
                try
                {
                    MyIni iniParser = new MyIni();
                    SaveConfig(iniParser);
                    SaveToStorage(iniParser.ToString());
                    iSurvivalLog.Info("Configuration saved after chat update.");
                }
                catch (Exception ex)
                {
                    iSurvivalLog.Error("Failed to save configuration after chat update: " + ex.Message);
                }
            }

            // Saves configuration string to storage
            private void SaveToStorage(string saveText)
            {
                try
                {
                    MyAPIGateway.Utilities.SetVariable<string>(VariableId, saveText);

                    using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(iSurvivalSettings)))
                    {
                        file.Write(saveText);
                    }
                    iSurvivalLog.Info("Configuration saved to world storage.");
                }
                catch (Exception ex)
                {
                    iSurvivalLog.Error("Error saving configuration to storage: " + ex.Message);
                }
            }
        }
    }
}
