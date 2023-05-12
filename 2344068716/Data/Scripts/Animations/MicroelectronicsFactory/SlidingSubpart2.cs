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
using Sandbox.Game;
using Sandbox.Game.Entities;
using System.Collections.Generic;

namespace MicroelectronicsFactorySlideAnimation2
{
    // Edit the block type and subtypes to match your block.
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Assembler), false, "MicroelectronicsFactory")]
    public class MicroelectronicsFactorySlide2 : MyGameLogicComponent
    {
        private const string SUBPART_NAME = "Slide2"; // dummy name without the "subpart_" prefix
        private const float MAX_DISTANCE_SQ = 1000 * 1000; // player camera must be under this distance (squared) to see the subpart spinning

        private IMyProductionBlock block;
		
        private bool subpartFirstFind = true;
		private bool FirstCycle = true;
		private bool FirstTick = true;
		
        private Matrix subpartLocalMatrix;
				
        private Matrix MatrixSlide;
				
        private int SlideTime = 400; // How long the subpart slides before reversing, in ticks
        private int SlideReverseTime = 0; // How long the subpart slides while reversing, in ticks
											 // In most cases should be the same as SlideTime
        private int CurrentSlideTime = 0;
		
        private int ShortenFirstCycle = 100;

        private float MovePerTick = 0.006f; // How fast the subpart slides, in meters per tick
        private float MoveReversePerTick = 2.4f; // How fast the subpart slides, in meters per tick
												   // In most cases should be the same as MovePerTick

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            if(MyAPIGateway.Utilities.IsDedicated)
                return;

            block = (IMyProductionBlock)Entity;

            if(block.CubeGrid?.Physics == null)
                return;

            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            block = (IMyProductionBlock)Entity;
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                bool shouldSpin = block.IsProducing;

                var camPos = MyAPIGateway.Session.Camera.WorldMatrix.Translation;

                if(Vector3D.DistanceSquared(camPos, block.GetPosition()) > MAX_DISTANCE_SQ)
                    return;

                MyEntitySubpart subpart;
                if(Entity.TryGetSubpart(SUBPART_NAME, out subpart))
                {
                    if(subpartFirstFind)
                    {
                        subpartFirstFind = false;
                        subpartLocalMatrix = subpart.PositionComp.LocalMatrix;
                    }

                    if(shouldSpin)
					{	
				
						if(FirstCycle)
						{
							if(FirstTick)
							{
								MatrixSlide = Matrix.CreateTranslation(-1.29274f, -0.73055f, -0.6f); // (x, y, z) offset from center of block for first cycle
								FirstTick = false;
							}
							if (CurrentSlideTime <= (SlideTime - ShortenFirstCycle))
							{
								MatrixSlide = Matrix.CreateTranslation(MatrixSlide.Translation.X, MatrixSlide.Translation.Y, MatrixSlide.Translation.Z + MovePerTick);
								subpart.PositionComp.LocalMatrix = MatrixSlide;
							}
							else if (CurrentSlideTime > (SlideTime - ShortenFirstCycle) && CurrentSlideTime <= ((SlideTime - ShortenFirstCycle) + SlideReverseTime))
							{	
								MatrixSlide = Matrix.CreateTranslation(MatrixSlide.Translation.X, MatrixSlide.Translation.Y, MatrixSlide.Translation.Z - MoveReversePerTick);
								subpart.PositionComp.LocalMatrix = MatrixSlide;
							}
						
							else
							{
								CurrentSlideTime = 0;
								MatrixSlide = Matrix.CreateTranslation(MatrixSlide.Translation.X, MatrixSlide.Translation.Y, MatrixSlide.Translation.Z);
								subpart.PositionComp.LocalMatrix = MatrixSlide;
								FirstCycle = false;
								MatrixSlide = Matrix.CreateTranslation(-1.29274f, -0.73055f, -1.2f); // (x, y, z) offset from center of block
							}
							CurrentSlideTime += 1;	
						}
						else
						{
							if (CurrentSlideTime <= SlideTime)
							{
								MatrixSlide = Matrix.CreateTranslation(MatrixSlide.Translation.X, MatrixSlide.Translation.Y, MatrixSlide.Translation.Z + MovePerTick);
								subpart.PositionComp.LocalMatrix = MatrixSlide;
							}
							else if (CurrentSlideTime > SlideTime && CurrentSlideTime <= (SlideTime + SlideReverseTime))
							{	
								MatrixSlide = Matrix.CreateTranslation(MatrixSlide.Translation.X, MatrixSlide.Translation.Y, MatrixSlide.Translation.Z - MoveReversePerTick);
								subpart.PositionComp.LocalMatrix = MatrixSlide;
							}
						
							else
							{
								CurrentSlideTime = 0;
								MatrixSlide = Matrix.CreateTranslation(MatrixSlide.Translation.X, MatrixSlide.Translation.Y, MatrixSlide.Translation.Z);
								subpart.PositionComp.LocalMatrix = MatrixSlide;
								MatrixSlide = Matrix.CreateTranslation(-1.29274f, -0.73055f, -1.2f); // (x, y, z) offset from center of block
							}
							CurrentSlideTime += 1;	
						}
					}
				}
            }
            catch(Exception e)
			{
				
			}
        }
    }
}