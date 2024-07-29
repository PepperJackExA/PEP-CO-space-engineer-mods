﻿using System;
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
using System.Text;
using System.Reflection;
using Sandbox.Game.Entities.Blocks;
using System.Collections;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Linq;




namespace PEPCO
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false, "NavigationScreen", "SG_NavigationScreen")]




    public class NavigationScreenLogic : MyGameLogicComponent
    {


        private const bool debug = false; //STOP SPAMMING ME!

        //The entities
        IMyTerminalBlock block; // the block itself
        private MyEntitySubpart subpart; // the subpart itself

        //Subpart name
        private const string subpartName = "MercPlane"; // what I called it

        double latitude;
        double longitude;
        private class MapLookup
        {
            public MyStringHash planetName { get; set; }
            public string subpartName { get; set; }
        }

        //Hashtable for the map names
        private Hashtable mapTable = new Hashtable()
        {
            //Add the map names to the hashtable
            { MyStringHash.GetOrCompute("Planet Thora 4"), "AaWThora4_Planet" },
            { MyStringHash.GetOrCompute("Planet Agaris"), "AaWAgaris_Planet" },
            { MyStringHash.GetOrCompute("Planet Crait"), "AaWCrait_Planet" },
            { MyStringHash.GetOrCompute("Agaris II"), "AaWAgarisII_Planet" },
            { MyStringHash.GetOrCompute("Planet Lezuno"), "AaWLezuno_Planet" },
            { MyStringHash.GetOrCompute("Planet Lorus"), "AaWLorus_Planet" }
        };

        



        //Subpart operation
        private bool subpartFirstFind = true; // used for storing the subpart Worldmatrix on first find

        //Subpart orientation
        private Matrix subpartLocalMatrix; // keeping the matrix here because subparts are being re-created on paint, resetting their orientations

        //Screen dimensions
        private const double screenWidth = 4.0;
        private const double screenHeight = 2.0;

        //Chevron dimension limits
        public const float minChevronScale = 0.1f;
        public const float maxChevronScale = 10f;

        //Chevron strength limits
        public const float minChevronStrength = 0.1f;
        public const float maxChevronStrengt = 10f;

        private Vector3 baseSouth = new Vector3(0f, 1f, 0f);

        //For rotating the indicator towards the heading
        private Vector3 rotationAxis = Vector3.Forward; // rotation axis for the subpart

        //For the distance check
        private const float maxDistanceSquared = 100 * 100; // player camera must be closer than this distance (squared) to see the gimmick's details

        //For the last position check
        private MatrixD lastPositionComponent;

        //For the planet check
        private MyPlanet lastPlanet;

        //The mod stroage component UID
        public readonly Guid SETTINGS_GUID = new Guid("4D3C2B1A-8765-CBA9-0FED-BA0987654321");

        public readonly NavigationScreenBlockSettings Settings = new NavigationScreenBlockSettings();
        int syncCountdown;
        public const int SETTINGS_CHANGED_COUNTDOWN = (60 * 1) / 10;


        NavigationScreenMod Mod => NavigationScreenMod.Instance;

        public long NavigationScreenOffset
        {
            get { return Settings.NavigationScreenOffset; }
            set
            {
                Settings.NavigationScreenOffset = value;

                SettingsChanged();
                UpdateMap(block);
            }
        }

        public Color NavigationScreenChevronColor
        {
            get { return Settings.NavigationScreenChevronColor; }
            set
            {
                Settings.NavigationScreenChevronColor = value;

                SettingsChanged();
                UpdateMap(block);
            }
        }

        public float NavigationScreenChevronScale
        {
            get { return Settings.NavigationScreenChevronScale; }
            set
            {
                Settings.NavigationScreenChevronScale = MathHelper.Clamp(value, minChevronScale, maxChevronScale);

                SettingsChanged();
                UpdateMap(block);
            }
        }

        public float NavigationScreenChevronStrength
        {
            get { return Settings.NavigationScreenChevronStrength; }
            set
            {
                Settings.NavigationScreenChevronStrength = MathHelper.Clamp(value, minChevronStrength, maxChevronStrengt);

                SettingsChanged();
                UpdateMap(block);
            }
        }

        public int NavigationScreenZoom
        {
            get { return Settings.NavigationScreenZoom; }
            set
            {
                Settings.NavigationScreenChevronStrength = MathHelper.Clamp(value, 0, 1);

                SettingsChanged();
                UpdateMap(block);
            }
        }


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (debug) Log.Info("Init()", "Init", 10000);

            block = (IMyTerminalBlock)Entity;
            block.AppendingCustomInfo += CustomInfo;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private void CustomInfo(IMyTerminalBlock block, StringBuilder builder)
        {
            if (NavigationScreenMod.Instance == null || !NavigationScreenMod.Instance.IsPlayer)
                return;

            if (block?.CubeGrid?.Physics == null)
                return;

            //builder.Clear();
            builder.Append($"Planet: {lastPlanet.Generator.Id.SubtypeName.ToString()}\n" +
                $"Orientation: {lastPlanet.PositionComp.GetOrientation()}\n" +
                $"Longitude: {MathHelper.ToDegrees(longitude)}\n" +
                $"Latitude: {MathHelper.ToDegrees(latitude)}\n" +
                $"Misc: ");

        }

        public override void UpdateOnceBeforeFrame()
        {
            try
            {
                if (debug) Log.Info("UpdateOnceBeforeFrame()", "UpdateOnceBeforeFrame", 10000);
                // If we are on a dedicated server, do nothing
                if (MyAPIGateway.Utilities.IsDedicated)
                {
                    //Log.Info("This mod does nothing on dedicated servers.");
                    return;
                }


                // If the block is not ignore
                if (block?.CubeGrid?.Physics == null)
                {
                    //Log.Info("Block or grid has no physics, skipping.");
                    return;
                }

                NavigationScreenTerminalControls.DoOnce(ModContext); //Add the custom terminal controls
                HideNavigationScreenControls.DoOnce(); //Hide the default terminal controls

                // set default settings
                Settings.NavigationScreenChevronScale = 1.0f;
                Settings.NavigationScreenChevronColor = new Color(255, 0, 255);
                Settings.NavigationScreenChevronStrength = 10.0f;
                Settings.NavigationScreenOffset = 2;
                Settings.NavigationScreenZoom = 0;


                //Set the block's screen settings so maps can be displayed
                var screen = block as IMyTextSurfaceProvider;
                if (screen != null)
                {
                    screen.GetSurface(0).ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    screen.GetSurface(0).FontSize = 1.0f;
                    screen.GetSurface(0).Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
                    screen.GetSurface(0).TextPadding = 0.0f;
                }

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
                var loadedSettings = MyAPIGateway.Utilities.SerializeFromBinary<NavigationScreenBlockSettings>(Convert.FromBase64String(rawData));

                if (loadedSettings != null)
                {
                    Settings.NavigationScreenOffset = loadedSettings.NavigationScreenOffset;
                    Settings.NavigationScreenChevronScale = loadedSettings.NavigationScreenChevronScale;
                    Settings.NavigationScreenChevronColor = loadedSettings.NavigationScreenChevronColor;
                    Settings.NavigationScreenChevronStrength = loadedSettings.NavigationScreenChevronStrength;
                    Settings.NavigationScreenZoom = loadedSettings.NavigationScreenZoom;
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
                throw new NullReferenceException($"Settings == null on entId={Entity?.EntityId}; modInstance={NavigationScreenMod.Instance != null}");

            if (MyAPIGateway.Utilities == null)
                throw new NullReferenceException($"MyAPIGateway.Utilities == null; entId={Entity?.EntityId}; modInstance={NavigationScreenMod.Instance != null}");

            if (block.Storage == null)
                block.Storage = new MyModStorageComponent();

            block.Storage.SetValue(SETTINGS_GUID, Convert.ToBase64String(MyAPIGateway.Utilities.SerializeToBinary(Settings)));
        }

        //public override void UpdateBeforeSimulation10()
        //{
        //    try
        //    {
        //        Log.Info("UpdateBeforeSimulation10");
        //        SyncSettings();
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e);
        //    }
        //}

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

            if (debug) Log.Info("UpdateAfterSimulation()", "UpdateAfterSimulation", 10000);

            // If the block is not valid, return
            if (block?.CubeGrid?.Physics == null || block.MarkedForClose || block.Closed) return;

            //Log.Info("UpdateAfterSimulation");
            // If the block is too far away, return
            if (Vector3D.DistanceSquared(MyAPIGateway.Session.Camera.Position, block.GetPosition()) > maxDistanceSquared) return;

            try
            {
                
                
                

                //Test if the closest planet has changed
                MyPlanet currentPlanet = MyGamePruningStructure.GetClosestPlanet(block.GetPosition());

                //Log.Info("Updating map subparts " + currentPlanet?.Generator.Id.SubtypeId);

                //Log.Info("Current planet: " + currentPlanet.StorageName + "\n" +
                //    lastPlanet?.StorageName + "\n" +
                //    (lastPlanet == currentPlanet));
                if (currentPlanet != null && currentPlanet != lastPlanet)
                {
                    //Update the map
                    UpdateMapScreen(block, currentPlanet.Generator.Id.SubtypeId);

                    lastPlanet = currentPlanet;
                }

                // Only update if the block has moved
                if (block.PositionComp.WorldMatrixRef != lastPositionComponent)
                {
                    //Log.Info("Block has not moved, skipping.");
                    // Update the lastPositionComponent 
                    lastPositionComponent = block.PositionComp.WorldMatrixRef;

                    UpdateMap(block);
                }
            }
            catch
            {
                //Log the error
                Log.Error("Error in UpdateAfterSimulation");
            }

        }

        private void UpdateMap(IMyTerminalBlock block)
        {
            //Log.Info("Updating the subpart");
            try
            {
                //Vector for the block position
                Vector3D blockPosition = block.GetPosition();

                // Vectors for the rotation
                MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(blockPosition);

                if (planet == null) return; // no planet found
                //Maybe add something here a subpart or so...

                // Get the Planet position
                Vector3D planetPosition = planet.PositionComp.GetPosition();

                // Get the vector from the block to the planet
                Vector3D blockToPlanet = Vector3D.Normalize(blockPosition - planetPosition);

                latitude = Math.Asin(blockToPlanet.Y / blockToPlanet.Length());
                double latFraction = latitude / (Math.PI / 2) * -1;
                longitude = Math.Atan2(blockToPlanet.Z, blockToPlanet.X * -1) * -1;
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

                // Calculate the heading in radians
                float heading = blockAzimuth - northAzimuth;
                if (heading < 0)
                {
                    heading += 2 * (float)Math.PI;
                }
                else if (heading > 2 * (float)Math.PI)
                {
                    heading -= 2 * (float)Math.PI;
                }

                // Output the heading
                //Log.Info($"heading: {heading} radians");

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
                    Matrix finalMatrix = Matrix.CreateFromAxisAngle(rotationAxis, heading + (float)(0.5 * Math.PI) + (float)(-Math.PI + NavigationScreenOffset * 0.5 * Math.PI)); // 90 degrees is added to make the chevron point north because I don't know why it doesn't --- + (float)(0.5 * Math.PI)
                    finalMatrix *= subpartLocalMatrix;

                    // Apply the offsetMatrix to the subpart
                    //subpart.PositionComp.SetLocalMatrix(ref offsetMatrix);

                    // Use the original position so that manipulations are not cumulative
                    Matrix offsetMatrix = subpartLocalMatrix;
                    Matrix translationMatrix;


                    //Adjust the screenwidth and height for small grid
                    if (block.CubeGrid.GridSizeEnum == MyCubeSize.Small)
                    {
                        // Apply the translation first
                        translationMatrix = Matrix.CreateTranslation((float)(-1 * lonFraction * screenWidth * 0.2 * 0.5), (float)(-1 * latFraction * screenHeight * 0.2 * 0.5), 0f);
                    }
                    else
                    {
                        // Apply the translation first
                        translationMatrix = Matrix.CreateTranslation((float)(-1 * lonFraction * screenWidth * 0.5), (float)(-1 * latFraction * screenHeight * 0.5), 0f);
                    }
                    // Combine the matrices
                    offsetMatrix *= translationMatrix;

                    //Keep the orientation and take the offset translation
                    finalMatrix.Translation = offsetMatrix.Translation;

                    //Apply the chevron scale
                    finalMatrix = Matrix.CreateScale(NavigationScreenChevronScale) * finalMatrix;

                    //Apply the final matrix to the subpart
                    subpart.PositionComp.SetLocalMatrix(ref finalMatrix);

                    //Set emissive parts on the subpart
                    subpart.SetEmissiveParts("Emissive0", NavigationScreenChevronColor, NavigationScreenChevronStrength);
                }
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in MoveSubpart: " + e);
            }
        }

        private void UpdateMapScreen(IMyTerminalBlock block, MyStringHash currentPlanetName)
        {
            //Error handling
            try
            {
                var textPanel = block as IMyTextSurfaceProvider; // Get the block to get the screens
                textPanel.GetSurface(0).ClearImagesFromSelection(); // Clear the images from the screen
                if (debug)
                {
                    
                    //textPanel.GetSurface(0).ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    textPanel.GetSurface(0).ClearImagesFromSelection();
                    
                    List<string> images = new List<string>();
                    textPanel.GetSurface(0).GetSelectedImages(images);
                    Log.Info($"Images: {string.Join(", ", images)}");
                }

                //lookup the map using the planet name
                string mapName = (string)mapTable[currentPlanetName];

                //was the map found?
                if (mapName == null)
                {
                    //Log the error
                    Log.Error("Error: Map not found for planet " + currentPlanetName);
                    mapName = "Error_Planet";
                }
                textPanel.GetSurface(0).AddImageToSelection(mapName);

                //Get the image based on the planet name

                

            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in UpdateMapSubparts: " + e);
            }

        }
    }

}
