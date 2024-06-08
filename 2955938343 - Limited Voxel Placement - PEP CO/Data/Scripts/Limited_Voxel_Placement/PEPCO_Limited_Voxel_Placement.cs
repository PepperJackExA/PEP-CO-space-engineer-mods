using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
using VRage.Utils;

namespace PEPCO_Limited_Voxel_Placement
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Limited_Voxel_Placement : MySessionComponentBase
    {
        PEPCO_Voxel_Settings Settings = new PEPCO_Voxel_Settings();
        List<string> excludedTypeBlocks;
        List<string> excludedSubTypeBlocks;

        public override void LoadData()
        {
            Settings.Load();
            excludedTypeBlocks = Settings.ExcludedBlockTypes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            excludedSubTypeBlocks = Settings.ExcludedBlockSubTypes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>())
            {
                if (def.Id.TypeId != typeof(MyObjectBuilder_CubeBlock) && !excludedTypeBlocks.Contains(def.Id.TypeId.ToString()) && !excludedSubTypeBlocks.Contains(def.Id.SubtypeId.ToString()))
                {
                    MyLog.Default.WriteLineAndConsole($"### Limited voxel placement of block={def.Id.TypeId} {def.Id.SubtypeId}");

                    def.VoxelPlacement = new VoxelPlacementOverride
                    {
                        DynamicMode = new VoxelPlacementSettings { PlacementMode = VoxelPlacementMode.OutsideVoxel },
                        StaticMode = new VoxelPlacementSettings { PlacementMode = VoxelPlacementMode.OutsideVoxel }
                    };
                }
            }
        }
    }

    public class PEPCO_Voxel_Settings
    {
        const string VariableId = nameof(PEPCO_Limited_Voxel_Placement);
        const string FileName = "PEPCO_Voxel_Settings.ini";
        const string IniSection = "Voxel_Settings";

        public string ExcludedBlockTypes = "MyObjectBuilder_BatteryBlock, MyObjectBuilder_Conveyor, MyObjectBuilder_ConveyorConnector";
        public string ExcludedBlockSubTypes = "BasicStaticDrill, AdvancedStaticDrill, StaticDrill, OakWoodLog, OakWoodPlank";

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
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(PEPCO_Voxel_Settings)))
            {
                using (var file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(PEPCO_Voxel_Settings)))
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

            using (var file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(PEPCO_Voxel_Settings)))
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
