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
using Sandbox.Game.EntityComponents;
using VectorThrustEngines.Data.Scripts.PEPONE_Vector_Thrusters;
using static VRageMath.Base6Directions;
using static VRageMath.Base27Directions;

namespace PEPCO.VectorThrustEngines
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false,
                                 "SmallBlockLargeAtmosphericThrust")]
    public class ThrustBlock : MyGameLogicComponent
    {

        MyThrust thrust;
        IMyThrust block;

        public const float AngleMin = -90;
        public const float AngleMax = +90;

        public readonly Guid SETTINGS_GUID = new Guid("0DFAA570-310D-D1C1-DDCC-C57913E20389");

        public readonly VectorThrustEnginesBlockSettings Settings = new VectorThrustEnginesBlockSettings();
        int syncCountdown;
        public const int SETTINGS_CHANGED_COUNTDOWN = (60 * 1) / 10;

        VectorThrustEnginesMod Mod => VectorThrustEnginesMod.Instance;

        public bool VectorThrust_Toggle
        {
            get { return Settings.VectorThrust_Toggle; }

            set
            {
                Settings.VectorThrust_Toggle = value;
                SettingsChanged();
            }
        }

        public bool VectorThrustReverse_Toggle
        {
            get { return Settings.VectorThrustReverse_Toggle; }

            set
            {
                Settings.VectorThrustReverse_Toggle = value;
                SettingsChanged();
            }
        }

        public float VectorThrust_Angle
        {
            get { return Settings.VectorThrust_Angle; }
            set
            {
                Settings.VectorThrust_Angle = MathHelper.Clamp(value, AngleMin, AngleMax);

                SettingsChanged();

            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            block = (IMyThrust)Entity;
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

        void SettingsChanged()
        {
            if (syncCountdown == 0)
                syncCountdown = SETTINGS_CHANGED_COUNTDOWN;
        }

        void SyncSettings()
        {
            if (syncCountdown > 0 && --syncCountdown <= 0)
            {
                SaveSettings();

                Mod.CachedPacketSettings.Send(block.EntityId, Settings);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            VectorThrustEnginesControls.DoOnce(ModContext);

            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;

            Settings.VectorThrust_Toggle = false;
            Settings.VectorThrustReverse_Toggle = false;
            Settings.VectorThrust_Angle = 0;

            LoadSettings();

            SaveSettings(); // required for IsSerialized()

        }

        public override void UpdateBeforeSimulation()
        {

            try
            {
                if (VectorThrustEnginesMod.Instance == null || !VectorThrustEnginesMod.Instance.IsPlayer)
                    return;

                thrust = (MyThrust)Entity;
                MyCubeGrid grid = thrust.CubeGrid;

                if (grid?.Physics == null || !grid.Physics.Enabled)
                    return;

                if (VectorThrust_Toggle && thrust.CurrentStrength > 0)
                {
                    // Get the relevant directions and axis
                    Vector3D axis = thrust.WorldMatrix.Left;
                    Vector3D direction = thrust.WorldMatrix.Backward;

                    // Rotate around the axis by degrees
                    MatrixD rotate = MatrixD.CreateFromAxisAngle(axis, MathHelper.ToRadians(VectorThrust_Angle));
                    Vector3D rotated = Vector3D.TransformNormal(direction, rotate);

                    // THis is where the forces will be applied
                    Vector3D forceAt = grid.Physics.CenterOfMassWorld;

                    // Assemble the force vectors
                    Vector3D counterForce = thrust.WorldMatrix.Forward * thrust.BlockDefinition.ForceMagnitude * thrust.CurrentStrength * 1; // Counters out the normal thrust force
                    grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, counterForce, forceAt, null);

                    Vector3D newThrustForce = rotated * thrust.BlockDefinition.ForceMagnitude * thrust.CurrentStrength;
                    
                    grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, newThrustForce, forceAt, null);
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
                var loadedSettings = MyAPIGateway.Utilities.SerializeFromBinary<VectorThrustEnginesBlockSettings>(Convert.FromBase64String(rawData));

                if (loadedSettings != null)
                {
                    Settings.VectorThrust_Toggle = loadedSettings.VectorThrust_Toggle;
                    Settings.VectorThrustReverse_Toggle = loadedSettings.VectorThrustReverse_Toggle;
                    Settings.VectorThrust_Angle = loadedSettings.VectorThrust_Angle;
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
                throw new NullReferenceException($"Settings == null on entId={Entity?.EntityId}; modInstance={VectorThrustEnginesMod.Instance != null}");

            if (MyAPIGateway.Utilities == null)
                throw new NullReferenceException($"MyAPIGateway.Utilities == null; entId={Entity?.EntityId}; modInstance={VectorThrustEnginesMod.Instance != null}");

            if (block.Storage == null)
                block.Storage = new MyModStorageComponent();

            block.Storage.SetValue(SETTINGS_GUID, Convert.ToBase64String(MyAPIGateway.Utilities.SerializeToBinary(Settings)));
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