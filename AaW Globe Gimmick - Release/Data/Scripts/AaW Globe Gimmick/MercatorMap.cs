using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using Digi;
using Sandbox.Game.Entities;
using static VRageRender.MyBillboard;
using static Sandbox.Game.Entities.MyCubeGrid;
using System.Collections.Generic;
using Sandbox.Game;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using Sandbox.Game.EntityComponents;




namespace PEPCO
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "MercatorMap")]
    public class MercatorMapLogic : MyGameLogicComponent
    {

        //The entities
        IMyTerminalBlock block; // the block itself
        private MyEntitySubpart subpart; // the subpart itself

        //Subpart name
        private const string subpartName = "MercPlane"; // what I called it

        //Subpart operation
        private bool subpartFirstFind = true; // used for storing the subpart Worldmatrix on first find

        //Subpart orientation
        private Matrix subpartLocalMatrix; // keeping the matrix here because subparts are being re-created on paint, resetting their orientations

        //Screen dimensions
        private const double screenWidth = 4.5;
        private const double screenHeight = screenWidth * 0.5;

        private Vector3 baseSouth = new Vector3(0f, 1f, 0f);

        //For rotating the indicator towards the bearing
        private Vector3 rotationAxis = Vector3.Forward; // rotation axis for the subpart

        //For the distance check
        private const float maxDistanceSquared = 100 * 100; // player camera must be closer than this distance (squared) to see the gimmick's details

        //THe mod stroage component UID
        public readonly Guid SETTINGS_GUID = new Guid("4D3C2B1A-8765-CBA9-0FED-BA0987654321");

        public readonly MercatorMapBlockSettings Settings = new MercatorMapBlockSettings();
        int syncCountdown;
        public const int SETTINGS_CHANGED_COUNTDOWN = (60 * 1) / 10;

        MercatorMapMod Mod => MercatorMapMod.Instance;

        public long mercatorMapOffset
        {
            get { return Settings.mercatorMapOffset; }
            set
            {
                Settings.mercatorMapOffset = value;

                SettingsChanged();
            }
        }

        public Color mercatorMapChevronColor
        {
            get { return Settings.mercatorMapChevronColor; }
            set
            {
                Settings.mercatorMapChevronColor = value;

                SettingsChanged();
            }
        }


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyTerminalBlock)Entity;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            try
            {
                // If we are on a dedicated server, do nothing
                if (MyAPIGateway.Utilities.IsDedicated)
                {
                    Log.Info("This mod does nothing on dedicated servers.");
                    return;
                }


                // If the block is not ignore
                if (block?.CubeGrid?.Physics == null)
                {
                    Log.Info("Block or grid has no physics, skipping.");
                    return;
                }

                MercatorMapTerminalControls.DoOnce(ModContext);

                LoadSettings();

                SaveSettings(); // required for IsSerialized()

                //Good to go
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
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
                var loadedSettings = MyAPIGateway.Utilities.SerializeFromBinary<MercatorMapBlockSettings>(Convert.FromBase64String(rawData));

                if (loadedSettings != null)
                {
                    Settings.mercatorMapOffset = loadedSettings.mercatorMapOffset;
                    Settings.mercatorMapChevronColorRed = loadedSettings.mercatorMapChevronColorRed;
                    Settings.mercatorMapChevronColorGreen = loadedSettings.mercatorMapChevronColorGreen;
                    Settings.mercatorMapChevronColorBlue = loadedSettings.mercatorMapChevronColorBlue;
                    Settings.mercatorMapChevronColor = loadedSettings.mercatorMapChevronColor;
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
                throw new NullReferenceException($"Settings == null on entId={Entity?.EntityId}; modInstance={MercatorMapMod.Instance != null}");

            if (MyAPIGateway.Utilities == null)
                throw new NullReferenceException($"MyAPIGateway.Utilities == null; entId={Entity?.EntityId}; modInstance={MercatorMapMod.Instance != null}");

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

        public override void UpdateAfterSimulation()
        {
            // If the block is not valid, return
            if (block?.CubeGrid?.Physics == null || block.MarkedForClose || block.Closed) return;

            // If the block is too far away, return
            if (Vector3D.DistanceSquared(MyAPIGateway.Session.Camera.Position, block.GetPosition()) > maxDistanceSquared) return;


            //Log.Info("Updating the subpart");
            try
            {
                //Vector for the block position
                Vector3D blockPosition = block.GetPosition();

                // Vectors for the rotation
                MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(blockPosition);

                //Test if the closest planet has Agaris in its name
                if (planet != null && planet.Name.Contains("Agaris"))
                {
                    // Get the Planet position
                    Vector3D planetPosition = planet.PositionComp.GetPosition();

                    // Get the vector from the block to the planet
                    Vector3D blockToPlanet = Vector3D.Normalize(blockPosition - planetPosition);

                    double latitude = Math.Asin(blockToPlanet.Y / blockToPlanet.Length());
                    double latFraction = latitude / (Math.PI / 2);
                    double longitude = Math.Atan2(blockToPlanet.X, blockToPlanet.Z);
                    double lonFraction = longitude / Math.PI;

                    // Find the direction the block is facing
                    Vector3 blockRotForwardVector = block.WorldMatrix.Rotation.Forward;

                    // Get block position normal (up in reference to the planet)
                    Vector3D relativeBlockPos = block.GetPosition() - planetPosition;
                    Vector3 relativeBlockPosNormal = Vector3.Normalize(relativeBlockPos);

                    // Get north normal (north relative to the planet)
                    Vector3D relativeNorth = planetPosition + new Vector3D(0f, planet.AverageRadius, 0f);

                    Vector3D blockToNorth = Vector3D.Cross(relativeNorth - blockPosition, planetPosition - blockPosition);
                    Vector3 blockRelativeNorthNormal = Vector3.Normalize(blockToNorth);

                    // Calculate the axes offset introduced by the planet's curve on the block
                    Matrix relativeBlockPosNormalOffset;
                    Matrix.CreateRotationFromTwoVectors(ref relativeBlockPosNormal, ref baseSouth, out relativeBlockPosNormalOffset);

                    // Apply that offset to the block's forward direction and north
                    Vector3 blockRotForwardVectorCorrected = Vector3.Transform(blockRotForwardVector, relativeBlockPosNormalOffset);
                    Vector3 blockRelativeNorthNormalCorrected = Vector3.Transform(blockRelativeNorthNormal, relativeBlockPosNormalOffset);

                    float blockAzimuth, blockElevation, northAzimuth, northElevation;

                    // Calculate the block's azimuth and elevation from its forward direction
                    Vector3.GetAzimuthAndElevation(blockRotForwardVectorCorrected, out blockAzimuth, out blockElevation);

                    // Calculate the north direction azimuth and elevation from its direction
                    Vector3.GetAzimuthAndElevation(blockRelativeNorthNormalCorrected, out northAzimuth, out northElevation);

                    // Calculate the bearing in radians
                    float bearing = blockAzimuth - northAzimuth;
                    if (bearing < 0)
                    {
                        bearing += 2 * (float)Math.PI;
                    }
                    else if (bearing > 2 * (float)Math.PI)
                    {
                        bearing -= 2 * (float)Math.PI;
                    }

                    // Output the bearing
                    //Log.Info($"Bearing: {bearing} radians");

                    // If the subpart is found
                    if (block.TryGetSubpart(subpartName, out subpart))
                    {
                        if (subpartFirstFind)
                        {
                            // Store the original position only once
                            subpartLocalMatrix = subpart.PositionComp.LocalMatrixRef;

                            subpartFirstFind = false;
                        }

                        // Calculate the rotation axis
                        rotationAxis = Vector3D.Up;

                        //Degrees to radians


                        // Apply the rotation
                        Matrix finalMatrix = Matrix.CreateFromAxisAngle(rotationAxis, bearing + (float)(0.5 * Math.PI) + (float)(-Math.PI + mercatorMapOffset * 0.5 *Math.PI)); // 90 degrees is added to make the chevron point north because I don't know why it doesn't --- + (float)(0.5 * Math.PI)
                        finalMatrix *= subpartLocalMatrix;

                        // Apply the offsetMatrix to the subpart
                        //subpart.PositionComp.SetLocalMatrix(ref offsetMatrix);

                        // Use the original position so that manipulations are not cumulative
                        Matrix offsetMatrix = subpartLocalMatrix;

                        // Apply the translation first
                        Matrix translationMatrix = Matrix.CreateTranslation((float)(-1 * lonFraction * screenWidth * 0.5), (float)(1 * latFraction * screenHeight * 0.5), 0f);
                        offsetMatrix *= translationMatrix;

                        //Combine the matrices
                        finalMatrix.Translation = offsetMatrix.Translation;

                        Log.Info(finalMatrix.Scale.ToString());

                        subpart.PositionComp.SetLocalMatrix(ref finalMatrix);

                        subpart.SetEmissiveParts("Emissive0", mercatorMapChevronColor, 10f);
                    }
                }
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in MoveSubpart: " + e);
            }
        }
    }
}
