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
using static VRageMath.Base6Directions;
using static VRageMath.Base27Directions;
using VRage.Game.Entity;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage;
using Sandbox.ModAPI.Interfaces;
using System.Text;
using Jakaria.API;
using Sandbox.Game.Entities.Cube;

namespace PEPCO_Propulsion
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false,
                                    "ElectricShipEngineModule")]
    public class PEPCOWaterThrustBlock : MyGameLogicComponent
    {

        MyThrust thrust;
        IMyThrust block;

        public const float AngleMin = -90;
        public const float AngleMax = +90;

        public readonly Guid SETTINGS_GUID = new Guid("0DFAA570-310D-D1C1-DDCC-C57913E20389");

        public readonly VectorThrustEnginesBlockSettings Settings = new VectorThrustEnginesBlockSettings();
        int syncCountdown;
        public const int SETTINGS_CHANGED_COUNTDOWN = (60 * 1) / 10;

        IMyUpgradeModule linkedMounting; // the drive shaft that this block is mounted on
        Func<float, float> efficiencyCalculation; // the function that calculates the efficiency of the mounting
        int maxRPM; // the maximum RPM of the mounting required for the shaft to spin
        Vector3D screwPosition; // the position of the screw relative to the block
        bool isUnderwater; // is the screw underwater


        MyTuple<float,float> infoOutput; // output efficiency and force to the custom info

        byte linkSkip = 127; // link ASAP

        private const string SUBPART_NAME = "DriveShaft"; // dummy name without the "subpart_" prefix
        //private const float DEGREES_PER_TICK = 9.0f; // rotation per tick in degrees (60 ticks per second)
        private const float ACCELERATE_PERCENT_PER_TICK = 0.05f; // aceleration percent of "DEGREES_PER_TICK" per tick.
        private const float DEACCELERATE_PERCENT_PER_TICK = 0.01f; // deaccleration percent of "DEGREES_PER_TICK" per tick.
        private readonly Vector3 ROTATION_AXIS = Vector3.Forward; // rotation axis for the subpart, you can do new Vector3(0.0f, 0.0f, 0.0f) for custom values
        private const float MAX_DISTANCE_SQ = 1000 * 1000; // player camera must be under this distance (squared) to see the subpart spinning

        private bool subpartFirstFind = true;
        private Matrix subpartLocalMatrix; // keeping the matrix here because subparts are being re-created on paint, resetting their orientations
        private float targetSpeedMultiplier; // used for smooth transition

        VectorThrustEnginesMod Mod => VectorThrustEnginesMod.Instance;

        public bool VectorThrustReverse_Toggle
        {
            get { return Settings.VectorThrustReverse_Toggle; }

            set
            {
                Settings.VectorThrustReverse_Toggle = value;
                SettingsChanged();
            }
        }


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            block = (IMyThrust)Entity;

            block.AppendingCustomInfo += CustomInfo;

        }

        private void CustomInfo(IMyTerminalBlock block, StringBuilder builder)
        {
            if (VectorThrustEnginesMod.Instance == null || !VectorThrustEnginesMod.Instance.IsPlayer)
                return;

            if (block?.CubeGrid?.Physics == null)
                return;

            if (linkedMounting != null)
            {
                builder.Append($"Attached mounting: {linkedMounting?.DefinitionDisplayNameText}\n" +
                    $"Mounting: {(linkedMounting.IsWorking ? "Working" : "Not working")}\n" +
                    $"Screw underwater: {isUnderwater}\n" +
                    $"RPM: {(thrust.CurrentStrength * maxRPM):N0}\n" +
                    $"Output efficiency: {infoOutput.Item1 * 100:N0}%\n" +
                    $"Output force: {infoOutput.Item2:N0}Nm");
                return;
            }
            builder.Append($"Attach a mounting for this engine to work.");

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
            //Look into overwriting the thrustoverride terminal control

            if (WaterModAPI.Registered)
            {
                thrust = (MyThrust)Entity;

                VectorThrustEnginesControls.DoOnce(ModContext);

                Settings.VectorThrust_Toggle = false;
                Settings.VectorThrustReverse_Toggle = false;
                Settings.VectorThrust_Angle = 0;

                LoadSettings();
                SaveSettings(); // required for IsSerialized()

                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            }
            else
            {
                Log.Info("WaterModAPI not registered");
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (VectorThrustEnginesMod.Instance == null || !VectorThrustEnginesMod.Instance.IsPlayer)
                return;

            thrust = (MyThrust)Entity;
            MyCubeGrid grid = thrust.CubeGrid;

            if (grid?.Physics == null || !grid.Physics.Enabled)
                return;

            try
            {
                if (linkedMounting == null) //Check for mounting
                {
                    if (++linkSkip >= 60) //Check every 60 frames
                    {
                        linkSkip = 0;
                        Vector3I pos = grid.WorldToGridInteger(thrust.WorldMatrix.Translation + thrust.WorldMatrix.Forward * grid.GridSize *2); //Delta of blocks found empirically

                        //

                        //Get all blocks on the grid
                        //var allBlocks = grid.GetFatBlocks();

                        ////Iterate all blocks and return the first block that is a mounting
                        //foreach (var testBlock in allBlocks)
                        //{
                        //    if (testBlock?.BlockDefinition?.Id.SubtypeName != "HighRPMMounting")
                        //        continue;
                        //    Vector3I tempVec = grid.WorldToGridInteger(testBlock.WorldMatrix.Translation);
                        //    Vector3I tempDelta = pos - tempVec;
                        //    Log.Info($"{testBlock?.BlockDefinition?.Id.SubtypeName}: {tempVec} delta: {tempDelta} ");
                        //}
                        

                        IMySlimBlock slimBlock = grid.GetCubeBlock(pos) as IMySlimBlock; 
                        Log.Info($"{slimBlock?.BlockDefinition?.Id.SubtypeId}");
                        IMyUpgradeModule upgradeModule = slimBlock?.FatBlock as IMyUpgradeModule;
                        if (upgradeModule == null)
                        {
                            Log.Info("No mounting found");
                            block.Enabled = false;
                            return;
                        }
                        else
                        {
                            double alignDot = Math.Round(Vector3.Dot(upgradeModule.WorldMatrix.Backward, thrust.WorldMatrix.Backward), 1);

                            VectorThrustEnginesMod.ShipPropellerDefinitions mountingDef;
                            

                            if (alignDot == 1 && VectorThrustEnginesMod.Instance.mountingsList.TryGetValue(upgradeModule.BlockDefinition.SubtypeId, out mountingDef)) //Check if mounting is aligned and has a definition
                            {
                                linkedMounting = upgradeModule;
                                efficiencyCalculation = mountingDef.efficiencyFunction; //Apply efficiency calculation
                                maxRPM = mountingDef.maxRPM; //Apply max RPM
                                screwPosition = mountingDef.screwPostion; //Apply screw position
                            }
                            else
                            {
                                //Log.Info($"Mounting found but not aligned {alignDot}");
                                block.Enabled = false;
                                return;
                            }
                        }


                    }

                    return;
                }

                if (linkedMounting.Closed || linkedMounting.MarkedForClose)
                {
                    linkedMounting = null;
                    block.Enabled = false;
                    return;
                }

                if (!linkedMounting.IsWorking)
                {
                    block.Enabled = false;
                }


                //thrust.BlockDefinition.ForceMagnitude = efficiencyCalculation(thrust.BlockDefinition.ForceMagnitude); //Apply efficiency calculation

                Vector3D worldScrewLocation = linkedMounting.GetPosition() + screwPosition;

                isUnderwater = WaterModAPI.IsUnderwater(worldScrewLocation);

                if (block.Enabled && thrust.CurrentStrength > 0 && isUnderwater)
                {
                    float efficiencyConstant = efficiencyCalculation(thrust.CurrentStrength);

                    Vector3D counterForce = thrust.WorldMatrix.Forward * thrust.BlockDefinition.ForceMagnitude * thrust.CurrentStrength; // Double the normal thrust force to 1) stall the forward thrust and 2) apply reverse thrust

                    Vector3D propulsionForce = counterForce * efficiencyConstant * (!VectorThrustReverse_Toggle ? -1.0 : 1.0); //!VectorThrustReverse_Toggle to account for block thrust direction

                    // THis is where the forces will be applied
                    Vector3D forceAt = grid.Physics.CenterOfMassWorld;

                    // Assemble the force vectors
                    Vector3D totalForce = counterForce + propulsionForce; // counterForce is the stall force, propulsionForce is the efficiencyConstant adjusted thrust
                    grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, totalForce, forceAt, null);

                    infoOutput = new MyTuple<float, float>((float)efficiencyConstant, (float)propulsionForce.Length());
                }
                else
                {
                    infoOutput = new MyTuple<float, float>(0, 0);
                }

                SpinShaft(thrust.CurrentStrength * (!VectorThrustReverse_Toggle ? -1.0f : 1.0f));
                if (thrust.CurrentStrength > 0.8)
                {
                    WaterModAPI.CreateBubble(worldScrewLocation, thrust.CurrentStrength/8);
                }



            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void SpinShaft(float targetRPM)
        {
            try
            {
                //Log.Info($"Spinning shaft with {targetRPM}", $"Spinning shaft with {targetRPM}",3000);
                bool shouldSpin = true; // if block is functional and enabled and powered.

                float desiredSpeedMultiplier = targetRPM;

                if (shouldSpin)
                {
                    if (targetSpeedMultiplier < desiredSpeedMultiplier)
                    {
                        targetSpeedMultiplier = Math.Min(targetSpeedMultiplier + ACCELERATE_PERCENT_PER_TICK, desiredSpeedMultiplier);
                    }
                    else if (targetSpeedMultiplier > desiredSpeedMultiplier)
                    {
                        targetSpeedMultiplier = Math.Max(targetSpeedMultiplier - DEACCELERATE_PERCENT_PER_TICK, desiredSpeedMultiplier);
                    }
                }
                else
                {
                    targetSpeedMultiplier = Math.Max(targetSpeedMultiplier - DEACCELERATE_PERCENT_PER_TICK, 0);
                }

                var camPos = MyAPIGateway.Session.Camera.WorldMatrix.Translation; // local machine camera position

                if (Vector3D.DistanceSquared(camPos, block.GetPosition()) > MAX_DISTANCE_SQ)
                    return;

                MyEntitySubpart subpart;
                if (linkedMounting.TryGetSubpart(SUBPART_NAME, out subpart)) // subpart does not exist when block is in build stage
                {
                    if (subpartFirstFind) // first time the subpart was found
                    {
                        subpartFirstFind = false;
                        subpartLocalMatrix = subpart.PositionComp.LocalMatrixRef;
                    }

                    if (Math.Abs(targetSpeedMultiplier) > 0)
                    {
                        // subpartLocalMatrix *= Matrix.CreateFromAxisAngle(ROTATION_AXIS, MathHelper.ToRadians(targetSpeedMultiplier * DEGREES_PER_TICK));
                        subpartLocalMatrix *= Matrix.CreateFromAxisAngle(ROTATION_AXIS, MathHelper.ToRadians(targetSpeedMultiplier * maxRPM / 10));
                        subpartLocalMatrix = Matrix.Normalize(subpartLocalMatrix); // normalize to avoid any rotation inaccuracies over time resulting in weird scaling
                    }

                    subpart.PositionComp.SetLocalMatrix(ref subpartLocalMatrix);
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
                    Settings.VectorThrustReverse_Toggle = loadedSettings.VectorThrustReverse_Toggle;
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