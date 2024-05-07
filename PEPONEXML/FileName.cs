using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage;
using System.IO;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace PEPONEXML
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class HeavyGasolineSession : MySessionComponentBase
    {
        public static bool EnableNPCs = false;
        public List<MyTuple<string, double>> gasList = new List<MyTuple<string, double>>()
            {
                MyTuple.Create("Gasoline",0.74),
                MyTuple.Create("RocketFuel",0.74),
                MyTuple.Create("Steam",0.74),
                MyTuple.Create("Deuterium",0.74),
                //MyTuple.Create("Hydrogen",0.01111),
                //MyTuple.Create("Oxygen",0.17777)
            };

        HeavyIOGasSettings Settings = new HeavyIOGasSettings();

        public override void LoadData()
        {

            Settings.Load();
            EnableNPCs = Settings.EnableNPCs;
            gasList = Settings.gasList;

            if (MyAPIGateway.Multiplayer.IsServer || MyAPIGateway.Utilities.IsDedicated)
                SaveSettings();
            LoadSettings();

            class HeavyIOGasSettings
        {
            public List<MyTuple<string, double>> gasList = new List<MyTuple<string, double>>()
            {
                MyTuple.Create("Gasoline",0.74),
                MyTuple.Create("RocketFuel",0.74),
                MyTuple.Create("Steam",0.74),
                MyTuple.Create("Deuterium",0.74),
                //MyTuple.Create("Hydrogen",0.01111),
                //MyTuple.Create("Oxygen",0.17777)
            };

            const string VariableId = nameof(HeavyIOGasSettings); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)
            const string FileName = "HeavyIOGas.ini"; // the file that gets saved to world storage under your mod's folder
            const string IniSection = "Config";

            public bool EnableNPCs = false;
            //public bool AdditionalGases = false;
            //public double GAS_L_KG_CONVERSION_Gasoline = 0.74; // The specific gravity of gasoline ranges from 0.71 to 0.77
            //public double GAS_L_KG_CONVERSION_RocketFuel = 0.791; //Density of Unsymmetrical dimethylhydrazine as 22°C
            //public double GAS_L_KG_CONVERSION_Steam = 0.0561597; //Density at 100 bar & 311.74 °C have fun changing it if you don't like it
            //public double GAS_L_KG_CONVERSION_Deuterium = 0.1624; // Density of liquid deuterium at 18 K (Yeah, I know quite cold eh?)
            //public string toGasList;
            //public string toConversionList;

            public HeavyIOGasSettings()
            {

            }

            public static Wheel7x7Config LoadSettings()
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage("ModConfig.xml", typeof(Wheel7x7Config)) == true)
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("ModConfig.xml", typeof(Wheel7x7Config));
                    return MyAPIGateway.Utilities.SerializeFromXML<Wheel7x7Config>(reader.ReadToEnd());
                }

                var settings = new Wheel7x7Config();
                SaveSettings(settings);
                return settings;
            }

            private static void SaveSettings(Wheel7x7Config settings)
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("ModConfig.xml", typeof(Wheel7x7Config)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(settings));
                }
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
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(HeavyIOGasSettings)))
                {
                    using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(HeavyIOGasSettings)))
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

                using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(HeavyIOGasSettings)))
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

        private void SaveSettings()
        {
            var configuration = Wheel7x7Config.LoadSettings();
            generalDamageMultiplier = configuration.enableNPC;
            disassembleRatio = configuration.disassembleRatio;
            wheelSuspensionPower = configuration.wheelSuspensionPower;
            wheelSuspensionComponents = configuration.wheelSuspensionComponents.GetRange(wheelSuspensionComponents.Count / 2, wheelSuspensionComponents.Count / 2);
            wheelBlockComponents = configuration.wheelBlockComponents.GetRange(wheelBlockComponents.Count / 2, wheelBlockComponents.Count / 2);
            wheelRealComponents = configuration.wheelRealComponents.GetRange(wheelRealComponents.Count / 2, wheelRealComponents.Count / 2);
            MyAPIGateway.Utilities.SetVariable("Wheel7x7_generalDamageMultiplier", generalDamageMultiplier);
            MyAPIGateway.Utilities.SetVariable("Wheel7x7_disassembleRatio", disassembleRatio);
            MyAPIGateway.Utilities.SetVariable("Wheel7x7_wheelSuspensionPower", wheelSuspensionPower);
            for (var i = 0; i < wheelSuspensionComponents.Count; i++)
                MyAPIGateway.Utilities.SetVariable("Wheel7x7_wheelSuspensionComponents" + i, wheelSuspensionComponents[i].ToArray());
            for (var i = 0; i < wheelBlockComponents.Count; i++)
                MyAPIGateway.Utilities.SetVariable("Wheel7x7_wheelBlockComponents" + i, wheelBlockComponents[i].ToArray());
            for (var i = 0; i < wheelRealComponents.Count; i++)
                MyAPIGateway.Utilities.SetVariable("Wheel7x7_wheelRealComponents" + i, wheelRealComponents[i].ToArray());
        }

        private void LoadSettings()
        {
            try
            {
                MyAPIGateway.Utilities.GetVariable("Wheel7x7_generalDamageMultiplier", out generalDamageMultiplier);
                MyAPIGateway.Utilities.GetVariable("Wheel7x7_disassembleRatio", out disassembleRatio);
                MyAPIGateway.Utilities.GetVariable("Wheel7x7_wheelSuspensionPower", out wheelSuspensionPower);
                for (var i = 0; i < wheelSuspensionComponents.Count; i++)
                {
                    string[] newList;
                    MyAPIGateway.Utilities.GetVariable("Wheel7x7_wheelSuspensionComponents" + i, out newList);
                    wheelSuspensionComponents[i] = new List<string>(newList);
                }
                for (var i = 0; i < wheelBlockComponents.Count; i++)
                {
                    string[] newList;
                    MyAPIGateway.Utilities.GetVariable("Wheel7x7_wheelBlockComponents" + i, out newList);
                    wheelBlockComponents[i] = new List<string>(newList);
                }
                for (var i = 0; i < wheelRealComponents.Count; i++)
                {
                    string[] newList;
                    MyAPIGateway.Utilities.GetVariable("Wheel7x7_wheelRealComponents" + i, out newList);
                    wheelRealComponents[i] = new List<string>(newList);
                }
            }
            catch (Exception e)
            {
                //MyVisualScriptLogicProvider.SendChatMessage(e.ToString());
            }
        }
    }

    public class Wheel7x7Config
    {
        public bool enableNPC;
        

        public Wheel7x7Config(bool enableNPC, )
        {
            this.enableNPC = generalDamageMultiplier;
            this.disassembleRatio = disassembleRatio;
            this.wheelSuspensionPower = wheelSuspensionPower;
            this.wheelSuspensionComponents = wheelSuspensionComponents;
            this.wheelBlockComponents = wheelBlockComponents;
            this.wheelRealComponents = wheelRealComponents;
        }

        public Wheel7x7Config()
        {
            var settings = Wheel7x7Settings.getSettings();
            enableNPC = settings.enableNPC;
            disassembleRatio = settings.disassembleRatio;
            wheelSuspensionPower = settings.wheelSuspensionPower;
            wheelSuspensionComponents = settings.wheelSuspensionComponents;
            wheelBlockComponents = settings.wheelBlockComponents;
            wheelRealComponents = settings.wheelRealComponents;
        }

        public static Wheel7x7Config LoadSettings()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("ModConfig.xml", typeof(Wheel7x7Config)) == true)
            {
                var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("ModConfig.xml", typeof(Wheel7x7Config));
                return MyAPIGateway.Utilities.SerializeFromXML<Wheel7x7Config>(reader.ReadToEnd());
            }

            var settings = new Wheel7x7Config();
            SaveSettings(settings);
            return settings;
        }

        private static void SaveSettings(Wheel7x7Config settings)
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("ModConfig.xml", typeof(Wheel7x7Config)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(settings));
            }
        }
    }
}
