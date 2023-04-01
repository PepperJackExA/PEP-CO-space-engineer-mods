using Sandbox.ModAPI;
using System;
using System.IO;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities; // this ingame namespace is safe to use in mods as it has nothing to collide with
using VRage.Utils;

namespace PEPCO_Limited_Voxel_Placement_Config
{
    // This example is minimal code required for it to work and with comments so you can better understand what is going on.

    // The gist of it is: ini file is loaded/created that admin can edit, SetVariable is used to store that data in sandbox.sbc which gets automatically sent to joining clients.
    // Benefit of this is clients will be getting this data before they join, very good if you need it during LoadData()
    // This example does not support reloading config while server runs, you can however implement that by sending a packet to all online players with the ini data for them to parse.

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class Config_Basic : MySessionComponentBase
    {
        Config_Settings Settings = new Config_Settings();

        public override void LoadData()
        {
            Settings.Load();

            // example usage/debug
            MyLog.Default.WriteLineAndConsole($"### SomeNumber value={Settings.ListofSubGridIds}");
        }
    }

    public class Config_Settings
    {
        const string VariableId = nameof(Config_Basic); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)
        const string FileName = "Config.ini"; // the file that gets saved to world storage under your mod's folder
        const string IniSection = "Config";

        // settings you'd be reading, and their defaults.
        public string[] ListofSubGridIds = { "BasicStaticDrill", "StaticDrill" };
        
        void LoadConfig(MyIni iniParser)
        {
            // repeat for each setting field
            ListofSubGridIds = iniParser.Get(IniSection, nameof(ListofSubGridIds)).(ListofSubGridIds);
        }

        void SaveConfig(MyIni iniParser)
        {
            // repeat for each setting field
            iniParser.Set(IniSection, nameof(ListofSubGridIds), ListofSubGridIds);
            iniParser.SetComment(IniSection, nameof(ListofSubGridIds), "This number does something for sure"); // optional

        }

        // nothing to edit below this point

        public Config_Settings()
        {
        }

        public void Load()
        {
            if (MyAPIGateway.Session.IsServer)
                LoadOnHost();
            else
                LoadOnClient();
        }

        void LoadOnHost()
        {
            MyIni iniParser = new MyIni();

            // load file if exists then save it regardless so that it can be sanitized and updated

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(Config_Settings)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(Config_Settings)))
                {
                    string text = file.ReadToEnd();

                    MyIniParseResult result;
                    if (!iniParser.TryParse(text, out result))
                        throw new Exception($"Config error: {result.ToString()}");

                    LoadConfig(iniParser);
                }
            }

            iniParser.Clear(); // remove any existing settings that might no longer exist

            SaveConfig(iniParser);

            string saveText = iniParser.ToString();

            MyAPIGateway.Utilities.SetVariable<string>(VariableId, saveText);

            using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(Config_Settings)))
            {
                file.Write(saveText);
            }
        }

        void LoadOnClient()
        {
            string text;
            if (!MyAPIGateway.Utilities.GetVariable<string>(VariableId, out text))
                throw new Exception("No config found in sandbox.sbc!");

            MyIni iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(text, out result))
                throw new Exception($"Config error: {result.ToString()}");

            LoadConfig(iniParser);
        }
    }
}