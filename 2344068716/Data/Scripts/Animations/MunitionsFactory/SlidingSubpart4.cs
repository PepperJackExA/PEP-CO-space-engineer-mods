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

namespace MunitionsFactorySlideAnimation4
{
    // Edit the block type and subtypes to match your block.
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Assembler), false, "MunitionsFactory")]
    public class MunitionsFactorySlide4 : MyGameLogicComponent
    {
        private const string SUBPART_NAME = "Slide4"; // dummy name without the "subpart_" prefix
        private const float MAX_DISTANCE_SQ = 1000 * 1000; // player camera must be under this distance (squared) to see the subpart spinning

        private IMyProductionBlock block;
        private bool subpartFirstFind = true;
        private Matrix subpartLocalMatrix;
				
        private Matrix MatrixSlide;
				
        private int SlideTime = 30; // How long the subpart slides before reversing, in ticks
        private int SlideReverseTime = 30; // How long the subpart slides while reversing, in ticks
											 // In most cases should be the same as SlideTime
        private int CurrentSlideTime = 0;

        private float MovePerTick = 0.0032f; // How fast the subpart slides, in meters per tick
        private float MoveReversePerTick = 0.0032f; // How fast the subpart slides, in meters per tick
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
						MatrixSlide = Matrix.CreateTranslation(0f, -0.371f, 0.8f); // (x, y, z) offset from center of block
                    }

                    if(shouldSpin)
					{	
						
						if (CurrentSlideTime <= SlideTime)
						{
							MatrixSlide = Matrix.CreateTranslation(MatrixSlide.Translation.X, MatrixSlide.Translation.Y + MovePerTick, MatrixSlide.Translation.Z);
							subpart.PositionComp.LocalMatrix = MatrixSlide;
						}
						else if (CurrentSlideTime > SlideTime && CurrentSlideTime <= (SlideTime + SlideReverseTime))
						{	
							MatrixSlide = Matrix.CreateTranslation(MatrixSlide.Translation.X, MatrixSlide.Translation.Y - MoveReversePerTick, MatrixSlide.Translation.Z);
							subpart.PositionComp.LocalMatrix = MatrixSlide;
						}
						
						else
						{
							CurrentSlideTime = 0;
							MatrixSlide = Matrix.CreateTranslation(MatrixSlide.Translation.X, MatrixSlide.Translation.Y, MatrixSlide.Translation.Z);
							subpart.PositionComp.LocalMatrix = MatrixSlide;
						}
						CurrentSlideTime += 1;	
					}
				}
            }
            catch(Exception e)
			{
				
			}
        }
    }
}