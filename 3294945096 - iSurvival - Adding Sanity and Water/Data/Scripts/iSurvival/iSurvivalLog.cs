using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ParallelTasks;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace PEPCO.iSurvival.Log
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, priority: int.MaxValue)]
    public class iSurvivalLog : MySessionComponentBase
    {
        private static iSurvivalLog instance;
        private static Handler handler;
        private static bool unloaded = false;

        public const string FILE = "info.log";
        public const int PRINT_TIME_INFO = 3000;
        public const int PRINT_TIME_ERROR = 10000;
        public const string PRINT_ERROR = "error";
        public const string PRINT_MSG = "msg";

        public override void LoadData()
        {
            instance = this;
            EnsureHandlerCreated();
            handler.Init(this);
        }

        protected override void UnloadData()
        {
            instance = null;

            if (handler != null && handler.AutoClose)
            {
                Unload();
            }
        }

        private void Unload()
        {
            try
            {
                if (unloaded)
                    return;

                unloaded = true;
                handler?.Close();
                handler = null;
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Error in {ModContext.ModName} ({ModContext.ModId}): {e.Message}\n{e.StackTrace}");
                throw new ModCrashedException(e, ModContext);
            }
        }

        private static void EnsureHandlerCreated()
        {
            if (unloaded)
                throw new Exception("Digi.Log accessed after it was unloaded!");

            if (handler == null)
                handler = new Handler();
        }

        public static void Close()
        {
            instance?.Unload();
        }

        public static bool AutoClose
        {
            get
            {
                EnsureHandlerCreated();
                return handler.AutoClose;
            }
            set
            {
                EnsureHandlerCreated();
                handler.AutoClose = value;
            }
        }

        public static string ModName
        {
            get
            {
                EnsureHandlerCreated();
                return handler.ModName;
            }
            set
            {
                EnsureHandlerCreated();
                handler.ModName = value;
            }
        }

        public static ulong WorkshopId => handler?.WorkshopId ?? 0;

        public static void IncreaseIndent()
        {
            EnsureHandlerCreated();
            handler.IncreaseIndent();
        }

        public static void DecreaseIndent()
        {
            EnsureHandlerCreated();
            handler.DecreaseIndent();
        }

        public static void ResetIndent()
        {
            EnsureHandlerCreated();
            handler.ResetIndent();
        }

        public static void Error(Exception exception, string printText = PRINT_ERROR, int printTimeMs = PRINT_TIME_ERROR)
        {
            EnsureHandlerCreated();
            handler.Error(exception.ToString(), printText, printTimeMs);
        }

        public static void Error(string message, string printText = PRINT_ERROR, int printTimeMs = PRINT_TIME_ERROR)
        {
            EnsureHandlerCreated();
            handler.Error(message, printText, printTimeMs);
        }

        public static void Info(string message, string printText = null, int printTimeMs = PRINT_TIME_INFO)
        {
            EnsureHandlerCreated();
            handler.Info(message, printText, printTimeMs);
        }

        public static bool TaskHasErrors(Task task, string taskName)
        {
            EnsureHandlerCreated();

            if (task.Exceptions != null && task.Exceptions.Length > 0)
            {
                foreach (Exception e in task.Exceptions)
                {
                    Error($"Error in {taskName} thread!\n{e}");
                }

                return true;
            }

            return false;
        }

        private class Handler
        {
            private iSurvivalLog sessionComp;
            private string modName = string.Empty;

            private TextWriter writer;
            private int indent = 0;
            private string errorPrintText;

            private IMyHudNotification notifyInfo;
            private IMyHudNotification notifyError;

            private StringBuilder sb = new StringBuilder(64);

            private List<string> preInitMessages;

            public bool AutoClose { get; set; } = true;

            public ulong WorkshopId { get; private set; } = 0;

            public string ModName
            {
                get
                {
                    return modName;
                }
                set
                {
                    modName = value;
                    ComputeErrorPrintText();
                }
            }

            public Handler()
            {
            }

            public void Init(iSurvivalLog sessionComp)
            {
                if (writer != null)
                    return;

                if (MyAPIGateway.Utilities == null)
                {
                    Error("MyAPIGateway.Utilities is NULL !");
                    return;
                }

                this.sessionComp = sessionComp;

                if (string.IsNullOrWhiteSpace(ModName))
                    ModName = sessionComp.ModContext.ModName;

                WorkshopId = GetWorkshopID(sessionComp.ModContext.ModId);

                writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(FILE, typeof(iSurvivalLog));

                if (preInitMessages != null)
                {
                    string warning = $"{modName} WARNING: there are log messages before the mod initialized!";

                    Info($"--- pre-init messages ---");

                    foreach (string msg in preInitMessages)
                    {
                        Info(msg, warning);
                    }

                    Info("--- end pre-init messages ---");

                    preInitMessages = null;
                }

                sb.Clear();
                sb.Append("Initialized");
                sb.Append("\nGameMode=").Append(MyAPIGateway.Session.SessionSettings.GameMode);
                sb.Append("\nOnlineMode=").Append(MyAPIGateway.Session.SessionSettings.OnlineMode);
                sb.Append("\nServer=").Append(MyAPIGateway.Session.IsServer);
                sb.Append("\nDS=").Append(MyAPIGateway.Utilities.IsDedicated);
                sb.Append("\nDefined=");

#if STABLE
                sb.Append("STABLE, ");
#endif

#if UNOFFICIAL
                sb.Append("UNOFFICIAL, ");
#endif

#if DEBUG
                sb.Append("DEBUG, ");
#endif

#if BRANCH_STABLE
                sb.Append("BRANCH_STABLE, ");
#endif

#if BRANCH_DEVELOP
                sb.Append("BRANCH_DEVELOP, ");
#endif

#if BRANCH_UNKNOWN
                sb.Append("BRANCH_UNKNOWN, ");
#endif

                Info(sb.ToString());
                sb.Clear();
            }

            public void Close()
            {
                if (writer != null)
                {
                    Info("Unloaded.");

                    writer.Flush();
                    writer.Close();
                    writer = null;
                }
            }

            private void ComputeErrorPrintText()
            {
                errorPrintText = $"[ {modName} ERROR, report contents of: %AppData%/SpaceEngineers/Storage/{MyAPIGateway.Utilities.GamePaths.ModScopeName}/{FILE} ]";
            }

            public void IncreaseIndent()
            {
                indent++;
            }

            public void DecreaseIndent()
            {
                if (indent > 0)
                    indent--;
            }

            public void ResetIndent()
            {
                indent = 0;
            }

            public void Error(string message, string printText = PRINT_ERROR, int printTime = PRINT_TIME_ERROR)
            {
                MyLog.Default.WriteLineAndConsole(modName + " error/exception: " + message);

                LogMessage(message, "ERROR: ");

                if (printText != null)
                    ShowHudMessage(ref notifyError, message, printText, printTime, MyFontEnum.Red);
            }

            public void Info(string message, string printText = null, int printTime = PRINT_TIME_INFO)
            {
                LogMessage(message);

                if (printText != null)
                    ShowHudMessage(ref notifyInfo, message, printText, printTime, MyFontEnum.White);
            }

            private void ShowHudMessage(ref IMyHudNotification notify, string message, string printText, int printTime, string font)
            {
                if (printText == null)
                    return;

                try
                {
                    if (MyAPIGateway.Utilities != null && !MyAPIGateway.Utilities.IsDedicated)
                    {
                        if (printText == PRINT_ERROR)
                            printText = errorPrintText;
                        else if (printText == PRINT_MSG)
                            printText = message;

                        if (notify == null)
                        {
                            notify = MyAPIGateway.Utilities.CreateNotification(printText, printTime, font);
                        }
                        else
                        {
                            notify.Text = printText;
                            notify.AliveTime = printTime;
                            notify.ResetAliveTime();
                        }

                        notify.Show();
                    }
                }
                catch (Exception e)
                {
                    Info("ERROR: Could not send notification to local client: " + e);
                    MyLog.Default.WriteLineAndConsole(modName + " logger error/exception: Could not send notification to local client: " + e);
                }
            }

            private void LogMessage(string message, string prefix = null)
            {
                try
                {
                    sb.Clear();
                    sb.Append(DateTime.Now.ToString("[HH:mm:ss] "));

                    if (writer == null)
                        sb.Append("(PRE-INIT) ");

                    for (int i = 0; i < indent; i++)
                        sb.Append(' ', 4);

                    if (prefix != null)
                        sb.Append(prefix);

                    sb.Append(message);

                    if (writer == null)
                    {
                        if (preInitMessages == null)
                            preInitMessages = new List<string>();

                        preInitMessages.Add(sb.ToString());
                    }
                    else
                    {
                        writer.WriteLine(sb);
                        writer.Flush();
                    }

                    sb.Clear();
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"{modName} had an error while logging message = '{message}'\nLogger error: {e.Message}\n{e.StackTrace}");
                }
            }

            private ulong GetWorkshopID(string modId)
            {
                foreach (MyObjectBuilder_Checkpoint.ModItem mod in MyAPIGateway.Session.Mods)
                {
                    if (mod.Name == modId)
                        return mod.PublishedFileId;
                }

                return 0;
            }
        }
    }
}
