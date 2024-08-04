using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using VRage.ModAPI;

using System.IO;
using System.Linq;
using VRage.ObjectBuilders;
using PEPCO.LogError;
using PEPCO.Sync;

namespace PEPCO.iSurvival.CustomEntitySpawner
{
    
    public class CustomEntitySpawnerChat
    {
        public static CustomEntitySpawnerSettings CESsettings = new CustomEntitySpawnerSettings();
        public static CustomEntitySpawner CESspawner = new CustomEntitySpawner();
        public static PEPCO_LogError log = new PEPCO_LogError();


        private const string CommandPrefix = "/pepco";

        #region Command Handling

        public void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            MyAPIGateway.Utilities.RegisterMessageHandler( ("CES", $"session?:{MyAPIGateway.Session.IsServer} multiplayer?: {MyAPIGateway.Multiplayer.IsServer}");
            //if (!MyAPIGateway.Multiplayer.IsServer) return;
            var player = MyAPIGateway.Session.Player;
            if (player == null || !MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
                {
                    MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Only admins can use this command.");
                    return;
                }

                var command = messageText.Split(' ');
                if (command[0].Equals(CommandPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    HandlePepcoCommand(command, ref sendToOthers);
                }
            
        }

        public void HandlePepcoCommand(string[] command, ref bool sendToOthers)
        {
            if (command.Length < 2) return;

            if (command[1].Equals("pause", StringComparison.OrdinalIgnoreCase))
            {
                CESspawner.PauseScript();
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"PauseScript:{CESsettings.scriptPaused}");
                CESsettings.Load();
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Load:{CESsettings.scriptPaused}");
                
                sendToOthers = false;
                return;
            }

            if (CESsettings.scriptPaused)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Script is currently paused.");

                return;
            }

            switch (command[1].ToLower())
            {
                case "spawn":
                    HandleSpawnCommand(command);
                    break;
                case "listbots":
                    CESspawner.ListValidBotIds();
                    break;
                case "ces":
                    HandleCESCommand(command, ref sendToOthers);
                    break;
                case "kill":
                    HandleKillCommand(command, ref sendToOthers);
                    break;
                case "cleanup":
                    if (command.Length > 2 && command[2].Equals("dead", StringComparison.OrdinalIgnoreCase))
                    {
                        CleanupDeadEntities();
                        sendToOthers = false;
                    }
                    break;
                case "show":
                    HandleShowCommand(command, ref sendToOthers);
                    break;
                case "logging":
                    CESsettings.EnableLogging = !CESsettings.EnableLogging;
                    CESsettings.Load();
                    MyAPIGateway.Utilities.ShowMessage("Custom Entity Spawner", $"Logging is now {(CESsettings.EnableLogging ? "enabled" : "disabled")}.");
                    sendToOthers = false;
                    break;
                case "help":
                    ShowHelp();
                    sendToOthers = false;
                    break;
                default:
                    MyAPIGateway.Utilities.ShowMessage("PEPCO", "Unknown command. Use /pepco help for a list of commands.");
                    break;
            }
        }

