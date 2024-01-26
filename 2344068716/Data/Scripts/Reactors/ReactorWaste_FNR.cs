using ProtoBuf;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace ReactorWaste_FNR
{                                                   // Add/replace your reactor subtype IDs here
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenGenerator), false, "FastNeutronReactor")]

    public class ReactorWaste_FNR : MyGameLogicComponent
    {

        public IMyGasGenerator OxygenGenerator;
        public IMyInventory Inventory;
        public bool SetupDone = false;
        public bool FirstRun = true;
        public float LastFuelLevel = 0;
        public float CurrentFuelLevel = 0;
        public float FuelUsed = 0;
        public float WasteToAdd = 0;
        public float WasteMultiplier = 0.5f; // Ratio of Waste produced to Fuel consumed
        public MyDefinitionId WasteDefId;
        public MyDefinitionId FuelDefId;
        public MyObjectBuilder_InventoryItem WasteItem;


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {

            base.Init(objectBuilder);

            try
            {

                NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;

            }
            catch (Exception exc)
            {



            }

        }

        public override void UpdateBeforeSimulation()
        {

        }

        public override void UpdateBeforeSimulation10()
        {

            if (SetupDone == false)
            {

                SetupDone = true;
                OxygenGenerator = Entity as IMyGasGenerator;
                Inventory = (VRage.Game.ModAPI.IMyInventory)OxygenGenerator.GetInventory();
                WasteDefId = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "NuclearWaste"); // SubtypeID of the waste item to be produced
                FuelDefId = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "Uranium"); // The fuel SubtypeID the script is checking for
                var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(WasteDefId);
                WasteItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };

            }

            if (FirstRun == true)
            {

                LastFuelLevel = (float)Inventory.GetItemAmount(FuelDefId); // Initializes the script by checking starting fuel level, and waits for the next run

                FirstRun = false;

                return;

            }
            else
            {

                CurrentFuelLevel = (float)Inventory.GetItemAmount(FuelDefId); // Checks current fuel level

                FuelUsed = LastFuelLevel - CurrentFuelLevel; // Compares current fuel level against last known fuel level

                if (FuelUsed > 0 && FuelUsed <= 5)
                { // Limits values to prevent bad behaviour when adding or removing fuel normally
                  // Change the maximum value to just above whatever your maximum consumption will be

                    WasteToAdd = FuelUsed * WasteMultiplier;

                    Inventory.AddItems((MyFixedPoint)WasteToAdd, WasteItem.Content);

                }

                LastFuelLevel = (float)Inventory.GetItemAmount(FuelDefId); // Updates last known fuel level for the next run


            }


        }

        public override void OnRemovedFromScene()
        {

            base.OnRemovedFromScene();

            var Block = Entity as IMyGasGenerator;

            if (Block == null)
            {

                return;

            }

        }

        public override void OnBeforeRemovedFromContainer()
        {

            base.OnBeforeRemovedFromContainer();

            if (Entity.InScene == true)
            {

                OnRemovedFromScene();

            }

        }

    }

}