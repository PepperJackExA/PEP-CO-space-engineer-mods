using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities; // this ingame namespace is safe to use in mods as it has nothing to collide with
using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
using VRage.Utils;

namespace PEPCO_Limited_Voxel_Placement
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Limited_Voxel_Placement : MySessionComponentBase
    {
        PEPCO_Voxel_Settings Settings = new PEPCO_Voxel_Settings();

        public List<string> excludedTypeBlocks;
        public List<string> excludedSubTypeBlocks;

        public override void LoadData()
        {

            Settings.Load();

            excludedTypeBlocks = Settings.ExcludedBlockTypes.Split(' ', ',').ToList();

            excludedSubTypeBlocks = Settings.ExcludedBlockSubTypes.Split(' ', ',').ToList();

            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions()) // Loop through all blocks
            {
                var blockDef = def as MyCubeBlockDefinition;
                if (blockDef != null && blockDef.Id.TypeId != typeof(MyObjectBuilder_CubeBlock)) // Filter out all blocks 
                {


                    var typeitem = excludedTypeBlocks.Find(x => x == blockDef.Id.TypeId.ToString()); // Find block in the exclusion list
                    var subtypeitem = excludedSubTypeBlocks.Find(x => x == blockDef.Id.SubtypeId.ToString()); // Find block in the exclusion list

                    if (typeitem == null && subtypeitem == null) // Apply voxel limitation if block wasn't found
                    {
                        MyLog.Default.WriteLineAndConsole($"### Limited voxel placement of block={blockDef.Id.TypeId.ToString()} {blockDef.Id.SubtypeId.ToString()}");

                        blockDef.VoxelPlacement = new VoxelPlacementOverride()
                        {
                            DynamicMode = new VoxelPlacementSettings()
                            {
                                PlacementMode = VoxelPlacementMode.OutsideVoxel,
                            },
                            StaticMode = new VoxelPlacementSettings()
                            {
                                PlacementMode = VoxelPlacementMode.OutsideVoxel,
                            },
                        };
                    }


                }
            }
        }
    }

    public class PEPCO_Voxel_Settings
    {
        const string VariableId = nameof(PEPCO_Limited_Voxel_Placement); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)
        const string FileName = "PEPCO_Voxel_Settings.ini"; // the file that gets saved to world storage under your mod's folder
        const string IniSection = "Voxel_Settings";

        // settings you'd be reading, and their defaults.
        public string ExcludedBlockTypes = "MyObjectBuilder_BatteryBlock, MyObjectBuilder_Conveyor, MyObjectBuilder_ConveyorConnector";
        public string ExcludedBlockSubTypes = "BasicStaticDrill, AdvancedStaticDrill, StaticDrill, OakWoodLog, OakWoodPlank";

        void LoadConfig(MyIni iniParser)
        {
            // repeat for each setting field
            ExcludedBlockTypes = iniParser.Get(IniSection, nameof(ExcludedBlockTypes)).ToString(ExcludedBlockTypes);
            ExcludedBlockSubTypes = iniParser.Get(IniSection, nameof(ExcludedBlockSubTypes)).ToString(ExcludedBlockSubTypes);

        }

        void SaveConfig(MyIni iniParser)
        {
            // repeat for each setting field
            iniParser.Set(IniSection, nameof(ExcludedBlockTypes), ExcludedBlockTypes);
            iniParser.SetComment(IniSection, nameof(ExcludedBlockTypes), "This is a list of all excluded block types. They can be placed in voxels"); // optional

            // repeat for each setting field
            iniParser.Set(IniSection, nameof(ExcludedBlockSubTypes), ExcludedBlockSubTypes);
            iniParser.SetComment(IniSection, nameof(ExcludedBlockSubTypes), "This is a list of all excluded block subtypes. They can be placed in voxels"); // optional

        }

        // nothing to edit below this point

        public PEPCO_Voxel_Settings()
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

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(PEPCO_Voxel_Settings)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(PEPCO_Voxel_Settings)))
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

            using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(PEPCO_Voxel_Settings)))
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