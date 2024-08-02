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
using Sandbox.Game.Entities.Cube;
using VRage.Game.ModAPI;
using CollisionLayers = Sandbox.Engine.Physics.MyPhysics.CollisionLayers;



namespace PEPCO
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false, "MercatorMap", "SG_MercatorMap", "SG_MercatorMap_Large")]
    public class NavigationScreenLogic : MyGameLogicComponent
    {


        private const bool debug = false; //STOP SPAMMING ME!

        //The entities
        IMyTerminalBlock block; // the block itself
        IMyTextSurfaceProvider blockAsTextSurfaceProvider; // the block as a text surface provider

        // Update the MapLookup class to include the subpartName and planetOffset properties
        class MapLookup
        {
            public string mapTextureName { get; set; }
            public int planetOffset { get; set; }
        }

        // Update the mapTable initialization code to use the MapLookup class
        private Dictionary<MyStringHash, MapLookup> mapTable = new Dictionary<MyStringHash, MapLookup>()
        {
            { MyStringHash.GetOrCompute("Planet Agaris"), new MapLookup { mapTextureName = "AaWAgaris_Planet", planetOffset = 0 } },
            { MyStringHash.GetOrCompute("Agaris II"), new MapLookup { mapTextureName = "AaWAgarisII_Planet", planetOffset = 0 } },
            { MyStringHash.GetOrCompute("Planet Crait"), new MapLookup { mapTextureName = "AaWCrait_Planet", planetOffset = 1 } },
            { MyStringHash.GetOrCompute("Planet Lezuno"), new MapLookup { mapTextureName = "AaWLezuno_Planet", planetOffset = 1 } },
            { MyStringHash.GetOrCompute("Planet Lorus"), new MapLookup { mapTextureName = "AaWLorus_Planet", planetOffset = 1 } },
            { MyStringHash.GetOrCompute("Planet Thora 4"), new MapLookup { mapTextureName = "AaWThora4_Planet", planetOffset = 1 } }
        };


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
        public double latitude; //For arranging the chevron on the map
        public double longitude;//For arranging the chevron on the map
        public double latitudeFraction; //For arranging the chevron on the map
        public double longitudeFraction; //For arranging the chevron on the map
        public float heading; //For rotating the chevron towards the heading
        public float gravity; //For rotating the chevron towards the heading
        public string mapName; //For the map name

        private bool inGravity = false; //For checking if the block is in gravity
        string longitudeOutput; 
        string latitudeOutput;

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
        public MyPlanet currentPlanet;

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

            

            // If there is no planet, return
            if (currentPlanet == null)
            {
                builder.AppendLine($"Planet: none\n");
                return;
            }
            else
            {
                //builder.Clear();
                builder.Append($"Planet: {currentPlanet?.Generator?.Id.SubtypeName.ToString()}\n" +
                    $"Heading: {MathHelper.ToDegrees(heading)}°\n" +
                    $"Longitude: {longitudeOutput}\n" +
                    $"Latitude: {latitudeOutput}\n" +
                    $"Zoom: {NavigationScreenZoom}\n" +
                    $"Misc: \n" +
                    $"Planet offset: {planetOffset}");
            }


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

                HideNavigationScreenControls.DoOnce(); //Hide the default terminal controls
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
                    ApplyDefaultBlockScript(); // Apply the default script to the block
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

        private void ApplyDefaultBlockScript()
        {
            try
            {
                blockAsTextSurfaceProvider.GetSurface(0).ContentType = VRage.Game.GUI.TextPanel.ContentType.SCRIPT; // Set the content type to script
                blockAsTextSurfaceProvider.GetSurface(0).FontSize = 1.0f; // Set the font size
                blockAsTextSurfaceProvider.GetSurface(0).Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER; // Set the alignment
                blockAsTextSurfaceProvider.GetSurface(0).TextPadding = 0.0f; // Set the text padding
                blockAsTextSurfaceProvider.GetSurface(0).Script = "NavigationScreenTSS"; // Set the script to the NavigationScreenTSS
                blockAsTextSurfaceProvider.GetSurface(0).ScriptBackgroundColor = new Color(0, 0, 0); // Set the script background color
            }
            catch (Exception e)
            {
                Log.Error("Error in ApplyDefaultBlockScript: " + e);
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
                if (debug) Log.Info("UpdateBeforeSimulation10");
                SyncSettings();

                // If the block is not valid, return
                if (block?.CubeGrid?.Physics == null || block.MarkedForClose || block.Closed) return;

                // If the block is too far away, return
                if (Vector3D.DistanceSquared(MyAPIGateway.Session.Camera.Position, block.GetPosition()) > maxDistanceSquared) return;


                try
                {

                    if (BlockInGravity())
                    {

                        currentPlanet = FindClosestPlanet(); // Find the closest planet

                        if (currentPlanet != null) GetNavigationData(); // Get the navigation data

                    }
                    else
                    {
                        // Clear the last planet
                        currentPlanet = null;
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

        private void GetNavigationData()
        {
            try
            {
                // Define a small threshold to account for floating-point precision errors
                const double epsilon = 1e-10;

                Vector3D blockPosition = block.GetPosition(); //Block position as vector                
                Vector3D planetPosition = currentPlanet.PositionComp.GetPosition(); //Planet position as vector
                Vector3D blockToPlanet = blockPosition - planetPosition; // Get the vector from the block to the planet
                Vector3D normalizedBlockToPlanet; //Normalized vector for the calculation of the latitude and longitude

                double lengthSquared = blockToPlanet.LengthSquared(); // Calculate the length squared of the vector

                // Normalize the vector if its length squared is greater than the threshold
                if (lengthSquared > epsilon)
                {
                    normalizedBlockToPlanet = blockToPlanet / Math.Sqrt(lengthSquared);
                }
                else
                {
                    // Handle the case where the vector length is too small
                    normalizedBlockToPlanet = Vector3D.Zero;
                }

                // Calculate latitude
                latitude = Math.Asin(normalizedBlockToPlanet.Y);
                latitudeFraction = latitude / (Math.PI / 2);

                //For the custom data
                latitudeOutput = Math.Abs(MathHelper.ToDegrees(latitude)).ToString("0.00") + "° " + (latitude > 0 ? "N" : "S");

                // Calculate longitude
                if (planetOffset == 0) // Agaris and Moon
                {
                    //Log.Info("Agaris and Moon");
                    longitude = Math.Atan2(normalizedBlockToPlanet.X, -normalizedBlockToPlanet.Z);
                }
                else if (planetOffset == 1) // Lezuno
                {
                    //Log.Info("Lezuno");
                    // was longitude = Math.Atan2(normalizedBlockToPlanet.Z, -normalizedBlockToPlanet.X);
                    // tried longitude = Math.Atan2(normalizedBlockToPlanet.Z, -normalizedBlockToPlanet.X);
                    longitude = Math.Atan2(-normalizedBlockToPlanet.Z, -normalizedBlockToPlanet.X); // THis seems to work
                }

                longitudeOutput = Math.Abs(MathHelper.ToDegrees(longitude)).ToString("0.00") + "° " + (longitude*-1 > 0 ? "E" : "W");

                // Output the results

                longitudeFraction = longitude / Math.PI;

                // Find the direction the block is facing
                Vector3 blockRotForwardVector = block.WorldMatrix.Rotation.Backward;

                // Get block position normal (up in reference to the planet)
                Vector3D relativeBlockPos = block.GetPosition() - planetPosition;
                Vector3 relativeBlockPosNormal = Vector3.Normalize(relativeBlockPos);

                // Get north normal (north relative to the planet)
                Vector3D relativeNorth = planetPosition + new Vector3D(0f, currentPlanet.AverageRadius, 0f);

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
                heading = blockAzimuth - northAzimuth;

                heading *= -1; //For some reason the rotation is off

                //Add another correction 
                heading -= (float)(Math.PI / 2);

                //Accounting for the block orientation
                heading -= (float)(NavigationScreenOffset * 0.5 * Math.PI);

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


            }
            

            catch (Exception e)
            {
                //Log the error with stack trace
                Log.Error("Error in GetNavigationData: " + e + "\n" + e.StackTrace);
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


        private bool BlockInGravity()
        {
            try
            {
                float gravityValue;
                MyAPIGateway.Physics.CalculateNaturalGravityAt(block.GetPosition(), out gravityValue);

                return gravityValue > 0;
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in getGravityAtBlock: " + e);
                return false;
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
                if (debug) Log.Info("Closest planet: " + closestPlanet.Generator.Id.SubtypeName);

                MapLookup mapLookup;

                //Set the mapName to the closest planet lookup
                if (mapTable.TryGetValue(closestPlanet.Generator.Id.SubtypeId, out mapLookup))
                {
                    mapName = mapLookup.mapTextureName;
                    planetOffset = mapLookup.planetOffset;
                }
                else
                {
                    mapName = "Error_Planet";
                    planetOffset = 0;
                }

                return closestPlanet;
            }
            else
            {
                return null;
            }
        }
    }

}
