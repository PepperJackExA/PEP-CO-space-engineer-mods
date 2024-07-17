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
using SubmarineStuff;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game;
using VRage;
using Jakaria.API;
using VRage.Game.Entity;
using System.Text;

namespace SubmarineStuff
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false,
                                    "BallastTank")]
    public class BallastTankLogic : MyGameLogicComponent
    {

        IMyTerminalBlock block;

        public const float maxFillLevel = 100;
        public const float minFillLevel = 0;

        private float lastFillLevel;
        Vector3D blockPosition;
        MyEntity myEntity;

        public readonly Guid SETTINGS_GUID = new Guid("A1B2C3D4-5678-9ABC-DEF0-1234567890AB");

        public readonly BallastTankBlockSettings Settings = new BallastTankBlockSettings();
        int syncCountdown;
        public const int SETTINGS_CHANGED_COUNTDOWN = (60 * 1) / 10;

        SubmarineStuffMod Mod => SubmarineStuffMod.Instance;

        public float ballastTank_Fill
        {
            get { return Settings.ballastTank_Fill; }
            set
            {
                Settings.ballastTank_Fill = MathHelper.Clamp(value, minFillLevel, maxFillLevel);

                SettingsChanged();

            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            block = (IMyTerminalBlock)Entity;
            myEntity = block.CubeGrid as MyEntity;


            block.AppendingCustomInfo += CustomInfo;
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
            BallastTankTerminalControls.DoOnce(ModContext);

            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;

            LoadSettings();

            SaveSettings(); // required for IsSerialized()

        }

        public override void UpdateBeforeSimulation()
        {
            if (SubmarineStuffMod.Instance == null || !SubmarineStuffMod.Instance.IsPlayer)
                return;

            if (block?.CubeGrid?.Physics == null)
                return;

            if (ballastTank_Fill != lastFillLevel)
            {
                lastFillLevel = ballastTank_Fill;
                try
                {
                    blockPosition = block.GetPosition();
                    MyInventory inv = (MyInventory)block.GetInventory();
                    inv.ExternalMass = (MyFixedPoint)(202 + (ballastTank_Fill / 100) * 3614);
                    inv.Refresh();

                    WaterModAPI.CreateSplash((blockPosition + new Vector3D(0, 0, 1)),1,true);
                    WaterModAPI.CreateBubble((blockPosition + new Vector3D(0, 0, 1)), 1);

                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private void CustomInfo(IMyTerminalBlock block, StringBuilder builder)
        {
            if (SubmarineStuffMod.Instance == null || !SubmarineStuffMod.Instance.IsPlayer)
                return;

            if (block?.CubeGrid?.Physics == null)
                return;

                //builder.Clear();
                builder.Append($"Fill Level: {ballastTank_Fill}\n" +
                        $"Depth: {WaterModAPI.GetDepth(blockPosition)}\n");

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
                var loadedSettings = MyAPIGateway.Utilities.SerializeFromBinary<BallastTankBlockSettings>(Convert.FromBase64String(rawData));

                if (loadedSettings != null)
                {
                    Settings.ballastTank_Fill = loadedSettings.ballastTank_Fill;
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
                throw new NullReferenceException($"Settings == null on entId={Entity?.EntityId}; modInstance={SubmarineStuffMod.Instance != null}");

            if (MyAPIGateway.Utilities == null)
                throw new NullReferenceException($"MyAPIGateway.Utilities == null; entId={Entity?.EntityId}; modInstance={SubmarineStuffMod.Instance != null}");

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