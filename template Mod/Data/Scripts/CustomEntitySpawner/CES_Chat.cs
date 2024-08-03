using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Game.ModAPI.Ingame.Utilities;
using System.Linq;
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

namespace PEPCO.iSurvival.CustomEntitySpawner
{
    public class CustomEntitySpawnerChat
    {
        public static CustomEntitySpawnerSettings settings = new CustomEntitySpawnerSettings();
        public static CustomEntitySpawner CESspawner = new CustomEntitySpawner();
        public static PEPCO_LogError log = new PEPCO_LogError();

        public static HashSet<string> validBotIds = new HashSet<string>();

        public void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            if (!MyAPIGateway.Session.IsServer)
            {
                return;
            }

            var player = MyAPIGateway.Session.Player;

            if (player == null || !MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Only admins can use this command.");
                return;
            }

            var command = messageText.Split(' ');
            switch (command[0].ToLower())
            {
                case "/pepco":
                    HandlePepcoCommand(command, ref sendToOthers);
                    break;
            }
        }
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
        public void HandlePepcoCommand(string[] command, ref bool sendToOthers)
        {
            if (command[1].ToLower() == "pause")
            {
                CESspawner.PauseScript();                
                sendToOthers = false;
                return;
            }

            if (CESspawner.scriptPaused)
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
                    LoadValidBotIds();
                    ListValidBotIds();
                    break;
                case "ces":
                    HandleCESCommand(command, ref sendToOthers);
                    break;
                case "kill":
                    HandleKillCommand(command, ref sendToOthers);
                    break;
                case "cleanup":
                    if (command.Length > 2 && command[2].ToLower() == "dead")
                    {
                        CleanupDeadEntities();
                        sendToOthers = false;
                    }
                    break;
                case "show":
                    if (command.Length > 2 && command[2].ToLower() == "all")
                    {
                        ShowAllEntities();
                        sendToOthers = false;
                    }
                    if (command.Length == 4)
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
                    break;
                case "logging":
                    settings.EnableLogging = !settings.EnableLogging;
                    MyAPIGateway.Utilities.ShowMessage("Custom Entity Spawner", $"Logging is now {(settings.EnableLogging ? "enabled" : "disabled")}.");
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
            if (command.Length == 3)
            {
                string botId = command[2];
                if (validBotIds.Contains(botId))
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
                    default:
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", "Unknown CES command. Use /pepco help for a list of commands.");
                        break;
                }
            }
        }
        public static void LoadValidBotIds()
        {
            var botDefinitions = MyDefinitionManager.Static.GetBotDefinitions();
            foreach (var botDefinition in botDefinitions)
            {
                validBotIds.Add(botDefinition.Id.SubtypeName);
            }
        }
        private void ListAllBlocksAndSpawns()
        {
            if (settings.BlockSpawnSettings.Count == 0)
            {
                MyAPIGateway.Utilities.ShowMessage("CustomEntitySpawner", "No block spawn settings found.");
                return;
            }

            foreach (var blockSettings in settings.BlockSpawnSettings)
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

                //if (MyAPIGateway.Session.IsServer)
                //{
                if (entityName.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    KillAllEntities(radius);
                }
                else
                {
                    KillEntitiesByName(entityName, radius);
                }
                //}
                sendToOthers = false;
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Usage: /pepco kill <EntityName|all> [Radius]");
            }
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
        "/pepco spawn <BotID> - Spawns a bot with the specified ID.",
        "/pepco listBots - Lists all valid bot IDs.",
        "/pepco logging - Enable debug logging",
        "/pepco pause - pause script"
    };

            foreach (var command in commands)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO Commands", command);
            }
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
        private static void ListValidBotIds()
        {
            foreach (var botId in validBotIds)
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", botId);
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

    }
}
