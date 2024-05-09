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

namespace PEPONE.HeavyGasesModular
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class HeavyGasesModular : MySessionComponentBase
    {
        public HeavyGasesModularSettings Settings = new HeavyGasesModularSettings();


        public static bool enableNPCs = false;
        public static List<MyTuple<string, double>> gasList = new List<MyTuple<string, double>>();

        public const ulong HEAVY_GAS_MOD_ID = 2921890257;
        public static bool HeavyGasVanillaModInstalled = false;

        public static HeavyGasesModular Instance = null;


        public override void LoadData()
        {
            Instance = this;

            // Check if the Heavy Gas mod is installed to avoid overlaps
            List<MyObjectBuilder_Checkpoint.ModItem> mods = MyAPIGateway.Session.Mods;

            foreach (MyObjectBuilder_Checkpoint.ModItem mod in mods)
            {
                if (mod.PublishedFileId == HEAVY_GAS_MOD_ID)
                {
                    HeavyGasVanillaModInstalled = true;
                    Log.Info("Heavy Gas mod found, will adjust the procedures accordingly.");
                    break;
                }
            }

            //Triggers the load of the mod information
            Settings.Load(HeavyGasVanillaModInstalled);

            enableNPCs = Settings.enableNPCs;
            gasList = Settings.gasList;


        }

        public override void BeforeStart()
        {
            if (HeavyGasVanillaModInstalled)
            {
                Log.Info("You have both the Heavy Gas mod and the Heavy Gas Modular mod installed.\nThe Heavy Gas Modular mod will ignore your settings for Hydrogen and Oxygen to avoid compatibility issues.",
                    "You have both the Heavy Gas mod and the Heavy Gas Modular mod installed.\nThe Heavy Gas Modular mod will ignore your settings for Hydrogen and Oxygen to avoid compatibility issues.",
                    10000);
            }
        }



        public class HeavyGasesModularSettings
        {


            const string VariableId = nameof(HeavyGasesModularSettings); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)
            const string FileName = "HeavyGasesModular.ini"; // the file that gets saved to world storage under your mod's folder
            const string IniSection = "Config";

            public bool enableNPCs = false;


            public bool HeavyGasVanillaModInstalled = false;

            //The initial list of IO gases used to prefill the .ini
            public List<MyTuple<string, double>> gasList = new List<MyTuple<string, double>>()
            {
                MyTuple.Create("Gasoline",0.74),
                MyTuple.Create("RocketFuel",0.791),
                MyTuple.Create("Steam",0.0561597),
                MyTuple.Create("Deuterium",0.1624)
            };

            public HeavyGasesModularSettings()
            {

            }



            void LoadConfig(MyIni iniParser)
            {
                // Load whether NPCs are impacted
                enableNPCs = iniParser.Get(IniSection, nameof(enableNPCs)).ToBoolean(enableNPCs);


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
                                Log.Error($"This line could not be imported properly: {s}\nMake sure it is formatted properly as per the instructions in the .ini");
                                continue;
                            }

                            var tempTuple = MyTuple.Create(gasName, gasConversion);

                            // if the gas was found, we replace it with the one from the .ini
                            gasList.RemoveAll(x => x.Item1 == gasName.Trim());

                            
                                
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
                iniParser.Set(IniSection, nameof(enableNPCs), enableNPCs);

                var myarray = gasList.ToArray(x => x.Item1 + ";" + x.Item2);
                iniParser.Set(IniSection, nameof(gasList), string.Join("\n", myarray));
                iniParser.SetComment(IniSection, nameof(gasList), "Add the IDs of additional gases in separate lines starting with a '|'. Use a ';' delimiter here. E.g. \n|Hydrogen;0.01111 \n|Oxygen;0.17777");

            }

            public void Load(bool compatibility)
            {
                HeavyGasVanillaModInstalled = compatibility;
                if (MyAPIGateway.Session.IsServer)
                    LoadOnHost();
                else
                    LoadOnClient();
            }
            // This loads on either a server or a single player
            void LoadOnHost()
            {

                foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
                {
                    var gasDef = def as MyGasProperties;
                    if (gasDef != null)
                    {
                        var tempTuple = MyTuple.Create(gasDef.Id.SubtypeName, 0.0);

                        int index = gasList.FindIndex(x => x.Item1 == gasDef.Id.SubtypeName.Trim());

                        // if the index is -1, the gas was not found so we add it to the ini
                        if (index == -1)
                        {
                        // Here we add the gas to the gas list so it can be found by the tanks wanting to gain weight
                        gasList.Add(tempTuple);
                        }


                    }
                }

                MyIni iniParser = new MyIni();

                // load file if exists then save it regardless so that it can be sanitized and updated
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(HeavyGasesModularSettings)))
                {
                    using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(HeavyGasesModularSettings)))
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

                using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(HeavyGasesModularSettings)))
                {
                    file.Write(saveText);
                }


            }
            // This only loads on a server client
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


        private IMyGasTank tank;

        bool setupComplete = false;
        double massMultiplierModular = 0;

        bool isNPCOwned = false;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // this method is called async! always do stuff in the first update unless you're sure it must be in this one.
            // NOTE the objectBuilder arg is not the Entity's but the component's, and since the component wasn't loaded from an OB that means it's always null, which it is (AFAIK).
            base.Init(objectBuilder);
            tank = (IMyGasTank)Entity;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME; 
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            
                if (tank?.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                    return;

                MyGasTankDefinition tankDef = (MyGasTankDefinition)tank.SlimBlock.BlockDefinition;

                // Check if the tank has a block definition
                if (tankDef != null)
                {

                    string searchString = tankDef.StoredGasId.ToString();
                try
                {
                    // Heavy Gas installed and not set to overwrite
                    if (HeavyGasesModular.HeavyGasVanillaModInstalled)
                    {
                        if (searchString == "MyObjectBuilder_GasProperties/Hydrogen" || searchString == "MyObjectBuilder_GasProperties/Oxygen")
                        {
                            //This is to ignore hydrogen and oxygen since they are managed by the Heavy Gas vanilla mod
                            return;

                        }
                    }
                    
                    // This means that either Heavy Gas isn't installed or that the tank is not an h2 / o2 tank
                    var gasMultiplier = HeavyGasesModular.gasList.Find(x => "MyObjectBuilder_GasProperties/" + x.Item1 == searchString);
                    massMultiplierModular = gasMultiplier.Item2;
                }

                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                NeedsUpdate = massMultiplierModular > 0f ? MyEntityUpdateEnum.EACH_FRAME : MyEntityUpdateEnum.NONE;

            }
            
            
        }

        private void CheckIfNPCOwned(IMyCubeGrid grid)
        {
            isNPCOwned = true;
            foreach (var owner in grid.BigOwners)
            {
                if (owner == 0)
                    continue;

                if (MyAPIGateway.Players.TryGetSteamId(owner) > 0)
                    isNPCOwned = false;
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

            if (setupComplete == false)
            {
                tank.CubeGrid.OnBlockOwnershipChanged += CheckIfNPCOwned;
                tank.CubeGrid.OnGridSplit += OnGridSplit;
                CheckIfNPCOwned(tank.CubeGrid);

                // Update every 100 frames only from now on
                NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
                setupComplete = true;
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            MyInventory inv = (MyInventory)tank.GetInventory();
            MyFixedPoint newExternalMass = (MyFixedPoint)((tank.FilledRatio * tank.Capacity) * massMultiplierModular);


            // disable extra mass for NPC grids, if needed
            if (HeavyGasesModular.enableNPCs == false && isNPCOwned == true)
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