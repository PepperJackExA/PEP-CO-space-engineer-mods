using System;
using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using IMyProgrammableBlock = Sandbox.ModAPI.Ingame.IMyProgrammableBlock;
using Digi;

namespace PEPONE.iSurvival
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

                TextPtr cmd = new TextPtr(messageText, MainCommand.Length);
                cmd = cmd.SkipWhitespace();

                if (cmd.StartsWithCaseInsensitive("exempt"))
                {
                    MyAPIGateway.Utilities.ShowMessage(Log.ModName, $"Exempt player ids: {String.Join(",", Mod.Settings.playerExceptions)}");
                }
                else if (cmd.StartsWithCaseInsensitive("addexemption"))
                {
                    Mod.Settings.playerExceptions.Add(MyAPIGateway.Session.LocalHumanPlayer.SteamUserId);
                    Mod.UpdateSettings();
                    MyAPIGateway.Utilities.ShowMessage(Log.ModName, $"Exempt player ids: {String.Join(",", Mod.Settings.playerExceptions)}");
                }
                else if (cmd.StartsWithCaseInsensitive("removeexemption"))
                {
                    Mod.Settings.playerRemovedExceptions = MyAPIGateway.Session.LocalHumanPlayer.SteamUserId;
                    Mod.UpdateSettings();
                    MyAPIGateway.Utilities.ShowMessage(Log.ModName, $"Exempt player ids: {String.Join(",", Mod.Settings.playerExceptions)}");
                }

                //    MyAPIGateway.Utilities.ShowMessage(Log.ModName, "Available commands:");
                //MyAPIGateway.Utilities.ShowMessage($"{MainCommand} clear ", "removes all objects from all PBs");
                //MyAPIGateway.Utilities.ShowMessage(Log.ModName, $"{MyAPIGateway.Session.LocalHumanPlayer.SteamUserId}");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}