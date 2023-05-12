using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Lights;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using ProtoBuf;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Utils;
using VRageMath;

namespace DeuteriumRamscoop{
	
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenGenerator), false, "DeuteriumRamscoop")]
	 
	public class DeuteriumRamscoop : MyGameLogicComponent{
		
		public IMyGasGenerator Ramscoop;
		public IMyInventory Inventory;
		public bool SetupDone = false;
		public bool CanCollect = false;
		public float BaseCollection = 0.025f;
		public MyDefinitionId IceDefId;
		public MyObjectBuilder_InventoryItem IceItem;
		public Vector3D Velocity;
		
		
		
		public override void Init(MyObjectBuilder_EntityBase objectBuilder){
			
			base.Init(objectBuilder);
			
			try{
				
				NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
				
			}catch(Exception exc){
				
				
				
			}
			
		}
		
		public override void UpdateBeforeSimulation(){
			
		}
		
		public override void UpdateBeforeSimulation10(){
			
			if (SetupDone == false){
				
				Ramscoop = Entity as IMyGasGenerator;
				Inventory = (VRage.Game.ModAPI.IMyInventory)Ramscoop.GetInventory();
				IceDefId = new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Ice");
				var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(IceDefId);
				IceItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };
				
				SetupDone = true;
				
			}
			
			try{
                
				var Grid = Ramscoop.CubeGrid;
				Velocity = Grid.Physics.LinearVelocity;

				var Orientation = Ramscoop.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Forward);
				var RayOrigin = Ramscoop.GetPosition() + (5 * Orientation);
				var RayTarget = RayOrigin + (100 * Orientation);
					
				float Grav;
				var GravityVector = MyAPIGateway.Physics.CalculateNaturalGravityAt(Ramscoop.GetPosition(), out Grav);

				if (Ramscoop.CubeGrid.RayCastBlocks(RayOrigin, RayTarget).HasValue){
                
					CanCollect = false;
				
				}
					
				else if (GravityVector != Vector3D.Zero){
						
					CanCollect = false;
						
				}
			
				else{
				
					CanCollect = true;
				
				}
			
				if ((Ramscoop.IsWorking) && (CanCollect == true)){
			
					double Speed = 0;

					if (Grid.Physics != null){
                    
						if (!Vector3D.IsZero(Velocity)){
                        
							Speed = Velocity.Length();

							var MotionAngleScalar = Vector3D.Dot(Velocity, Orientation);
							var NormalizeLengths = Velocity.Length() * Orientation.Length();
								
							var EffectiveForwardMotion = (MotionAngleScalar / NormalizeLengths);

							EffectiveForwardMotion += 1.0f;
						
							double EffectiveForwardMotionMult = (EffectiveForwardMotion * 5.0d);

							Inventory.AddItems(((MyFixedPoint)BaseCollection * (MyFixedPoint)EffectiveForwardMotionMult), IceItem.Content);
						}
					
						else{
						
							Inventory.AddItems((MyFixedPoint)BaseCollection, IceItem.Content);
						
						}
						
					}
					
                }
				
			}catch(Exception exc){
				
			}
			
        }
		
	}
	
}