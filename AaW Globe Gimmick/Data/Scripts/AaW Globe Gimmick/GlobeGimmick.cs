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

namespace Digi.Examples
{
    // Edit the block type and subtypes to match your custom block.
    // For type always use the same name as the <TypeId> and append "MyObjectBuilder_" to it, don't use the one from xsi:type.
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "GlobeGimmick")]
    public class Example_SpinningSubpart : MyGameLogicComponent
    {
        private const string SUBPART_NAME = "Star"; // dummy name without the "subpart_" prefix

        // Add later
        //private const float MAX_DISTANCE_SQ = 100 * 100; // player camera must be under this distance (squared) to see the subpart spinning

        private IMyTerminalBlock block;
        private bool subpartFirstFind = true;
        private Matrix subpartLocalMatrix; // keeping the matrix here because subparts are being re-created on paint, resetting their orientations

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (MyAPIGateway.Utilities.IsDedicated)
                return;

            if (block?.CubeGrid?.Physics == null)
                return;

            block = (IMyTerminalBlock)Entity;

            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            // if (!initiated)
            if (block?.CubeGrid?.Physics == null)
                return;

            try
            {
                MyEntitySubpart subpart;
                if (Entity.TryGetSubpart(SUBPART_NAME, out subpart)) // subpart does not exist when block is in build stage
                {
                    if (subpartFirstFind) // first time the subpart was found
                    {
                        subpartFirstFind = false;
                        subpartLocalMatrix = subpart.PositionComp.LocalMatrixRef;
                        //Log.Info("Subpart found");
                    }

                    subpart.Render.Visible = !subpart.Render.Visible;

                    subpart.PositionComp.SetLocalMatrix(ref subpartLocalMatrix);

                }
                float naturalGravity = -1;
                MyAPIGateway.Physics.CalculateNaturalGravityAt(block.GetPosition(), out naturalGravity);

                Log.Info($"{naturalGravity}");
                //Test if grid is in gravity
                if (naturalGravity == -1)
                {
                    //Log.Info("No gravity");
                    return;
                }
                else
                {

                    // Vectors for the rotation
                    MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(block.GetPosition());
                    //Log.Info("Planet found: " + planet.Name);

                    if (!planet.Name.Contains("Agaris"))
                    {
                        //Log.Info("Not Agaris: " + planet.Name);
                        return;
                    }
                    else
                    {
                        Vector3D planetPosition = planet.PositionComp.GetPosition();
                        Vector3D blockPosition = block.GetPosition();

                        // Get the position of the planet
                        Log.Info($"Planet {planet.Name} Position: " + planetPosition);
                        //Calculate the angle between the block and Vector(0,1,0)
                        Vector3D blockToPlanet = Vector3D.Normalize(planetPosition - blockPosition);
                        Vector3D upVector = planet.WorldMatrix.Up;
                        Vector3D forwardVector = planet.WorldMatrix.Forward;
                        Log.Info($"Planet {planet.Name} upVector: " + upVector);
                        double latitudeAngle = Math.Acos(Vector3D.Dot(blockToPlanet, upVector));
                        //TO degrees
                        latitudeAngle = latitudeAngle * 180 / Math.PI - 90;
                        Log.Info("Latitude Angle: " + latitudeAngle);

                        //Longitude
                        double longitudeAngle = Math.Acos(Vector3D.Dot(blockToPlanet, forwardVector));
                        //TO degrees
                        longitudeAngle = longitudeAngle * 180 / Math.PI - 90;
                        Log.Info("Longitude Angle: " + longitudeAngle);

                    }

                }



            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        }

    }
}