using Sandbox.ModAPI;
using System;

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
            string existingContent = ReadExistingLogContent(logFilePath);

            WriteLogContent(logFilePath, existingContent, message);

            NotifyAdminPlayer(message);
        }

        private string ReadExistingLogContent(string logFilePath)
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
                {
                    return reader.ReadToEnd();
                }
            }
            return string.Empty;
        }

        private void WriteLogContent(string logFilePath, string existingContent, string message)
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
            {
                if (!string.IsNullOrEmpty(existingContent))
                {
                    writer.Write(existingContent);
                }
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

        private void NotifyAdminPlayer(string message)
        {
            var player = MyAPIGateway.Session.Player;
            if (player != null && MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
            {
                MyAPIGateway.Utilities.ShowMessage("CES", message);
            }
        }
    }
}
