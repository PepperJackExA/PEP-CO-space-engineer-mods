using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using PEPCO_Propulsion.Sync;
using Digi;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI.Interfaces;
using VRage;

namespace PEPCO_Propulsion
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]


    public class VectorThrustEnginesMod : MySessionComponentBase
    {
        public class ShipPropellerDefinitions
        {

            public Func<float, float> efficiencyFunction;

            public int maxRPM;

            public Vector3D screwPostion;

            public bool canVector;

        }

        private const string MOD_NAME = "PEPCO_Propulsion";

        public static VectorThrustEnginesMod Instance = null;

        public Networking Networking = new Networking(48293);

        public PacketBlockSettings CachedPacketSettings;

        public bool IsInit = false;
        public bool IsPlayer = false;

        public Dictionary<string, ShipPropellerDefinitions> mountingsList = new Dictionary<string, ShipPropellerDefinitions>();

        public override void LoadData()
        {
            //fill the dictionary with the mountings
            mountingsList.Add("HighRPMMounting", new ShipPropellerDefinitions()
            {
                efficiencyFunction = (float x) => { return (float)Math.Pow(x, 3.0); }, // efficiency for calculating thrust output
                maxRPM = 200, // max RPM for the propeller
                screwPostion = new Vector3D(0, 1.075, 0), // position of the screw relative to the block
                canVector = false // can the propeller vector
            });
            mountingsList.Add("LowRPMMounting", new ShipPropellerDefinitions()
            {
                efficiencyFunction = (float x) => { return (float)(0.65 * Math.Pow(x, (1.0 / 3.0))); }, // efficiency for calculating thrust output
                maxRPM = 120, // max RPM for the propeller
                screwPostion = new Vector3D(0, 1.075, 0), // position of the screw relative to the block
                canVector = false // can the propeller vector
            });


            Instance = this;
            Log.ModName = MOD_NAME;
            Log.AutoClose = true;

            Networking.Register();

            CachedPacketSettings = new PacketBlockSettings();

            IsPlayer = !(MyAPIGateway.Session.IsServer && MyAPIGateway.Utilities.IsDedicated);

            

        }

        protected override void UnloadData()
        {
            IsInit = false;

            Networking?.Unregister();
            Networking = null;

            Log.Close();
            Instance = null;
        }
    }
}