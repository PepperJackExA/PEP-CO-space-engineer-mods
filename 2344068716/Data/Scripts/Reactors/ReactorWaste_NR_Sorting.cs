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

namespace ReactorWaste_Sort{													// Add/replace your reactor subtype IDs here
	
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenGenerator), false, "NuclearReactor")]
	 
	public class ReactorWaste_Sort : MyGameLogicComponent{
		
		public IMyGasGenerator OxygenGenerator;
		public IMyInventory Inventory;
		public bool SetupDone = false;
		public float CurrentWasteLevel = 0;
		public float CurrentFuelLevel = 0;
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
				OxygenGenerator = Entity as IMyGasGenerator;
				Inventory = (VRage.Game.ModAPI.IMyInventory)OxygenGenerator.GetInventory();
				WasteDefId = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "SpentFuel"); // SubtypeID of the waste item to be produced
				FuelDefId = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "Uranium"); // The fuel SubtypeID the script is checking for
				var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(WasteDefId);
				WasteItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };
								
			}
				
			CurrentFuelLevel = (float)Inventory.GetItemAmount(FuelDefId); // Checks current fuel level
				
			if(CurrentFuelLevel != 0){

				Inventory.TransferItemTo(Inventory, 1, 0, false, (MyFixedPoint)CurrentFuelLevel, true);

			}

		}

	}

}