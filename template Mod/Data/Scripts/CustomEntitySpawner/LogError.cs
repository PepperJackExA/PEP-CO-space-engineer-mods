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
    public class PEPCO_LogError
    {
        public static CustomEntitySpawnerSettings settings = new CustomEntitySpawnerSettings();

        public void LogError(string message)
        {
            if (!settings.EnableLogging)
                return;

            string logFilePath = "CustomEntitySpawner.log";
            string existingContent = "";

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
                {
                    existingContent = reader.ReadToEnd();
                }
            }

            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
            {
                if (!string.IsNullOrEmpty(existingContent))
                {
                    writer.Write(existingContent);
                }
                writer.WriteLine($"{DateTime.Now}: {message}");
            }

            var player = MyAPIGateway.Session.Player;
            if (player != null && MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
            {
                MyAPIGateway.Utilities.ShowMessage("CES", message);
            }
        }
    }
}