        private void HandleSpawnCommand(string[] command)
        {
            CESspawner.LoadValidBotIds();
            var BotIds = CESspawner.validBotIds;
            if (command.Length == 3)
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"command.Length");
                string botId = command[2];
                if (BotIds.Contains(botId))
                {
                    Vector3D playerPosition = MyAPIGateway.Session.Player.GetPosition();
                    Vector3D forwardDirection = MyAPIGateway.Session.Player.Character.WorldMatrix.Forward;
                    Vector3D offset = forwardDirection * 5 * 2.5;
                    Vector3D spawnPosition = playerPosition + offset;
                    MyVisualScriptLogicProvider.SpawnBot(botId, spawnPosition);
                    MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Spawned bot: {botId} at {spawnPosition}");

                    int entitiesCount = CountEntitiesInRadius(playerPosition, 10);
                    MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"{entitiesCount} entities are within a 10-meter radius.");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Invalid bot ID: {botId}");
                }
            }
        }

        private void HandleCESCommand(string[] command, ref bool sendToOthers)
        {
            if (command.Length == 3)
            {
                switch (command[2].ToLower())
                {
                    case "reload":
                        ReloadSettings();
                        sendToOthers = false;
                        break;
                    case "list":
                        ListAllBlocksAndSpawns();
                        sendToOthers = false;
                        break;
                    case "update":
                        // Example usage of sending a settings update packet
                        var newSettings = new BotSpawnerConfig("SmallBlockSmallContainer", "MyObjectBuilder_CargoContainer");
                        var packet = new PacketBlockSettings(0, newSettings); // Example entityId, replace if necessary
                        packet.Send();
                        sendToOthers = false;
                        break;
                    default:
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", "Unknown CES command. Use /pepco help for a list of commands.");
                        break;
                }
            }
        }
        private void HandleKillCommand(string[] command, ref bool sendToOthers)
        {
            if (command.Length >= 3)
            {
                string entityName = command[2];
                double radius = 10;
                if (command.Length == 4)
                {
                    double parsedRadius;
                    if (double.TryParse(command[3], out parsedRadius))
                    {
                        radius = parsedRadius;
                    }
                }

                if (entityName.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    KillAllEntities(radius);
                }
                else
                {
                    KillEntitiesByName(entityName, radius);
                }
                sendToOthers = false;
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Usage: /pepco kill <EntityName|all> [Radius]");
            }
        }

        private void HandleShowCommand(string[] command, ref bool sendToOthers)
        {
            if (command.Length > 2 && command[2].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                ShowAllEntities();
                sendToOthers = false;
            }
            else if (command.Length == 4)
            {
                string entityId = command[2];
                double radius;
                if (double.TryParse(command[3], out radius))
                {
                    ShowEntities(entityId, radius);
                    sendToOthers = false;
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("PEPCO", "Invalid radius. Usage: /pepco show <entityID> <radius>");
                }
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Usage: /pepco show <entityID> <radius>");
            }
        }

        private void ShowHelp()
        {
            var commands = new List<string>
            {
                "/pepco help - Displays this help message.",
                "/pepco CES reload - Reloads the Custom Entity Spawner settings.",
                "/pepco CES list - Lists all blocks and their spawn settings.",
                "/pepco kill <EntityName|all> [Radius] - Kills entities by name or all entities within a radius.",
                "/pepco cleanup dead - Cleans up dead entities.",
                "/pepco show all - Shows all entities.",
                "/pepco show <entityID> <radius> - Shows entities of a specific ID within a radius.",
                "/pepco spawn <BotID> - Spawns a bot with the specified ID.",
                "/pepco listBots - Lists all valid bot IDs.",
                "/pepco logging - Enable or disable debug logging.",
                "/pepco pause - Pause script execution."
            };

            foreach (var command in commands)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO Commands", command);
            }
        }

        #endregion

        #region Entity Operations

        private void ReloadSettings()
        {
            try
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Reloading settings...");
                log.LogError("Reloading settings...");

                CESspawner.EnsureDefaultIniFilesExist();
                CESspawner.CopyAllCESFilesToWorldStorage();
                CESspawner.LoadAllFilesFromWorldStorage();
                CESspawner.InitializeBotSpawnerConfig();

                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Settings reloaded successfully.");
                log.LogError("Settings reloaded successfully.");
            }
            catch (Exception ex)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Error reloading settings: {ex.Message}");
                log.LogError($"Error reloading settings: {ex.Message}");
            }
        }

        private void ListAllBlocksAndSpawns()
        {
            if (CESsettings.BlockSpawnSettings.Count == 0)
            {
                MyAPIGateway.Utilities.ShowMessage("CustomEntitySpawner", "No block spawn settings found.");
                return;
            }

            foreach (var blockSettings in CESsettings.BlockSpawnSettings)
            {
                string message = $"BlockId: {blockSettings.BlockId}, BlockType: {blockSettings.BlockType}, Entities: {string.Join(", ", blockSettings.EntityID)}";
                MyAPIGateway.Utilities.ShowMessage("CustomEntitySpawner", message);
            }
        }

        private void KillAllEntities(double radius)
        {
            var entities = new HashSet<IMyEntity>();
            Vector3D playerPosition = MyAPIGateway.Session.Player.GetPosition();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;

            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            int entitiesKilled = 0;
            foreach (var entity in entities)
            {
                IMyCharacter character = entity as IMyCharacter;
                if (character != null && Vector3D.Distance(character.GetPosition(), playerPosition) <= radius && character != playerCharacter)
                {
                    entity.Close();
                    entitiesKilled++;
                }
            }

            MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Killed {entitiesKilled} entities within {radius} meters.");
        }

        private void KillEntitiesByName(string entitySubtypeId, double radius)
        {
            var entities = new HashSet<IMyEntity>();
            Vector3D playerPosition = MyAPIGateway.Session.Player.GetPosition();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;

            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            int entitiesKilled = 0;
            foreach (var entity in entities)
            {
                IMyCharacter character = entity as IMyCharacter;
                string entitySubtypeIdCurrent = (entity as MyEntity)?.DefinitionId?.SubtypeId.ToString();
                if (character != null && Vector3D.Distance(character.GetPosition(), playerPosition) <= radius && character != playerCharacter && entitySubtypeIdCurrent != null && entitySubtypeIdCurrent.IndexOf(entitySubtypeId, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    entity.Close();
                    entitiesKilled++;
                }
            }

            MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Killed {entitiesKilled} entities with SubtypeId: {entitySubtypeId} within {radius} meters.");
        }

        private void ShowEntities(string entitySubtypeId, double radius)
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            int entityCount = 0;
            foreach (var entity in entities)
            {
                string entityCurrentSubtypeId = (entity as MyEntity)?.DefinitionId?.SubtypeId.ToString();

                if (entityCurrentSubtypeId != null && entityCurrentSubtypeId.Equals(entitySubtypeId, StringComparison.OrdinalIgnoreCase) &&
                    Vector3D.Distance(entity.GetPosition(), MyAPIGateway.Session.Player.GetPosition()) <= radius)
                {
                    entityCount++;
                    string entityType = entity.GetType().Name;
                    Vector3D entityPosition = entity.GetPosition();

                    log.LogError($"Type: {entityType}, SubtypeId: {entityCurrentSubtypeId}, Location: {entityPosition}");
                }
            }

            log.LogError($"Total entities of subtype '{entitySubtypeId}' within {radius} meters: {entityCount}");
        }

        private void ShowAllEntities()
        {
            var entities = new HashSet<IMyEntity>();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;
            MyAPIGateway.Entities.GetEntities(entities);
            foreach (var entity in entities)
            {
                var character = entity as IMyCharacter;
                if (entity is IMyCharacter && character != playerCharacter && !character.IsDead)
                {
                    string entityType = entity.GetType().Name;
                    string entitySubtypeId = (entity as MyEntity)?.DefinitionId?.SubtypeId.ToString() ?? "Unnamed";
                    Vector3D entityPosition = entity.GetPosition();

                    MyAPIGateway.Utilities.ShowMessage("Entity", $"Type: {entityType}, SubtypeId: {entitySubtypeId}, Location: {entityPosition}");
                }
            }
        }

        public void CleanupDeadEntities()
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities);

            foreach (var entity in entities)
            {
                if (IsEntityDead(entity))
                {
                    entity.Close();
                }
            }
        }

        private bool IsEntityDead(IMyEntity entity)
        {
            var character = entity as IMyCharacter;
            if (character != null && character.IsDead)
            {
                return true;
            }
            return false;
        }

        private int CountEntitiesInRadius(Vector3D position, double radius)
        {
            var entities = new HashSet<IMyEntity>();
            Vector3D playerPosition = MyAPIGateway.Session.Player.GetPosition();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;

            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);
            int entitiesCount = 0;
            foreach (var entity in entities)
            {
                IMyCharacter character = entity as IMyCharacter;
                if (character != null && Vector3D.Distance(character.GetPosition(), playerPosition) <= radius && character != playerCharacter)
                {
                    entitiesCount++;
                }
            }
            return entitiesCount;
        }

        #endregion

        #region Utility Methods

        

        #endregion
    }
}
