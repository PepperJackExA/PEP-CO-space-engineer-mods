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
using Digi;
using PEPCO.AtmoReversible.Sync;
using Sandbox.Game.EntityComponents;

namespace PEPCO.AtmoReversible
{

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false,
                                 "PEPCO_AtmoReverse")]
    public class ThrustBlock : MyGameLogicComponent
    {

        MyThrust thrust;
        IMyThrust block;

        public readonly Guid SETTINGS_GUID = new Guid("0DFAA570-310D-4D1C-DDCC-C57913E20389");

        public readonly AtmoReversibleBlockSettings Settings = new AtmoReversibleBlockSettings();

        public bool Thrust_ReverseToggle 
        {  
            get {  return Settings.Thrust_ReverseToggle ; } 
            
            set 
            {
                Settings.Thrust_ReverseToggle = value ;
                SaveSettings();
            }
        }
        


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            block = (IMyThrust)Entity;
        }

        public override void UpdateOnceBeforeFrame()
        {
            AtmoReversibleControls.DoOnce(ModContext);

            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;

            Settings.Thrust_ReverseToggle = false;

            LoadSettings();

            SaveSettings(); // required for IsSerialized()

        }

        public override void UpdateBeforeSimulation()
        {

            try
            {
                if (AtmoReversibleMod.Instance == null || !AtmoReversibleMod.Instance.IsPlayer)
                    return;

                thrust = (MyThrust)Entity;
                MyCubeGrid grid = thrust.CubeGrid;

                if (grid?.Physics == null || !grid.Physics.Enabled)
                    return;

                if (Thrust_ReverseToggle && thrust.CurrentStrength > 0)
                {
                    Vector3D force = thrust.WorldMatrix.Forward * thrust.BlockDefinition.ForceMagnitude * thrust.CurrentStrength * 1.75;
                    Vector3D forceAt = grid.Physics.CenterOfMassWorld;
                    grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, force, forceAt, null);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        void LoadSettings()
        {
            if (block.Storage == null)
            {
                return;
            }


            string rawData;
            if (!block.Storage.TryGetValue(SETTINGS_GUID, out rawData))
            {
                return;
            }



            try
            {
                var loadedSettings = MyAPIGateway.Utilities.SerializeFromBinary<AtmoReversibleBlockSettings>(Convert.FromBase64String(rawData));

                if (loadedSettings != null)
                {
                    Settings.Thrust_ReverseToggle = loadedSettings.Thrust_ReverseToggle;
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error loading settings!\n{e}");
            }

            return;
        }


        void SaveSettings()
        {
            if (block == null)
                return; // called too soon or after it was already closed, ignore

            if (Settings == null)
                throw new NullReferenceException($"Settings == null on entId={Entity?.EntityId}; modInstance={AtmoReversibleMod.Instance != null}");

            if (MyAPIGateway.Utilities == null)
                throw new NullReferenceException($"MyAPIGateway.Utilities == null; entId={Entity?.EntityId}; modInstance={AtmoReversibleMod.Instance != null}");

            if (block.Storage == null)
                block.Storage = new MyModStorageComponent();

            block.Storage.SetValue(SETTINGS_GUID, Convert.ToBase64String(MyAPIGateway.Utilities.SerializeToBinary(Settings)));
        }

        public override void UpdateBeforeSimulation10()
        {
            try
            {
                SyncSettings();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }


        void SyncSettings()
        {
                SaveSettings();

                AtmoReversibleMod.Instance.CachedPacketSettings.Send(block.EntityId, Settings);

        }

        public override bool IsSerialized()
        {
            // called when the game iterates components to check if they should be serialized, before they're actually serialized.
            // this does not only include saving but also streaming and blueprinting.
            // NOTE for this to work reliably the MyModStorageComponent needs to already exist in this block with at least one element.

            try
            {
                SaveSettings();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return base.IsSerialized();
        }

    }
}
