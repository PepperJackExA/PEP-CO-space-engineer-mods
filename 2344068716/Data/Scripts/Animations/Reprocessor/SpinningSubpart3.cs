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

namespace ReprocessorSpinAnimation3
{
    // Edit the block type and subtypes to match your block.
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Refinery), false, "Reprocessor")]
    public class ReprocessorSpin3 : MyGameLogicComponent
    {
        private const string SUBPART_NAME = "Spin3"; // dummy name without the "subpart_" prefix
        private const float MAX_DISTANCE_SQ = 1000 * 1000; // player camera must be under this distance (squared) to see the subpart spinning

        private IMyProductionBlock block;
        private bool subpartFirstFind = true;
        private Matrix subpartLocalMatrix;
		
        private Matrix MatrixX;
        private Matrix MatrixY;
        private Matrix MatrixZ;
		
        private Matrix MatrixXReverse;
        private Matrix MatrixYReverse;
        private Matrix MatrixZReverse;
		
		private bool UseReverseRotation = true; // If the spin should reverse after the defined time
				
        private int SpinTime = 300; // How long the subpart spins before reversing, in ticks
		
        private int SpinReverseTime = 300; // How long the subpart spins in reverse, in ticks
											 // In most cases should be the same as SpinTime
        
		private int CurrentRotationTime = 0;

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

            MatrixX = Matrix.CreateRotationX(0f); // rotation speed for each axis
            MatrixY = Matrix.CreateRotationY(0.00375f);
            MatrixZ = Matrix.CreateRotationZ(0f);

            MatrixXReverse = Matrix.CreateRotationX(0f); // reverse rotation speed for each axis
            MatrixYReverse = Matrix.CreateRotationY(-0.00375f); // in most cases should be the exact negative of the normal rotation
            MatrixZReverse = Matrix.CreateRotationZ(0f);
			
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
						var offset = new Vector3(-0.899045f, -0.518086f, 4.55353f); // (x, y, z) offset from center of block
						var MatrixTrans1 = Matrix.CreateTranslation(-(offset));
						var MatrixTrans2 = Matrix.CreateTranslation(offset);
						var MatrixTotal = subpart.PositionComp.LocalMatrix;
					
						if(UseReverseRotation)
						{
							if (CurrentRotationTime <= SpinTime)
							{
								MatrixTotal *= (MatrixTrans1 * MatrixX * MatrixY * MatrixZ * MatrixTrans2);
								subpart.PositionComp.LocalMatrix = MatrixTotal;
							}

							else if (CurrentRotationTime > SpinTime && CurrentRotationTime <= (SpinTime + SpinReverseTime))
							{	
								MatrixTotal *= (MatrixTrans1 * MatrixXReverse * MatrixYReverse * MatrixZReverse * MatrixTrans2);
								subpart.PositionComp.LocalMatrix = MatrixTotal;
							}
						
							else
							{
								CurrentRotationTime = 0;
							}
							
							CurrentRotationTime += 1;	
						}
						
						else
						{
							MatrixTotal *= (MatrixTrans1 * MatrixX * MatrixY * MatrixZ * MatrixTrans2);
							subpart.PositionComp.LocalMatrix = MatrixTotal;
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