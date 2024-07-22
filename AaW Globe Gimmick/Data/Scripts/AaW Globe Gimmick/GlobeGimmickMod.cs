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
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum; // required for MyTransparentGeometry/MySimpleObjectDraw to be able to set blend type.

namespace PEPCO
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class GlobeGimmickSession : MySessionComponentBase
    {

        //List of GlobeGimmicks
        public List<IMyTerminalBlock> GlobeGimmicks = new List<IMyTerminalBlock>();

        //Material for the GlobeGimmick
        private MyStringId Material = MyStringId.GetOrCompute("Square");

        public static GlobeGimmickSession Instance { get; private set; }

        private const float MAX_DISTANCE_SQ = 1000 * 1000; // player camera must be under this distance (squared) to see the gimmick's details

        public override void LoadData()
        {
            Instance = this;
        }

        protected override void UnloadData()
        {
            //Clean out after yourself
            GlobeGimmicks.Clear();
            Instance = null;
        }

        public bool RegisterGlobe(IMyTerminalBlock block)
        {
            GlobeGimmicks.Add(block);
            return true;
        }

        public override void Draw()
        {
            // Iterate backwards through the GlobeGimmicks
            for (int i = GlobeGimmicks.Count - 1; i >= 0; i--)
            {
                var block = GlobeGimmicks[i];

                // If the block is not valid, remove it from the list
                if (block?.CubeGrid?.Physics == null || block.MarkedForClose || block.Closed)
                {
                    //Remove the GlobeGimmick from the list
                    GlobeGimmicks.RemoveAtFast(i);
                    continue;
                }

                // If the block is visible, draw the GlobeGimmick
                if (Vector3D.DistanceSquared(MyAPIGateway.Session.Camera.Position, block.GetPosition()) > MAX_DISTANCE_SQ)
                {
                    //Log.Info(block.CustomName + " isn't visible");
                    continue;
                }

                //Draw the GlobeGimmick
                DrawGlobeGimmick(block);
            }
        }

        //Function to handle the drawing of the GlobeGimmick
        public void DrawGlobeGimmick(IMyTerminalBlock block)
        {


            //Catch errors 
            try
            {
                //For testing, remove soon
                Log.Info(block.CustomName + " is visible");

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

                    //Get the radius of the planet
                    double planetRadius = planet.MaximumRadius;

                    //Get the height of the planet closest to the block
                    double height  = planet.GetHeightFromSurface(blockPosition);

                    //Rotate the blockToPlanet vector by the block's rotation
                    blockToPlanet = Vector3D.Transform(blockToPlanet, block.WorldMatrix.GetOrientation());

                    // Move the matrix to the block position and offset by the vector to the planet
                    MatrixD matrix = MatrixD.CreateWorld(blockPosition + blockToPlanet, block.WorldMatrix.Forward, block.WorldMatrix.Up);

                    //Because I like the color red
                    var color = (Color.Red * 100);


                    //MySimpleObjectDraw.DrawTransparentCylinder(ref matrix, baseRadius, topRadius, height, ref color, wireframe, wireDivRatio, wireframeThickness, Material);
                    MySimpleObjectDraw.DrawTransparentSphere(ref matrix, (float)0.01f, ref color, MySimpleObjectRasterizer.Solid, 20, Material, Material, 0.005f, blendType: BlendTypeEnum.PostPP);
                }
            }
            catch (Exception e)
            {
                //Log the error
                Log.Error("Error in DrawGlobeGimmick: " + e);
            }


        }
    }

}