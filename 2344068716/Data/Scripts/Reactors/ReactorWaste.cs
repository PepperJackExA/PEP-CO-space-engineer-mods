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

namespace ReactorWaste{													// Add/replace your reactor subtype IDs here
	
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), false, "SmallCompactReactor","LargeCompactReactor","SmallRTG","LargeRTG","LargeBlockLargeGenerator","SmallBlockLargeGenerator","LargeBlockSmallGenerator","SmallBlockSmallGenerator","SmallCompactReactorWarfare2","LargeCompactReactorWarfare2","SmallRTGWarfare2","LargeRTGWarfare2","LargeBlockLargeGeneratorWarfare2","SmallBlockLargeGeneratorWarfare2","LargeBlockSmallGeneratorWarfare2","SmallBlockSmallGeneratorWarfare2")]
	public class ReactorWaste : MyGameLogicComponent{
		
		public IMyReactor Reactor;
		public IMyInventory Inventory;
		public bool SetupDone = false;
		public bool FirstRun = true;
		public float LastFuelLevel = 0;
		public float CurrentFuelLevel = 0;
		public float FuelUsed = 0;
		public float WasteToAdd = 0;
		public float WasteMultiplier = 1; // Ratio of Waste produced to Fuel consumed
		public MyDefinitionId WasteDefId;
		public MyDefinitionId FuelDefId;
		public MyObjectBuilder_InventoryItem WasteItem;
		
		
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
			
			if(SetupDone == false){
				
				SetupDone = true;
				Reactor = Entity as IMyReactor;
				Inventory = (VRage.Game.ModAPI.IMyInventory)Reactor.GetInventory();
				WasteDefId = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "SpentFuel"); // SubtypeID of the waste item to be produced
				FuelDefId = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "Uranium"); // The fuel SubtypeID the script is checking for
				var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(WasteDefId);
				WasteItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };
								
			}
			
			if(FirstRun == true){
				
				LastFuelLevel = (float)Inventory.GetItemAmount(FuelDefId); // Initializes the script by checking starting fuel level, and waits for the next run
			
			    FirstRun = false;
				
				return;
				
			}else{
				
				CurrentFuelLevel = (float)Inventory.GetItemAmount(FuelDefId); // Checks current fuel level
			
				FuelUsed = LastFuelLevel - CurrentFuelLevel; // Compares current fuel level against last known fuel level
				
				if(FuelUsed > 0 && FuelUsed <= 5){ // Limits values to prevent bad behaviour when adding or removing fuel normally
												  // Change the maximum value to just above whatever your maximum consumption will be
					
					WasteToAdd = FuelUsed * WasteMultiplier;
					
					Inventory.AddItems((MyFixedPoint)WasteToAdd, WasteItem.Content);
				
				}
				
				LastFuelLevel = (float)Inventory.GetItemAmount(FuelDefId); // Updates last known fuel level for the next run
				
				
			}
			
			
		}
		
		public override void OnRemovedFromScene(){
			
			base.OnRemovedFromScene();
			
			var Block = Entity as IMyReactor;
			
			if(Block == null){
				
				return;
				
			}
			
		}
		
		public override void OnBeforeRemovedFromContainer(){
			
			base.OnBeforeRemovedFromContainer();
			
			if(Entity.InScene == true){
				
				OnRemovedFromScene();
				
			}
			
		}
		
	}
	
}