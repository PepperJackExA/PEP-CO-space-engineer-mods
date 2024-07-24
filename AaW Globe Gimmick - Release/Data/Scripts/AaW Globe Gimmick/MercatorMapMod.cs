using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using Digi;
using PEPCO.Sync;

namespace PEPCO
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MercatorMapMod : MySessionComponentBase
    {
        private const string MOD_NAME = "AaW Mercator Map";

        public static MercatorMapMod Instance = null;

        // was  new Networking(57579);
        public Networking Networking = new Networking(17885);

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
