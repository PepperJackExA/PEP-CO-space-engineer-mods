using PEPCO.iSurvival.CustomEntitySpawner.Config;
using System.Collections.Generic;
using VRage.ModAPI; // This provides access to IMyEntity.
using VRage.Game.ModAPI; // Include this if other VRage Game ModAPI components are needed.
using Sandbox.ModAPI; // Provides access to Space Engineers ModAPI components.


namespace PEPCO.iSurvival.CustomEntitySpawner.NPC
{
    public static class NPCDeathHandler
    {
        public static void OnEntityRemoved(IMyEntity entity, List<BlockSettings> blockSettings)
        {
            // Code to handle NPC death and trigger spawns
        }
    }
}
