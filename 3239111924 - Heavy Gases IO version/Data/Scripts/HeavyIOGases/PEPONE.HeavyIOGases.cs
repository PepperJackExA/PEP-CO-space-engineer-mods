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

namespace PEPONE.HeavyIOGases
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class HeavyGasolineSession : MySessionComponentBase
    {
        HeavyIOGasSettings Settings = new HeavyIOGasSettings();


        public static bool EnableNPCs = false;
        public static double GAS_L_KG_CONVERSION_Gasoline = 0;
        public static double GAS_L_KG_CONVERSION_RocketFuel = 0;
        public static double GAS_L_KG_CONVERSION_Steam = 0;
        public static double GAS_L_KG_CONVERSION_Deuterium = 0;

        public override void LoadData()
        {
            //Triggers the load of the mod information
            Settings.Load();
            EnableNPCs = Settings.EnableNPCs;
            GAS_L_KG_CONVERSION_Gasoline = Settings.GAS_L_KG_CONVERSION_Gasoline;
            GAS_L_KG_CONVERSION_RocketFuel = Settings.GAS_L_KG_CONVERSION_RocketFuel;
            GAS_L_KG_CONVERSION_Steam = Settings.GAS_L_KG_CONVERSION_Steam;
            GAS_L_KG_CONVERSION_Deuterium = Settings.GAS_L_KG_CONVERSION_Deuterium;
        }

        class HeavyIOGasSettings
        {
            const string VariableId = nameof(HeavyIOGasSettings); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)
            const string FileName = "HeavyIOGas.ini"; // the file that gets saved to world storage under your mod's folder
            const string IniSection = "Config";

            public bool EnableNPCs = false;
            public double GAS_L_KG_CONVERSION_Gasoline = 0.74; // The specific gravity of gasoline ranges from 0.71 to 0.77
            public double GAS_L_KG_CONVERSION_RocketFuel = 0.791; //Density of Unsymmetrical dimethylhydrazine as 22°C
            public double GAS_L_KG_CONVERSION_Steam = 0.0561597; //Density at 100 bar & 311.74 °C have fun changing it if you don't like it
            public double GAS_L_KG_CONVERSION_Deuterium = 0.1624; // Density of liquid deuterium at 18 K (Yeah, I know quite cold eh?)

            public HeavyIOGasSettings()
            {

            }

            void LoadConfig(MyIni iniParser)
            {
                // Load whether NPCs are impacted
                EnableNPCs = iniParser.Get(IniSection, nameof(EnableNPCs)).ToBoolean(EnableNPCs);

                // Load the other mass coversion factors; default to the above values if anything goes wrong
                GAS_L_KG_CONVERSION_Gasoline = iniParser.Get(IniSection, nameof(GAS_L_KG_CONVERSION_Gasoline)).ToDouble(GAS_L_KG_CONVERSION_Gasoline);
                GAS_L_KG_CONVERSION_RocketFuel = iniParser.Get(IniSection, nameof(GAS_L_KG_CONVERSION_RocketFuel)).ToDouble(GAS_L_KG_CONVERSION_RocketFuel);
                GAS_L_KG_CONVERSION_Steam = iniParser.Get(IniSection, nameof(GAS_L_KG_CONVERSION_Steam)).ToDouble(GAS_L_KG_CONVERSION_Steam);
                GAS_L_KG_CONVERSION_Deuterium = iniParser.Get(IniSection, nameof(GAS_L_KG_CONVERSION_Deuterium)).ToDouble(GAS_L_KG_CONVERSION_Deuterium);
            }

            void SaveConfig(MyIni iniParser)
            {
                // Define whether NPCs are impacted
                iniParser.Set(IniSection, nameof(EnableNPCs), EnableNPCs);

                // Gasoline settings
                iniParser.Set(IniSection, nameof(GAS_L_KG_CONVERSION_Gasoline), GAS_L_KG_CONVERSION_Gasoline);
                iniParser.SetComment(IniSection, nameof(GAS_L_KG_CONVERSION_Gasoline), "This is the gasoline mass factor in Kg / l");

                // Rocket fuel settings
                iniParser.Set(IniSection, nameof(GAS_L_KG_CONVERSION_RocketFuel), GAS_L_KG_CONVERSION_RocketFuel);
                iniParser.SetComment(IniSection, nameof(GAS_L_KG_CONVERSION_RocketFuel), "This is the rocket fuel mass factor in Kg / l");

                // Steam settings
                iniParser.Set(IniSection, nameof(GAS_L_KG_CONVERSION_Steam), GAS_L_KG_CONVERSION_Steam);
                iniParser.SetComment(IniSection, nameof(GAS_L_KG_CONVERSION_Steam), "This is the steam mass factor in Kg / l");

                // Deuterium settings
                iniParser.Set(IniSection, nameof(GAS_L_KG_CONVERSION_Deuterium), GAS_L_KG_CONVERSION_Deuterium);
                iniParser.SetComment(IniSection, nameof(GAS_L_KG_CONVERSION_Deuterium), "This is the deuterium mass factor in Kg / l");
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
                // Find out what type of tank this is and set multiplier accordingly
                switch (tankDef.StoredGasId.ToString())
                {
                    case "MyObjectBuilder_GasProperties/Gasoline":
                        massMultiplierIO = HeavyGasolineSession.GAS_L_KG_CONVERSION_Gasoline;
                        break;
                    case "MyObjectBuilder_GasProperties/RocketFuel":
                        massMultiplierIO = HeavyGasolineSession.GAS_L_KG_CONVERSION_RocketFuel;
                        break;
                    case "MyObjectBuilder_GasProperties/Steam":
                        massMultiplierIO = HeavyGasolineSession.GAS_L_KG_CONVERSION_Steam;
                        break;
                    case "MyObjectBuilder_GasProperties/Deuterium":
                        massMultiplierIO = HeavyGasolineSession.GAS_L_KG_CONVERSION_Deuterium;
                        break;
                    default:
                        //Maybe add a message for gases that were not found.
                        break;

                }
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
