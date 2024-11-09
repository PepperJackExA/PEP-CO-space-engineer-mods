using PEPCO.iSurvival.CustomEntitySpawner.Config;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game.ObjectBuilders.AI.Bot;
using VRage.Game;
using VRageMath;
using PEPCO.iSurvival.CustomEntitySpawner.Utilities;
using VRage.Game.ModAPI;
using System;

namespace PEPCO.iSurvival.CustomEntitySpawner.Spawning
{
    public static class EntitySpawner
    {
        // Spawns entities based on block settings and position
        public static void SpawnEntityForBlock(BlockSettings blockSettings, Vector3D blockPosition)
        {
            if (blockSettings.EnableCustomSpawning)
            {
                if (SpawnConditions.CheckSpawnConditions(blockSettings))
                {
                    for (int i = 0; i < blockSettings.SpawnCount; i++)
                    {
                        Vector3D spawnPosition = GetRandomSpawnPosition(blockPosition, blockSettings.SpawnRadius, blockSettings.SpawnHeight);
                        SpawnEntity(blockSettings.EntityName, spawnPosition);
                    }
                }
            }
        }
        // Helper method to handle spawning an individual entity at a specific position
        public static void SpawnEntity(string entityName, Vector3D position)
        {
            var entityId = new MyDefinitionId(typeof(IMyCharacter), entityName);
            IMyEntity entity;
            bool success = MyAPIGateway.Entities.TryGetEntityById(entityId, out entity);  // Placeholder for actual entity creation

            if (success && entity != null)
            {
                MyAPIGateway.Entities.AddEntity(entity);
                Logger.LogError($"Successfully spawned entity: {entity.DisplayName} at position {position}");
            }
            else
            {
                Logger.LogError($"Failed to spawn entity of type {entityName} at position {position}");
            }
        }        
        public static Vector3D GetRandomSpawnPosition(Vector3D centerPosition, double radius, double height)
        {
            var random = new Random();
            double angle = random.NextDouble() * Math.PI * 2;
            double distance = random.NextDouble() * radius;
            double offsetX = Math.Cos(angle) * distance;
            double offsetZ = Math.Sin(angle) * distance;
            double offsetY = (random.NextDouble() - 0.5) * height;

            return centerPosition + new Vector3D(offsetX, offsetY, offsetZ);
        }

    }
}
