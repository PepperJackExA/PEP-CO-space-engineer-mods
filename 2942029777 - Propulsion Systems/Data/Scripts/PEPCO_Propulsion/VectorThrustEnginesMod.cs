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

namespace PEPCO_Propulsion
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class VectorThrustEnginesMod : MySessionComponentBase
    {
        private const string MOD_NAME = "PEPCO_Propulsion";

        public static VectorThrustEnginesMod Instance = null;

        public Networking Networking = new Networking(48293);

        public PacketBlockSettings CachedPacketSettings;

        public bool IsInit = false;
        public bool IsPlayer = false;

        public override void LoadData()
        {
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