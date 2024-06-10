using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Utils;

namespace PEPCO_Limited_Cargo_Modification
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Limited_Cargo_Modification : MySessionComponentBase
    {
        PEPCO_Cargo_Settings Settings = new PEPCO_Cargo_Settings();
        List<string> excludedTypeBlocks;
        List<string> excludedSubTypeBlocks;

        public override void LoadData()
        {
            base.LoadData();
            Settings.Load();
            excludedTypeBlocks = Settings.ExcludedBlockTypes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            excludedSubTypeBlocks = Settings.ExcludedBlockSubTypes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
            base.UnloadData();
        }

        private void OnEntityAdd(VRage.Game.ModAPI.IMyEntity entity)
        {
            var cargoContainer = entity as Sandbox.Game.Entities.Cube.MyCargoContainer;
            if (cargoContainer != null)
            {
                var definition = cargoContainer.BlockDefinition as MyCargoContainerDefinition;
                if (definition != null && !excludedTypeBlocks.Contains(definition.Id.TypeId.ToString()) && !excludedSubTypeBlocks.Contains(definition.Id.SubtypeId.ToString()))
                {
                    MyLog.Default.WriteLineAndConsole($"### Adjusting cargo capacity of block={definition.Id.TypeId} {definition.Id.SubtypeId}");

                    // Access and modify the inventory component definitions
                    if (cargoContainer.GetInventoryBase() != null)
                    {
                        var inventory = cargoContainer.GetInventoryBase();
                        var size = new VRageMath.Vector3(100, 100, 100);
                        inventory.Constraint = new MyInventoryConstraint("NewSize", null, size.Volume);
                        MyLog.Default.WriteLineAndConsole($"### Adjusted inventory size: {inventory.CurrentVolume} / {inventory.MaxVolume}");
                    }
                }
            }
        }
    }

    public class PEPCO_Cargo_Settings
    {
        const string VariableId = nameof(PEPCO_Limited_Cargo_Modification);
        const string FileName = "PEPCO_Cargo_Settings.ini";
        const string IniSection = "Cargo_Settings";

        public string ExcludedBlockTypes = "MyObjectBuilder_BatteryBlock, MyObjectBuilder_Conveyor, MyObjectBuilder_ConveyorConnector";
        public string ExcludedBlockSubTypes = "BasicCargoContainer, AdvancedCargoContainer";

        public void Load()
        {
            if (MyAPIGateway.Session.IsServer)
                LoadOnHost();
            else
                LoadOnClient();
        }

        void LoadOnHost()
        {
            var iniParser = new MyIni();
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(PEPCO_Cargo_Settings)))
            {
                using (var file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(PEPCO_Cargo_Settings)))
                {
                    string fileContent = file.ReadToEnd();
                    MyIniParseResult result;
                    if (!iniParser.TryParse(fileContent, out result))
                        throw new Exception($"Config error: {result}");
                }
            }

            SaveConfig(iniParser);
            var saveText = iniParser.ToString();
            MyAPIGateway.Utilities.SetVariable(VariableId, saveText);

            using (var file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(PEPCO_Cargo_Settings)))
            {
                file.Write(saveText);
            }
        }

        void LoadOnClient()
        {
            string text;
            if (!MyAPIGateway.Utilities.GetVariable(VariableId, out text))
                throw new Exception("No config found in sandbox.sbc!");

            var iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(text, out result))
                throw new Exception($"Config error: {result}");

            LoadConfig(iniParser);
        }

        void LoadConfig(MyIni iniParser)
        {
            ExcludedBlockTypes = iniParser.Get(IniSection, nameof(ExcludedBlockTypes)).ToString(ExcludedBlockTypes);
            ExcludedBlockSubTypes = iniParser.Get(IniSection, nameof(ExcludedBlockSubTypes)).ToString(ExcludedBlockSubTypes);
        }

        void SaveConfig(MyIni iniParser)
        {
            iniParser.Set(IniSection, nameof(ExcludedBlockTypes), ExcludedBlockTypes);
            iniParser.Set(IniSection, nameof(ExcludedBlockSubTypes), ExcludedBlockSubTypes);
        }
    }
}
