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

        //Material for the drawings
        private MyStringId material = MyStringId.GetOrCompute("Square"); //I prefer round squares but this will have to suffice

        //Screen dimensions
        private static double screenWidth = 4.5;
        private static double screenHeight = screenWidth * 0.5;

        private Vector3 baseSouth = new Vector3(0f, 1f, 0f);

        //To indicate where the ship is relative to the surface
        Vector4 aboveGround = new Vector4(255, 0, 127, 1); //I like pink
        Color belowGround = Color.Brown; //Get dirty
        Color submerged = Color.White; //Blue doesn't work since you can't see it on the map

        //For rotating the indicator towards the bearing
        private Vector3 rotationAxis = Vector3.Forward; // rotation axis for the subpart

        //For the distance check
        private const float maxDistanceSquared = 100 * 100; // player camera must be closer than this distance (squared) to see the gimmick's details



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

                //Good to go
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
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

                    double latitude = Math.Asin(blockToPlanet.Y / blockToPlanet.Length()) * (180.0 / Math.PI);
                    double latFraction = latitude / 90.0;
                    double longitude = Math.Atan2(blockToPlanet.X, blockToPlanet.Z) * (180.0 / Math.PI);
                    double lonFraction = longitude / 180.0;

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

                    // Calculate the bearing
                    float blockDirection = blockAzimuth * (180.0f / (float)Math.PI);
                    float north = northAzimuth * (180.0f / (float)Math.PI);
                    float bearing = blockDirection - north;
                    if (bearing < 0)
                    {
                        bearing += 360.0f;
                    }
                    else if (bearing > 360.0f)
                    {
                        bearing -= 360.0f;
                    }

                    // Output the bearing
                    //Log.Info($"Bearing: {bearing}°");

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

                        // Apply the rotation
                        Matrix finalMatrix = Matrix.CreateFromAxisAngle(rotationAxis, MathHelper.ToRadians(bearing));
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

                        subpart.PositionComp.SetLocalMatrix(ref finalMatrix);


                        subpart.SetEmissiveParts("Emissive0", aboveGround, 10f);
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
