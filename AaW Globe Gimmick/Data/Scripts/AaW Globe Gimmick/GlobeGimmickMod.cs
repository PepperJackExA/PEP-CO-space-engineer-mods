using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;
using Digi;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using VRage.Game.Entity; // required for MyTransparentGeometry/MySimpleObjectDraw to be able to set blend type.

namespace PEPCO
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class GlobeGimmickSession : MySessionComponentBase
    {

        //Subpart name
        private const string SUBPART_NAME = "MercPlane";

        SubpartMover mover = new SubpartMover();

        //Subpart operation
        private bool subpartFirstFind = true;

        //Subpart orientation
        private MatrixD subpartWorldMatrix; // keeping the matrix here because subparts are being re-created on paint, resetting their orientations

        //List of GlobeGimmicks
        public List<IMyTerminalBlock> globeGimmicks = new List<IMyTerminalBlock>();

        //List of Maps
        public List<IMyTerminalBlock> mercatorMaps = new List<IMyTerminalBlock>();

        //Material for the drawings
        private MyStringId Material = MyStringId.GetOrCompute("Square");


        //Because I like the color red
        Color aboveGround = Color.HotPink.ToVector4();
        Color belowGround = Color.Brown.ToVector4();

        private readonly Vector3 ROTATION_AXIS = Vector3.Forward; // rotation axis for the subpart

        public static GlobeGimmickSession Instance { get; private set; }

        private const float MAX_DISTANCE_SQ = 100 * 100; // player camera must be under this distance (squared) to see the gimmick's details

        public override void LoadData()
        {
            Instance = this;
        }

        protected override void UnloadData()
        {
            //Clean out after yourself
            globeGimmicks.Clear();
            Instance = null;
        }

        public bool RegisterGlobe(IMyTerminalBlock block)
        {
            globeGimmicks.Add(block);
            return true;
        }

        public bool RegisterMap(IMyTerminalBlock block)
        {
            mercatorMaps.Add(block);
            return true;
        }

        public override void Draw()
        {
            // Iterate backwards through the GlobeGimmicks
            for (int i = globeGimmicks.Count - 1; i >= 0; i--)
            {
                var block = globeGimmicks[i];

                // If the block is not valid, remove it from the list
                if (block?.CubeGrid?.Physics == null || block.MarkedForClose || block.Closed)
                {
                    //Remove the GlobeGimmick from the list
                    globeGimmicks.RemoveAtFast(i);
                    continue;
                }

                // If the block is visible, draw the GlobeGimmick
                if (Vector3D.DistanceSquared(MyAPIGateway.Session.Camera.Position, block.GetPosition()) > MAX_DISTANCE_SQ)
                {
                    //Log.Info(block.CustomName + " isn't visible");
                    continue;
                }


                //Draw the GlobeGimmick
                //DrawGlobeGimmick(block);
            }

            // Iterate backwards through the mercatorMaps
            for (int i = mercatorMaps.Count - 1; i >= 0; i--)
            {
                var block = mercatorMaps[i];

                // If the block is not valid, remove it from the list
                if (block?.CubeGrid?.Physics == null || block.MarkedForClose || block.Closed)
                {
                    //Remove the GlobeGimmick from the list
                    mercatorMaps.RemoveAtFast(i);
                    continue;
                }

                // If the block is visible, draw the GlobeGimmick
                if (Vector3D.DistanceSquared(MyAPIGateway.Session.Camera.Position, block.GetPosition()) > MAX_DISTANCE_SQ)
                {
                    //Log.Info(block.CustomName + " isn't visible");
                    continue;
                }


                block.SetEmissivePartsForSubparts("Emissive", Color.Pink, 10f);
                //Draw the GlobeGimmick
                mover.MoveSubpart(block);
            }
        }


        //Handle the mercator maps
        //public void DrawMercatorMap(IMyTerminalBlock block)
        //{
        //    //Catch errors
        //    try
        //    {
        //        //For testing, remove soon
        //        //Log.Info(block.CustomName + " is visible");

        //        //Get subpart
        //        MyEntitySubpart subpart;



        //        //Screen dimensions
        //        double screenWidth = 4.5;
        //        double screenHeight = screenWidth*0.5;

        //        //Vector for the block position
        //        Vector3D blockPosition = block.GetPosition();

        //        // Vectors for the rotation
        //        MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(blockPosition);

        //        //Test if the closest planet has Agaris in its name
        //        if (planet != null && planet.Name.Contains("Agaris"))
        //        {
        //            //Get the Planet position
        //            Vector3D planetPosition = planet.PositionComp.GetPosition();

        //            // Get the vector from the block to the planet
        //            Vector3D blockToPlanet = Vector3D.Normalize(blockPosition - planetPosition);

        //            double latitude = Math.Asin(blockToPlanet.Y / blockToPlanet.Length()) * (180.0 / Math.PI);
        //            double latFraction = latitude / 90.0;
        //            double longitude = Math.Atan2(blockToPlanet.X, blockToPlanet.Z) * (180.0 / Math.PI);
        //            double lonFraction = longitude / 180.0;

        //            Log.Info("Latitude: " + latitude.ToString("N2") + " Longitude: " + longitude.ToString("N2"), "Latitude: " + latitude.ToString("N2") + " Longitude: " + longitude.ToString("N2"), 10);

        //            ////Switch the color based on the distance
        //            Color appliedColor = Color.Pink * 10f;

        //            //// Scale the orientation vector
        //            Vector3D offsetVector = block.PositionComp.GetOrientation().Backward * 1.12 + block.PositionComp.GetOrientation().Up * latFraction * screenHeight*0.5 + block.PositionComp.GetOrientation().Left * lonFraction *screenWidth * 0.5;

        //            //If the subpart is found
        //            if (block.TryGetSubpart(SUBPART_NAME, out subpart))
        //            {
        //                //Log if found
        //                Log.Info("Subpart found");

        //                //subpartWorldMatrix = block.PositionComp.WorldMatrixRef;

        //                ////Subpart to screen offset
        //                //Vector3D subpartScreenOffset = new Vector3D((1 * lonFraction * screenWidth * 0.5), (1 * latFraction * screenHeight * 0.5), 0);

        //                ////Rotate the subpartScreenOffset by the block's rotation
        //                //subpartScreenOffset = Vector3D.Transform(subpartScreenOffset, block.WorldMatrix.GetOrientation());

        //                //subpartWorldMatrix.Translation += subpartScreenOffset;

        //                //Compare subpart and part local matrix translation
        //                Log.Info("Subpart local matrix translation: " + subpart.PositionComp.WorldMatrixRef.Translation +"\n"
        //                     + block.PositionComp.WorldMatrixRef.Translation + "\n" +
        //                     Vector3D.Distance((Vector3D)block.PositionComp.WorldMatrixRef.Translation, (Vector3D)subpart.PositionComp.WorldMatrixRef.Translation).ToString());


        //                //Apply the subpartWorldMatrix to the subpart
        //                subpart.SetEmissiveParts("Emissive", Color.Pink, 10f);
        //                subpart.WorldMatrix = blockPosition + offsetVector;


        //            }



        //            MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("WeaponLaser"), appliedColor.ToVector4()*100, blockPosition + offsetVector, block.PositionComp.GetOrientation().Forward, 0.1f, 0.01f, blendType: BlendTypeEnum.SDR);

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        //Log the error
        //        Log.Error("Error in DrawMercatorMap: " + e);
        //    }
        //}

        //Function to handle the drawing of the GlobeGimmick
        public void DrawGlobeGimmick(IMyTerminalBlock block)
        {


            //Catch errors 
            try
            {
                //For testing, remove soon
                //Log.Info(block.CustomName + " is visible");

                //Vector for the block position
                Vector3D blockPosition = block.GetPosition();

                // Vectors for the rotation
                MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(blockPosition);

                //Test if the closest planet has Agaris in its name
                if (planet != null && planet.Name.Contains("Agaris"))
                {
                    //Get the Planet position
                    Vector3D planetPosition = planet.PositionComp.GetPosition();

                    // Get the vector from the block to the planet
                    Vector3D blockToPlanet = Vector3D.Normalize(blockPosition - planetPosition);

                    //Get the distance from the block to the planet
                    double distance = Vector3D.Distance(blockPosition, planetPosition);

                    //Get the max radius of the planet
                    double planetMaxRadius = planet.MaximumRadius;

                    //Get the min radius of the planet
                    double planetRadiusAtBlock = Vector3D.Distance(planet.GetClosestSurfacePointGlobal(blockPosition),planetPosition);

                    //Get the default radius of the planet
                    double planetDefaultRadius = planet.AverageRadius;

                    //Get the height of the planet closest to the block
                    double height  = planet.GetHeightFromSurface(blockPosition);

                    //Distance over block


                    //Rotate the blockToPlanet vector by the block's rotation
                    blockToPlanet = Vector3D.Transform(blockToPlanet, block.WorldMatrix.GetOrientation());

                    // Move the matrix to the block position and offset by the vector to the planet
                    MatrixD matrix = MatrixD.CreateWorld(blockPosition + blockToPlanet, block.WorldMatrix.Forward, block.WorldMatrix.Up);

                    //Switch the color based on the distance
                    Color appliedColor = distance > planetRadiusAtBlock ? aboveGround : belowGround;


                    //Log.Info(block.CustomName + " is visible");

                    MyEntitySubpart subpart;
                    if (block.TryGetSubpart(SUBPART_NAME, out subpart)) // subpart does not exist when block is in build stage
                    {
                        // Log if found
                        //Log.Info("Subpart found");

                        if (subpartFirstFind) // first time the subpart was found
                        {
                            subpartFirstFind = false;
                            subpartWorldMatrix = subpart.PositionComp.LocalMatrixRef;
                        }

                        //Calculate angle between world north and the subpart forward vector
                        double angle = Vector3D.Angle(Vector3D.TransformNormal(Vector3D.Forward, subpart.PositionComp.WorldMatrixRef), Vector3D.Forward);

                        //Creat a rotation speed to reduce the angle to 0 based on the magnitude of the angle
                        double rotationSpeed = MathHelper.Clamp(angle / 10, 0.001, 0.1);

                        //Log subpart world matrix
                        //Log.Info("Subpart world matrix: " + subpart.PositionComp.WorldMatrixRef);

                        MatrixD newOrientation = MatrixD.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(0, 90, 0));
                        MatrixD newMatrix = MatrixD.CreateWorld(subpart.WorldMatrix.Translation, block.WorldMatrix.Up, block.WorldMatrix.Forward);

                        //Log new matrix
                        //Log.Info("New matrix: " + newMatrix);

                        subpartWorldMatrix = MatrixD.CreateWorld(subpart.PositionComp.LocalMatrixRef.Translation, block.WorldMatrix.Forward, block.WorldMatrix.Up);

                        //subpart.PositionComp.SetLocalMatrix(ref subpartWorldMatrix);

                    }

                    //MySimpleObjectDraw.DrawTransparentCylinder(ref matrix, baseRadius, topRadius, height, ref color, wireframe, wireDivRatio, wireframeThickness, Material);
                    //MySimpleObjectDraw.DrawTransparentSphere(ref matrix, (float)0.01f, ref color, MySimpleObjectRasterizer.Solid, 20, Material, Material, 0.005f, blendType: BlendTypeEnum.PostPP);
                    MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("WeaponLaser"), appliedColor, blockPosition + blockToPlanet, blockToPlanet, 0.1f, 0.01f, blendType: BlendTypeEnum.SDR);

                    //Right from the block's position / orientation
                    MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("WeaponLaser"), Color.Red, blockPosition * 1.1f, Vector3D.Transform(block.WorldMatrix.Right, block.WorldMatrix.GetOrientation()), 0.1f, 0.01f, blendType: BlendTypeEnum.SDR);

                    //Up from the block's position / orientation
                    MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("WeaponLaser"), Color.Green, blockPosition + blockToPlanet * 1.1f, Vector3D.Transform(block.WorldMatrix.Up, block.WorldMatrix.GetOrientation()), 0.1f, 0.01f, blendType: BlendTypeEnum.SDR);

                    //Forward from the block's position / orientation
                    MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("WeaponLaser"), Color.RoyalBlue, blockPosition + blockToPlanet * 1.1f, Vector3D.Transform(block.WorldMatrix.Forward, block.WorldMatrix.GetOrientation()), 0.1f, 0.01f, blendType: BlendTypeEnum.SDR);

                }
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in DrawGlobeGimmick: " + e);
            }


        }
    }
    public class SubpartMover
    {
        private Matrix subpartWorldMatrix;
        private bool subpartFirstFind = false;

        //Get subpart
        MyEntitySubpart subpart;

        //Subpart name
        private const string SUBPART_NAME = "MercPlane";

        public void MoveSubpart(IMyTerminalBlock block)
        {

            try
            {
                //Screen dimensions
                double screenWidth = 4.5;
                double screenHeight = screenWidth * 0.5;

                //Vector for the block position
                Vector3D blockPosition = block.GetPosition();

                // Vectors for the rotation
                MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(blockPosition);

                //Test if the closest planet has Agaris in its name
                if (planet != null && planet.Name.Contains("Agaris"))
                {
                    //Get the Planet position
                    Vector3D planetPosition = planet.PositionComp.GetPosition();

                    // Get the vector from the block to the planet
                    Vector3D blockToPlanet = Vector3D.Normalize(blockPosition - planetPosition);

                    double latitude = Math.Asin(blockToPlanet.Y / blockToPlanet.Length()) * (180.0 / Math.PI);
                    double latFraction = latitude / 90.0;
                    double longitude = Math.Atan2(blockToPlanet.X, blockToPlanet.Z) * (180.0 / Math.PI);
                    double lonFraction = longitude / 180.0;

                    //Log.Info("Latitude: " + latitude.ToString("N2") + " Longitude: " + longitude.ToString("N2"), "Latitude: " + latitude.ToString("N2") + " Longitude: " + longitude.ToString("N2"), 10);

                    ////Switch the color based on the distance
                    Color appliedColor = Color.Pink * 10f;

                    //// Scale the orientation vector
                    //Vector3D offsetVector = block.PositionComp.GetOrientation().Backward * 1.12 + block.PositionComp.GetOrientation().Up * latFraction * screenHeight * 0.5 + block.PositionComp.GetOrientation().Left * lonFraction * screenWidth * 0.5;

                    //If the subpart is found
                    if (block.TryGetSubpart(SUBPART_NAME, out subpart))
                    {
                        //Log if found
                        //Log.Info("Subpart found");

                        if (!subpartFirstFind)
                        {
                            // Store the original position only once
                            subpartWorldMatrix = subpart.PositionComp.LocalMatrixRef;
                            subpartFirstFind = true;
                        }

                        // move the original position to the right by 1 meter and store it in the offsetMatrix
                        Matrix offsetMatrix = Matrix.CreateTranslation((float)(-1 * latFraction * screenHeight * 0.5), 0, (float)(-1 * lonFraction * screenWidth * 0.5)) * subpartWorldMatrix;

                        // apply the offsetMatrix to the subpart
                        subpart.PositionComp.SetLocalMatrix(ref offsetMatrix);
                    }



                    //MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("WeaponLaser"), appliedColor.ToVector4() * 100, blockPosition + offsetVector, block.PositionComp.GetOrientation().Forward, 0.1f, 0.01f, blendType: BlendTypeEnum.SDR);
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