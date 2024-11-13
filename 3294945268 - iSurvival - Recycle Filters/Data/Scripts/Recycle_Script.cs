using Sandbox.Game.Entities;
using System;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.Components;
using VRage.Game;
using VRage;
using Sandbox.Game.Components;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Utils;

namespace PEPCO.iSurvival.RecycleFilters
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class iSurvivalRecycleSession : MySessionComponentBase
    {
        public static int runCount = 0;
        public static int loadWait = 120;
        public static float waterValueLast = 0;
        public static float hungerValueLast = 0;

        public override void UpdateAfterSimulation()
        {
            try
            {
                if (++runCount % 15 > 0) // Run every quarter of a second
                    return;

                if (!MyAPIGateway.Multiplayer.IsServer && loadWait > 0) // Delay loading message handler for clients
                {
                    if (--loadWait == 0)
                        return;
                }

                if (runCount % 600 > 0) // Run 10 every second
                    return;

                ProcessPlayers();

                if (runCount > 299)
                    runCount = 0;
            }
            catch { }
        }

        private void ProcessPlayers()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players, p => p.Character != null && !(p.Character.IsDead) && p.Character.ToString().Contains("Astronaut"));

            foreach (IMyPlayer player in players)
            {               
                    var statComp = player.Character?.Components.Get<MyEntityStatComponent>();
                    if (statComp == null)
                        continue;

                    MyEntityStat hunger, water;
                    if (
                        !statComp.TryGetStat(MyStringHash.GetOrCompute("Hunger"), out hunger) ||
                        !statComp.TryGetStat(MyStringHash.GetOrCompute("Water"), out water)
                        continue;

                    ProcessOrganicCollection(player, hunger, water);        
                               
            }
        }

        private void ProcessOrganicCollection(IMyPlayer player, MyEntityStat hunger, MyEntityStat water)
        {
            string recycleItem = "PhysicalGunObject/Recycler_Tier1";
            var inventory = player.Character.GetInventory();
            float CurrentWaterValue = water.Value;
            float currentHungerValue = hunger.Value;
            MyDefinitionId requiredItemDefinition = MyDefinitionId.Parse($"{recycleItem}");
            MyFixedPoint requiredItemAmount = 1;

            if (inventory.ContainItems(requiredItemAmount, requiredItemDefinition))
            {
                if (water.Value < waterValueLast)
                {
                    inventory.AddItems((MyFixedPoint)((waterValueLast - CurrentWaterValue)/10),
                        (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Ice"))); 

                    //MyAPIGateway.Utilities.ShowMessage("water", $"Collected {(waterValueLast - CurrentWaterValue)} last:{waterValueLast} current: {CurrentWaterValue}");
                }
                if (hunger.Value < hungerValueLast)
                {
                    inventory.AddItems((MyFixedPoint)((hungerValueLast - currentHungerValue)/10),
                    (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Organic")));
                    //MyAPIGateway.Utilities.ShowMessage("hunger", $"Collected {(hungerValueLast - currentHungerValue)}  last:{hungerValueLast} current: {currentHungerValue}");
                }                

            }
            waterValueLast = CurrentWaterValue;
            hungerValueLast = currentHungerValue;            
        }
    }
}
