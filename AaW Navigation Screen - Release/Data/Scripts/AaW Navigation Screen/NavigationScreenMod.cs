using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using Digi;
using PEPCO.Sync;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace PEPCO
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class NavigationScreenMod : MySessionComponentBase
    {
        private const string MOD_NAME = "AaW Navigation Screen";

        public static NavigationScreenMod Instance = null;

        // was  new Networking(57579);
        public Networking Networking = new Networking(17885);

        public PacketBlockSettings CachedPacketSettings;

        public bool IsInit = false;
        public bool IsPlayer = false;

        public readonly List<MyEntity> Entities = new List<MyEntity>();
        public readonly List<MyPlanet> Planets = new List<MyPlanet>();

        private Func<IMyEntity, bool> entityFilterCached;

        public override void LoadData()
        {
            Instance = this;
            Log.ModName = MOD_NAME;
            Log.AutoClose = true;

            Networking.Register();

            CachedPacketSettings = new PacketBlockSettings();

            IsPlayer = !(MyAPIGateway.Session.IsServer && MyAPIGateway.Utilities.IsDedicated);

            entityFilterCached = new Func<IMyEntity, bool>(EntityFilter);
            MyAPIGateway.Entities.GetEntities(null, entityFilterCached);
        }

        protected override void UnloadData()
        {
            IsInit = false;

            Networking?.Unregister();
            Networking = null;

            Log.Close();
            Instance = null;
        }
        public override void UpdateBeforeSimulation()
        {
            //Set update to none
            if (!IsInit)
            {
                IsInit = true;
                try
                {
                    MyAPIGateway.Entities.GetEntities(null, entityFilterCached);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            
        }


        private bool EntityFilter(IMyEntity ent)
        {
            var p = ent as MyPlanet;

            if (p != null)
                Planets.Add(p);

            return false; // don't add to the list, it's null
        }
    }
}
