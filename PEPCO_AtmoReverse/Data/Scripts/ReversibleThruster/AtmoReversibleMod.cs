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
using Digi;
using PEPCO.AtmoReversible.Sync;

namespace PEPCO.AtmoReversible
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class AtmoReversibleMod : MySessionComponentBase
    {
        private const string MOD_NAME = "Reversible Thrusters";

        public bool RealisticThrustersInstalled = false;
        public const ulong REALISTIC_THRUSTERS_MOD_ID = 575893643;
        public const string REALISTIC_THRUSTERS_LOCAL = "ImprovedThrusters.dev";

        public static AtmoReversibleMod Instance = null;

        public Networking Networking = new Networking(57575);

        public PacketBlockSettings CachedPacketSettings;

        public bool IsInit = false;
        public bool IsPlayer = false;

        public override void LoadData()
        {
            Instance = this;
            Log.ModName = MOD_NAME;
            Log.AutoClose = false;

            Networking.Register();

            CachedPacketSettings = new PacketBlockSettings();

            IsPlayer = !(MyAPIGateway.Session.IsServer && MyAPIGateway.Utilities.IsDedicated);

        }

        public override void BeforeStart()
        {
            IsInit = true;

            List<MyObjectBuilder_Checkpoint.ModItem> mods = MyAPIGateway.Session.Mods;

            foreach(MyObjectBuilder_Checkpoint.ModItem mod in mods)
            {
                if(mod.PublishedFileId == REALISTIC_THRUSTERS_MOD_ID || (mod.PublishedFileId == 0 && mod.Name == REALISTIC_THRUSTERS_LOCAL))
                {
                    RealisticThrustersInstalled = true;
                    Log.Info("Realistic Thrusters mod found, will adjust the thrust reversers accordingly.");
                    break;
                }
            }
        }

        protected override void UnloadData()
        {
            IsInit = false;
            RealisticThrustersInstalled = false;

            Networking?.Unregister();
            Networking = null;

            Log.Close();
            Instance = null;
        }
    }
}