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
using System.Text;
using System.Reflection;
using Sandbox.Game.Entities.Blocks;
using System.Collections;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Linq;
using Sandbox.Definitions;
using VRage;
using System.Runtime.Remoting.Metadata.W3cXsd2001;




namespace PEPCO
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false, "NavigationScreen", "SG_NavigationScreen")]




    public class NavigationScreenLogic : MyGameLogicComponent
    {


        private const bool debug = false; //STOP SPAMMING ME!

        //The entities
        IMyTerminalBlock block; // the block itself
        IMyTextSurfaceProvider blockAsTextSurfaceProvider; // the block as a text surface provider
        private MyEntitySubpart subpart; // the subpart itself

        //Subpart name
        private const string subpartName = "MercPlane"; // what I called it

        // Update the MapLookup class to include the subpartName and planetOffset properties
        class MapLookup
        {
            public string mapTextureName { get; set; }
            public int planetOffset { get; set; }
        }

        // Update the mapTable initialization code to use the MapLookup class
        private Dictionary<MyStringHash, MapLookup> mapTable = new Dictionary<MyStringHash, MapLookup>()
        {
            { MyStringHash.GetOrCompute("Planet Thora 4"), new MapLookup { mapTextureName = "AaWThora4_Planet", planetOffset = 2 } },
            { MyStringHash.GetOrCompute("Planet Agaris"), new MapLookup { mapTextureName = "AaWAgaris_Planet", planetOffset = 1 } },
            { MyStringHash.GetOrCompute("Planet Crait"), new MapLookup { mapTextureName = "AaWCrait_Planet", planetOffset = 2 } },
            { MyStringHash.GetOrCompute("Agaris II"), new MapLookup { mapTextureName = "AaWAgarisII_Planet", planetOffset = 1 } },
            { MyStringHash.GetOrCompute("Planet Lezuno"), new MapLookup { mapTextureName = "AaWLezuno_Planet", planetOffset = 2 } },
            { MyStringHash.GetOrCompute("Planet Lorus"), new MapLookup { mapTextureName = "AaWLorus_Planet", planetOffset = 2 } }
        };


        Vector2 quadrant; // Used for zooming in on the map
        Vector2 previousquadrant; // Used for zooming in on the map


        //Subpart operation
        private bool subpartFirstFind = true; // used for storing the subpart Worldmatrix on first find

        //Test if map screen needs an update
        private bool mapScreenNeedsUpdate = true;

        //Subpart orientation
        private Matrix subpartLocalMatrix; // keeping the matrix here because subparts are being re-created on paint, resetting their orientations

        //Screen dimensions
        private const float screenWidth = 4.0f;
        private const float screenHeight = 2.0f;

        //Chevron dimension limits
        public const float minChevronScale = 0.1f;
        public const float maxChevronScale = 10f;

        //Chevron strength limits
        public const float minChevronStrength = 0.1f;
        public const float maxChevronStrengt = 10f;

        //Zoom  limits
        public const float minZoom = 1f;
        public const float maxZoom = 10f;


        //For the planet check & for showing on the map
        private double latitude, longitude; //For arranging the chevron on the map
        private float heading, gravity; //For rotating the chevron towards the heading
        private bool inGravity = false; //For checking if the block is in gravity
        string longitudeOutput; 
        string latitudeOutput;

        MapLookup planetLookup;

        //Required for the map since sometimes the planet is rotated funny
        private float planetOffset;

        private Vector3 baseSouth = new Vector3(0f, 1f, 0f);

        //For rotating the indicator towards the heading
        private Vector3 rotationAxis = Vector3.Forward; // rotation axis for the subpart

        //For the distance check
        private const float maxDistanceSquared = 100 * 100; // player camera must be closer than this distance (squared) to see the gimmick's details

        //For the last position check
        private MatrixD lastPositionComponent;

        //For the planet check
        private MyPlanet currentPlanet;

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
            }
        }

        public Color NavigationScreenChevronColor
        {
            get { return Settings.NavigationScreenChevronColor; }
            set
            {
                Settings.NavigationScreenChevronColor = value;

                SettingsChanged();
            }
        }

        public float NavigationScreenChevronScale
        {
            get { return Settings.NavigationScreenChevronScale; }
            set
            {
                Settings.NavigationScreenChevronScale = MathHelper.Clamp(value, minChevronScale, maxChevronScale);

                SettingsChanged();
            }
        }

        public float NavigationScreenChevronStrength
        {
            get { return Settings.NavigationScreenChevronStrength; }
            set
            {
                Settings.NavigationScreenChevronStrength = MathHelper.Clamp(value, minChevronStrength, maxChevronStrengt);

                SettingsChanged();
            }
        }

        public float NavigationScreenZoom
        {
            get { return Settings.NavigationScreenZoom; }
            set
            {
                Settings.NavigationScreenZoom = MathHelper.Clamp(value, minZoom, maxZoom);

                SettingsChanged();
            }
        }


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            try
            {
                if (debug) Log.Info("Init()", "Init", 10000);

                block = (IMyTerminalBlock)Entity;
                blockAsTextSurfaceProvider = block as IMyTextSurfaceProvider;
                block.AppendingCustomInfo += CustomInfo;



                NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
            catch (Exception e)
            {
                Log.Error("Error in the init: "+e);
            }
        }

        private void CustomInfo(IMyTerminalBlock block, StringBuilder builder)
        {
            if (NavigationScreenMod.Instance == null || !NavigationScreenMod.Instance.IsPlayer)
                return;

            if (block?.CubeGrid?.Physics == null)
                return;

            


            //builder.Clear();
            builder.Append($"Planet: {currentPlanet.Generator.Id.SubtypeName.ToString()}\n" +
                $"Orientation:\n{currentPlanet.PositionComp.WorldMatrixRef.GetOrientation().Rotation.Col0}\n{currentPlanet.PositionComp.WorldMatrixRef.GetOrientation().Rotation.Col1}\n{currentPlanet.PositionComp.WorldMatrixRef.GetOrientation().Rotation.Col2}\n" +
                $"Longitude: {longitudeOutput}\n" +
                $"Latitude: {latitudeOutput}\n" +
                $"Zoom: {NavigationScreenZoom}\n" +
                $"Misc: {quadrant.X}{quadrant.Y}\n" +
                $"Planet offset: {planetOffset}");

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

                //HideNavigationScreenControls.DoOnce(); //Hide the default terminal controls
                NavigationScreenTerminalControls.DoOnce(ModContext); //Add the custom terminal controls


                // set default settings
                Settings.NavigationScreenChevronScale = 1.0f;
                Settings.NavigationScreenChevronColor = new Color(255, 0, 255);
                Settings.NavigationScreenChevronStrength = 10.0f;
                Settings.NavigationScreenOffset = 2;
                Settings.NavigationScreenZoom = 1f;


                //Set the block's screen settings so maps can be displayed

                if (blockAsTextSurfaceProvider != null)
                {
                    blockAsTextSurfaceProvider.GetSurface(0).ContentType = VRage.Game.GUI.TextPanel.ContentType.SCRIPT;
                    blockAsTextSurfaceProvider.GetSurface(0).FontSize = 1.0f;
                    blockAsTextSurfaceProvider.GetSurface(0).Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
                    blockAsTextSurfaceProvider.GetSurface(0).TextPadding = 0.0f;
                    blockAsTextSurfaceProvider.GetSurface(0).Script = "NavigationScreenTSS";
                    blockAsTextSurfaceProvider.GetSurface(0).ScriptBackgroundColor = new Color(0,0,0);

                }

                LoadSettings();

                SaveSettings(); // required for IsSerialized()



                //Good to go
                NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME;
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

        public override void UpdateBeforeSimulation10()
        {
            try
            {
                Log.Info("UpdateBeforeSimulation10");
                SyncSettings();

                if (debug) Log.Info("UpdateAfterSimulation()", "UpdateAfterSimulation", 10000);

                // If the block is not valid, return
                if (block?.CubeGrid?.Physics == null || block.MarkedForClose || block.Closed) return;

                // If the block is too far away, return
                if (Vector3D.DistanceSquared(MyAPIGateway.Session.Camera.Position, block.GetPosition()) > maxDistanceSquared) return;


                try
                {
                    gravity = GetGravityAtBlock(); // Get the gravity at the block

                    inGravity = gravity > 0; // Test if block is in natural gravity

                    if (debug) Log.Info("In gravity: " + inGravity);

                    if (currentPlanet != FindClosestPlanet())
                    {
                        // Find the closest planet
                        currentPlanet = FindClosestPlanet();
                        mapScreenNeedsUpdate = true;
                    }

                    if (inGravity) // Change in planet detected
                    {
                        if (debug) Log.Info($"block.GetPosition(): {block.GetPosition()}");

                        if (debug) Log.Info("Current planet: " + currentPlanet?.Generator.Id.SubtypeId);

                        // Update the map screen to show the planet
                        if (mapScreenNeedsUpdate)
                        {
                            UpdateMapScreen();
                            mapScreenNeedsUpdate = false;
                            if (debug) Log.Info("Map screen updated");
                        }

                        if (planetLookup != null)
                        {
                            // Show the subpart
                            ShowSubpart();
                            if (debug) Log.Info("Subpart shown");
                        }
                        else
                        {
                            // Hide the subpart
                            HideSubpart();
                            if (debug) Log.Info("Subpart hidden");
                        }

                        // Only update if the block has moved
                        if (block?.PositionComp?.WorldMatrixRef != lastPositionComponent)
                        {
                            // Update the map chevron
                            UpdateMapChevron();

                            lastPositionComponent = block.PositionComp.WorldMatrixRef;
                        }
                    }
                    else
                    {
                        // Clear the last planet
                        currentPlanet = null;

                        // Update the screen to show no planet
                        SetMapScreenToNoPlanet();

                        // Hide the subpart
                        HideSubpart();
                    }
                }
                catch (NullReferenceException nullRefEx)
                {
                    // Handle NullReferenceException
                    Log.Error("NullReferenceException in UpdateAfterSimulation: " + nullRefEx);
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    Log.Error("Error in UpdateAfterSimulation: " + ex);
                }
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

        private void UpdateMapChevron()
        {
            ////Log.Info("Updating the subpart");
            //try
            //{
            //    // Vector for the block position
            //    Vector3D blockPosition = block.GetPosition();

            //    // Get the Planet position
            //    Vector3D planetPosition = currentPlanet.PositionComp.GetPosition();

            //    // Get the vector from the block to the planet
            //    Vector3D blockToPlanet = blockPosition - planetPosition;

            //    // Define a small threshold to account for floating-point precision errors
            //    const double epsilon = 1e-10;

            //    Vector3D normalizedBlockToPlanet;

            //    // Calculate the length squared of the vector
            //    double lengthSquared = blockToPlanet.LengthSquared();

            //    // Normalize the vector if its length squared is greater than the threshold
            //    if (lengthSquared > epsilon)
            //    {
            //        normalizedBlockToPlanet = blockToPlanet / Math.Sqrt(lengthSquared);
            //    }
            //    else
            //    {
            //        // Handle the case where the vector length is too small
            //        normalizedBlockToPlanet = Vector3D.Zero;
            //    }

            //    // Calculate latitude
            //    double latitude = Math.Asin(normalizedBlockToPlanet.Y);
            //    double latFraction = latitude / (Math.PI / 2);
            //    latitudeOutput = Math.Abs(MathHelper.ToDegrees(latitude)).ToString("0.00") + "° " + (latitude > 0 ? "N" : "S");

            //    double longitude;
            //    // Calculate longitude
            //    if (planetOffset == 1) // Agaris and Moon
            //    {
            //        Log.Info("Agaris and Moon");
            //        longitude = Math.Atan2(-normalizedBlockToPlanet.X, -normalizedBlockToPlanet.Z);
            //    }
            //    else if (planetOffset == 2) // Lezuno
            //    {
            //        Log.Info("Lezuno");
            //        longitude = Math.Atan2(normalizedBlockToPlanet.Z, -normalizedBlockToPlanet.X);
            //    }
            //    else
            //    {
            //        Log.Info("Other planets");
            //        longitude = Math.Atan2(-normalizedBlockToPlanet.Z, -normalizedBlockToPlanet.X);
            //    }

            //    longitudeOutput = Math.Abs(MathHelper.ToDegrees(longitude)).ToString("0.00") + "° " + (longitude > 0 ? "E" : "W");

            //    // Output the results

            //    double lonFraction = longitude / Math.PI;



            //    quadrant.X = (longitude > 0 ? 1 : 0);
            //    quadrant.Y = (latitude > 0 ? 1 : 0);

            //    //Update the map if the quadrant has changed
            //    if (quadrant != previousquadrant)
            //    {
            //        UpdateMapScreen();
            //        previousquadrant = quadrant;
            //    }

            //    // Find the direction the block is facing
            //    Vector3 blockRotForwardVector = block.WorldMatrix.Rotation.Forward;

            //    // Get block position normal (up in reference to the planet)
            //    Vector3D relativeBlockPos = block.GetPosition() - planetPosition;
            //    Vector3 relativeBlockPosNormal = Vector3.Normalize(relativeBlockPos);

            //    // Get north normal (north relative to the planet)
            //    Vector3D relativeNorth = planetPosition + new Vector3D(0f, currentPlanet.AverageRadius, 0f);

            //    Vector3D blockToNorth = Vector3D.Cross(relativeNorth - blockPosition, planetPosition - blockPosition);
            //    Vector3 blockRelativeNorthNormal = Vector3.Normalize(blockToNorth);

            //    // Calculate the axes offset introduced by the planet's curve on the block
            //    Matrix relativeBlockPosNormalOffset;
            //    Matrix.CreateRotationFromTwoVectors(ref relativeBlockPosNormal, ref baseSouth, out relativeBlockPosNormalOffset);

            //    // Apply that offset to the block's forward direction and north
            //    Vector3 blockRotForwardVectorCorrected = Vector3.Transform(blockRotForwardVector, relativeBlockPosNormalOffset);
            //    Vector3 blockRelativeNorthNormalCorrected = Vector3.Transform(blockRelativeNorthNormal, relativeBlockPosNormalOffset);

            //    float blockAzimuth, blockElevation, northAzimuth, northElevation;

            //    // Calculate the block's azimuth and elevation from its forward direction
            //    Vector3.GetAzimuthAndElevation(blockRotForwardVectorCorrected, out blockAzimuth, out blockElevation);

            //    // Calculate the north direction azimuth and elevation from its direction
            //    Vector3.GetAzimuthAndElevation(blockRelativeNorthNormalCorrected, out northAzimuth, out northElevation);

            //    // Calculate the heading in radians
            //    heading = blockAzimuth - northAzimuth;
            //    if (heading < 0)
            //    {
            //        heading += 2 * (float)Math.PI;
            //    }
            //    else if (heading > 2 * (float)Math.PI)
            //    {
            //        heading -= 2 * (float)Math.PI;
            //    }

            //    // Output the heading
            //    //Log.Info($"heading: {heading} radians");

            //    // If the subpart is found
            //    if (block.TryGetSubpart(subpartName, out subpart))
            //    {
            //        if (subpartFirstFind)
            //        {
            //            // Store the original position only once
            //            subpartLocalMatrix = subpart.PositionComp.LocalMatrixRef;

            //            subpartFirstFind = false;
            //        }

            //        // Calculate the rotation axis
            //        rotationAxis = Vector3D.Up;

            //        //Degrees to radians


            //        // Apply the rotation
            //        Matrix finalMatrix = Matrix.CreateFromAxisAngle(rotationAxis, heading + (float)(0.5 * Math.PI) + (float)(-Math.PI + NavigationScreenOffset * 0.5 * Math.PI)); // 90 degrees is added to make the chevron point north because I don't know why it doesn't --- + (float)(0.5 * Math.PI)
            //        finalMatrix *= subpartLocalMatrix;

            //        // Apply the offsetMatrix to the subpart
            //        //subpart.PositionComp.SetLocalMatrix(ref offsetMatrix);

            //        // Use the original position so that manipulations are not cumulative
            //        Matrix offsetMatrix = subpartLocalMatrix;
            //        Matrix translationMatrix;


            //        //If zoom is 0
            //        if (NavigationScreenZoom == 0)
            //        {
            //            //Adjust the screenwidth and height for small grid
            //            if (block.CubeGrid.GridSizeEnum == MyCubeSize.Small)
            //            {
            //                // Apply the translation first
            //                translationMatrix = Matrix.CreateTranslation((float)(lonFraction * screenWidth * 0.2 * 0.5), (float)(latFraction * screenHeight * 0.2 * 0.5), 0f);
            //            }
            //            else
            //            {
            //                // Apply the translation first
            //                translationMatrix = Matrix.CreateTranslation((float)(lonFraction * screenWidth * 0.5), (float)(latFraction * screenHeight * 0.5), 0f);
            //            }
            //        }
            //        else
            //        {
            //            float xOffset = (quadrant.X == 0 ? 0.5f : -0.5f) * screenWidth + (float)lonFraction * screenWidth;
            //            float yOffset = (quadrant.Y == 0 ? 0.5f : -0.5f) * screenHeight + (float)latFraction * screenHeight;
            //            if (debug) Log.Info($"xOffset: {xOffset}");
            //            if (debug) Log.Info($"yOffset: {yOffset}");
            //            if (debug) Log.Info($"latFraction: {latFraction}");
            //            if (debug) Log.Info($"lonFraction: {lonFraction}");

            //            //Adjust the screenwidth and height for small grid
            //            if (block.CubeGrid.GridSizeEnum == MyCubeSize.Small)
            //            {
            //                // Apply the translation first
            //                translationMatrix = Matrix.CreateTranslation((float)(lonFraction * screenWidth * 0.2 * 0.5), (float)(latFraction * screenHeight * 0.2 * 0.5), 0f);
            //            }
            //            else
            //            {
            //                // Apply the translation first
            //                //translationMatrix = Matrix.CreateTranslation((float)(quadrant.Item1 == 0 ? 0.5 : -0.5 * screenWidth), (float)(quadrant.Item2 == 0 ? 0.5 : -0.5 * screenHeight), 0f);
            //                translationMatrix = Matrix.CreateTranslation(xOffset, yOffset, 0f);
            //            }
            //        }



            //        // Combine the matrices
            //        offsetMatrix *= translationMatrix;

            //        //Keep the orientation and take the offset translation
            //        finalMatrix.Translation = offsetMatrix.Translation;

            //        //Apply the chevron scale
            //        finalMatrix = Matrix.CreateScale(NavigationScreenChevronScale) * finalMatrix;

            //        //Apply the final matrix to the subpart
            //        subpart.PositionComp.SetLocalMatrix(ref finalMatrix);

            //        //Set emissive parts on the subpart
            //        subpart.SetEmissiveParts("Emissive0", NavigationScreenChevronColor, NavigationScreenChevronStrength);
            //    }
            //}
            //catch (Exception e)
            //{
            //    //Log the error
            //    Log.Error("Error in MoveSubpart: " + e);
            //}
        }

        private void UpdateMapScreen()
        {
            //Error handling
            try
            {
                MyStringHash currentPlanetName = currentPlanet.Generator.Id.SubtypeId;
                if (debug) Log.Info("Updating map screen " + currentPlanetName);

                var mapSurface = blockAsTextSurfaceProvider.GetSurface(0); // Get the surface to display the map

                mapSurface.ClearImagesFromSelection(); // Clear the images from the screen
                if (debug)
                {

                    //textPanel.GetSurface(0).ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    mapSurface.ClearImagesFromSelection();

                    List<string> images = new List<string>();
                    mapSurface.GetSelectedImages(images);
                    Log.Info($"Images: {string.Join(", ", images)}");
                }




                //was the map found?
                if (!mapTable.TryGetValue(currentPlanetName, out planetLookup))
                {
                    //Log the error
                    //Log.Info("Error: Map not found for planet " + currentPlanetName);
                    setMapScreenToError();
                }
                else
                {
                    planetOffset = planetLookup.planetOffset; // Get the planet offset from the lookup
                    string mapName = planetLookup.mapTextureName; // Get the map name from the lookup
                    //If the mapName was found, update the map
                    if (NavigationScreenZoom == 0)
                    {
                        //If zoom 0 show the planet
                        mapSurface.AddImageToSelection(mapName);
                        if (debug) Log.Info("Zoom 0: " + mapName);
                    }
                    else
                    {
                        //If zoom 1 show the zoomed in quadrant
                        mapSurface.AddImageToSelection(mapName + "_" + quadrant.X + quadrant.Y);
                        if (debug) Log.Info("Zoom 1: " + mapName + "_" + quadrant);
                    }
                }





            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in UpdateMapSubparts: " + e);
            }

        }
        private void setMapScreenToError()
        {
            try
            {
                var textPanel = block as IMyTextSurfaceProvider; // Get the block to get the screens
                if (textPanel != null)
                {
                    //textPanel.GetSurface(0).ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    textPanel.GetSurface(0).ClearImagesFromSelection();
                    textPanel.GetSurface(0).AddImageToSelection("Error_Planet");
                }
                else
                {
                    //Log the error
                    Log.Error("Error: No text panel found");
                }
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in setMapScreenToError: " + e);
            }
        }

        private void SetMapScreenToNoPlanet()
        {
            try
            {
                var textPanel = block as IMyTextSurfaceProvider; // Get the block to get the screens
                if (textPanel != null)
                {
                    //textPanel.GetSurface(0).ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    textPanel.GetSurface(0).ClearImagesFromSelection();
                    textPanel.GetSurface(0).AddImageToSelection("Error_Gravity"); //Change soon
                }
                else
                {
                    //Log the error
                    Log.Error("Error: No text panel found");
                }
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in setMapScreenToError: " + e);
            }
        }

        private void HideSubpart()
        {
            try
            {
                if (subpart != null)
                {
                    subpart.Render.Visible = false;
                }
                else
                {
                    if (block.TryGetSubpart(subpartName, out subpart)) subpart.Render.Visible = false; // First get the subpart and then hide it
                }
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in hideSubpart: " + e);
            }
        }

        private void ShowSubpart()
        {
            try
            {
                if (subpart != null)
                {
                    subpart.Render.Visible = true;
                }
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in showSubpart: " + e);
            }
        }

        private float GetGravityAtBlock()
        {
            try
            {
                float gravityValue;
                MyAPIGateway.Physics.CalculateNaturalGravityAt(block.GetPosition(), out gravityValue);

                return gravityValue;
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in getGravityAtBlock: " + e);
                return 0;
            }
        }

        private MyPlanet FindClosestPlanet()
        {
            Vector3D blockPosition = block.GetPosition();
            MyPlanet closestPlanet = null;
            double closestDistance = double.MaxValue;

            //Log.Info("Mod.Planets: "+ Mod.Planets.Count());
            foreach (var planet in Mod.Planets)
            {
                double distance = Vector3D.Distance(blockPosition, planet.PositionComp.GetPosition());
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlanet = planet;
                }
            }

            if (closestPlanet != null)
            {
                return closestPlanet;
            }
            else
            {
                return null;
            }
        }
    }

}
