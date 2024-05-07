using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Lights;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using Sandbox.Game.EntityComponents;
using Sandbox.Game;
using System.IO;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage;
using Digi;
using Sandbox.Game.Entities.Blocks;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using VRage.Game.ObjectBuilders.Definitions;
using System.Security.Cryptography;

namespace PEPONE.HeavyIOGases
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class HeavyGasolineSession : MySessionComponentBase
    {
        public HeavyIOGasSettings Settings = new HeavyIOGasSettings();


        public static bool EnableNPCs = false;

        public static List<MyTuple<string, double>> gasList = new List<MyTuple<string, double>>();

        public static List<string> gasListString = new List<string>();

        public override void LoadData()
        {

            


            //Triggers the load of the mod information
            Settings.Load();
            EnableNPCs = Settings.EnableNPCs;

            


            gasList = Settings.gasList;

        }

public class HeavyIOGasSettings
        {
            //The initial list of IO gases
            public List<MyTuple<string, double>> gasList = new List<MyTuple<string, double>>()
            {
                MyTuple.Create("Gasoline",0.74),
                MyTuple.Create("RocketFuel",0.791),
                MyTuple.Create("Steam",0.0561597),
                MyTuple.Create("Deuterium",0.1624)
            };

            

            const string VariableId = nameof(HeavyIOGasSettings); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)
            const string FileName = "HeavyIOGas.ini"; // the file that gets saved to world storage under your mod's folder
            const string IniSection = "Config";

            public bool EnableNPCs = false;




            public HeavyIOGasSettings()
            {

            }

            void LoadConfig(MyIni iniParser)
            {
                // Load whether NPCs are impacted
                EnableNPCs = iniParser.Get(IniSection, nameof(EnableNPCs)).ToBoolean(EnableNPCs);


                //Because errors can and do happen
                try 
                {
                    //Get the gas list from the ini and split it to an array
                    var configList = iniParser.Get(IniSection, nameof(gasList)).ToString().Trim().Split('\n');


                    foreach (string s in configList)
                    {
                        // Get the gas name from the ini
                        string gasName = s.Split(';')[0];

                        // Try and get rid of flukes by making sure the gas has a name
                        if (gasName.Length > 0)
                        {
                            //Get the gas conversion from the ini
                            double gasConversion = 0;
                            
                            // If the second part cannot be parsed as a double, the import of the gas is skipped
                            if (!double.TryParse(s.Split(';')[1], out gasConversion))
                            {
                                Log.Error($"This line could not be imported properly: {s} make sure it is formatted properly as per the instructions in the .ini");
                                continue;
                            }

                            var tempTuple = MyTuple.Create(gasName, gasConversion);

                            int index = gasList.FindIndex(x => x.Item1 == gasName.Trim());

                            // if the index is not -1, it was found so we replace the gas with the one from the .ini
                            if (index != -1)
                            {
                                gasList.RemoveAt(index);
                            }
                                
                            // Here we add the gas to the gas list so it can be found by the tanks wanting to gain weight
                            gasList.Add(tempTuple);
                            
                        }

                    }
                } catch (Exception ex) 
                { 
                    Log.Error(ex);
                }

            }

            void SaveConfig(MyIni iniParser)
            {
                // Define whether NPCs are impacted
                iniParser.Set(IniSection, nameof(EnableNPCs), EnableNPCs);

                var myarray = gasList.ToArray(x => x.Item1 + ";" + x.Item2);
                iniParser.Set(IniSection, nameof(gasList), string.Join("\n", myarray));
                iniParser.SetComment(IniSection, nameof(gasList), "Add the IDs of additional gases in separate lines starting with a '|'. Use a ';' delimiter here. E.g. \n|Hydrogen;0.01111 \n|Oxygen;0.17777");

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

                foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
                {
                    var gasDef = def as MyGasProperties;
                    if (gasDef != null)
                    {
                        Log.Info($"found this: " +
                            $"\ngasDef.Id: {gasDef.Id}" +
                            $"\n gasDef.Id.SubtypeName {gasDef.Id.SubtypeName}");
                        var tempTuple = MyTuple.Create(gasDef.Id.SubtypeName, 0.0);

                        int index = gasList.FindIndex(x => x.Item1 == gasDef.Id.SubtypeName.Trim());

                        // if the index is not -1, it was found so we replace the gas with the one from the .ini
                        if (index != -1)
                        {
                            gasList.RemoveAt(index);
                        }

                        // Here we add the gas to the gas list so it can be found by the tanks wanting to gain weight
                        gasList.Add(tempTuple);

                    }
                }

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

                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("ModConfig.xml", typeof(HeavyIOGasSettings)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(gasList));
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

    // This object gets attached to entities depending on their type and optionally subtype aswell.
    // The 2nd arg, "false", is for entity-attached update if set to true which is not recommended, see for more info: https://forum.keenswh.com/threads/modapi-changes-jan-26.7392280/
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenTank), false)]
    public class HeavyGas : MyGameLogicComponent
    {
        // A molecule of water weights 18 atomic mass units
        // .. 2 of which are hydrogen
        // .. and 16 are oxygen
        // in SE, turning 1kg of Ice into gas results in 10L of Hydrogen, and 5L of Oxygen, scale the values accordingly

        private IMyGasTank tank;

        bool SetupComplete = false;
        double massMultiplierIO = 0;

        bool NPCOwned = false;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // this method is called async! always do stuff in the first update unless you're sure it must be in this one.
            // NOTE the objectBuilder arg is not the Entity's but the component's, and since the component wasn't loaded from an OB that means it's always null, which it is (AFAIK).
            base.Init(objectBuilder);
            tank = (IMyGasTank)Entity;

            MyGasTankDefinition tankDef = (MyGasTankDefinition)tank.SlimBlock.BlockDefinition;

            // Check if the tank has a block definition
            if (tankDef != null)
            {

                string searchString = tankDef.StoredGasId.ToString();

                var gasMultiplier = HeavyGasolineSession.gasList.Find(x => "MyObjectBuilder_GasProperties/"+x.Item1 == searchString);
                        massMultiplierIO = gasMultiplier.Item2;
                //break;
                Log.Info($"Look! I found this nice tank: {searchString} and it gave me the mass multiplier {massMultiplierIO}");

                //}
            }


            NeedsUpdate = massMultiplierIO > 0f ? MyEntityUpdateEnum.EACH_FRAME : MyEntityUpdateEnum.NONE;
        }

        private void CheckIfNPCOwned(IMyCubeGrid grid)
        {
            NPCOwned = true;
            foreach (var owner in grid.BigOwners)
            {
                if (owner == 0)
                    continue;

                if (MyAPIGateway.Players.TryGetSteamId(owner) > 0)
                    NPCOwned = false;
            }
        }

        private void OnGridSplit(IMyCubeGrid arg1, IMyCubeGrid arg2)
        {
            // stop listening for events on split grids
            arg1.OnBlockOwnershipChanged -= CheckIfNPCOwned;
            arg1.OnGridSplit -= OnGridSplit;
            arg2.OnBlockOwnershipChanged -= CheckIfNPCOwned;
            arg2.OnGridSplit -= OnGridSplit;

            // and continue listening on the tanks grid
            tank.CubeGrid.OnBlockOwnershipChanged += CheckIfNPCOwned;
            tank.CubeGrid.OnGridSplit += OnGridSplit;

            // .. and update ownership now
            CheckIfNPCOwned(tank.CubeGrid);
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();

            if (SetupComplete == false)
            {
                tank.CubeGrid.OnBlockOwnershipChanged += CheckIfNPCOwned;
                tank.CubeGrid.OnGridSplit += OnGridSplit;
                CheckIfNPCOwned(tank.CubeGrid);

                // Update every 100 frames only from now on
                NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
                SetupComplete = true;
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            MyInventory inv = (MyInventory)tank.GetInventory();
            MyFixedPoint newExternalMass = (MyFixedPoint)((tank.FilledRatio * tank.Capacity) * massMultiplierIO);


            // disable extra mass for NPC grids, if needed
            if (HeavyGasolineSession.EnableNPCs == false && NPCOwned == true)
            {
                newExternalMass = 0;
            }
            // update external mass
            if (inv != null && newExternalMass != inv.ExternalMass)
            {
                inv.ExternalMass = newExternalMass;
                inv.Refresh();
            }
        }
    }
}