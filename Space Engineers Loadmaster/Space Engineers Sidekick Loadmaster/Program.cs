using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        string excludedInventoryTag = "Excl";
        string primaryConnectorTag = "PrimCo"; //This better be unique or else you void any non-existing warranty

        // You shall not pass this line

        double totalCargoVolume;
        double totalCargoVolumeLarge;
        double totalCargoVolumeSmall;
        double totalVolumeRequired;
        string customDataHold = "";
        List<IMyTerminalBlock> cargoContainerList = new List<IMyTerminalBlock>();
        IMyShipConnector primaryConnector;
        MyItemType itemLarge = MyItemType.MakeComponent("SteelPlate");
        MyItemType itemSmall = MyItemType.MakeComponent("Construction");
        MyItemType itemTest = MyItemType.MakeComponent("LargeTube");

        // The three different connections a cargo container can have
        enum conveyorState
        {
            not,
            small,
            large
        }

        /// <summary>
        /// Represents the cargo definition
        /// Represents a tuple containing:
        /// Item1: The terminal block.
        /// Item2: The state of the conveyor.
        /// Item3: The volume of the cargo.
        /// </summary>
        List<MyTuple<IMyTerminalBlock, conveyorState, double>> cargoDefinition = new List<MyTuple<IMyTerminalBlock, conveyorState, double>>();

        /// <summary>
        /// Represents the inventory items
        /// Represents a tuple containing:
        /// Item1: The item type.
        /// Item2: Indicates if the item requires large conveyors.
        /// Item3: The amount of the item.
        /// </summary>
        List<MyTuple<MyItemType, bool, int>> myInventoryItems = new List<MyTuple<MyItemType, bool, int>>();

        public Program()
        {

            // Subscribe to the update frequency
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument != "")
            {
                Echo("This script needs no arguments.Don't give me any ideas.");
            }
            else
            {


                //Check if the primary connector was found
                if (primaryConnector == null)
                {
                    List<IMyShipConnector> myShipConnectors = new List<IMyShipConnector>();
                    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(myShipConnectors, c => c.CubeGrid == Me.CubeGrid && c.CustomName.Contains(primaryConnectorTag));

                    //If myShipConnectors isn't empty, set the primary connector to the first connector in the list
                    if (myShipConnectors.Count > 0) primaryConnector = myShipConnectors[0];

                    if (primaryConnector == null)
                    {
                        Echo("Primary Connector not found.");
                    }
                    else
                    {
                        Echo("Primary Connector found.");
                    }
                }
                else
                {
                    string output = "";

                    // Only consider blocks on the same grid as the programmable block with cargo, not production blocks and blocks connected to the primary connector if they don't have an exclusion tag in their name
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(cargoContainerList, c => c.CubeGrid == Me.CubeGrid && c.HasInventory && c.GetInventory(0).CanTransferItemTo(primaryConnector.GetInventory(0), itemSmall) && !(c is IMyProductionBlock) && !c.CustomName.Contains(excludedInventoryTag));

                    cargoDefinition.Clear();

                    // Check if the cargo container can transfer items and their size to the primary connector and add it to the cargoDefinition list
                    cargoContainerList.ForEach(c =>
                    {
                        if (c.GetInventory(0).CanTransferItemTo(primaryConnector.GetInventory(0), itemLarge))
                        {
                            cargoDefinition.Add(new MyTuple<IMyTerminalBlock, conveyorState, double>(c, conveyorState.large, (double)c.GetInventory(0).MaxVolume));
                        }
                        else if (c.GetInventory(0).CanTransferItemTo(primaryConnector.GetInventory(0), itemSmall))
                        {
                            cargoDefinition.Add(new MyTuple<IMyTerminalBlock, conveyorState, double>(c, conveyorState.small, (double)c.GetInventory(0).MaxVolume));
                        }
                    });

                    // Sum up the volume of the cargo containers based on item transferability
                    totalCargoVolumeLarge = cargoDefinition.Sum(c => c.Item2 == conveyorState.large ? (double)c.Item1.GetInventory(0).MaxVolume * 1000 : 0);
                    totalCargoVolumeSmall = cargoDefinition.Sum(c => c.Item2 == conveyorState.small ? (double)c.Item1.GetInventory(0).MaxVolume * 1000 : 0);

                    // Sum up the total available cargo volume
                    totalCargoVolume = totalCargoVolumeLarge + totalCargoVolumeSmall;

                    output += "Total Cargo Volume: " + totalCargoVolume + "l\n" +
                        $"Large cargo connected: {totalCargoVolumeLarge}l\n" +
                        $"Small cargo connected: {totalCargoVolumeSmall}l\n";

                    // Check if custom data is entered and if it's different from the last custom data
                    if (Me.CustomData != "" && Me.CustomData != customDataHold)
                    {
                        customDataHold = Me.CustomData;
                        var customDataArray = Me.CustomData.Trim().Split('\n');

                        myInventoryItems.Clear();

                        //Iterate through the custom data array
                        foreach (var item in customDataArray)
                        {
                            //Split the custom data array into item name and item amount
                            var itemArray = item.Split('=');
                            var itemName = itemArray[0].Split('/')[1];
                            var itemAmount = itemArray[1];
                            MyItemType itemType = MyItemType.MakeComponent(itemName);

                            //Check if the item fits through small or large conveyors - true if it needs large conveyors
                            bool isLarge = itemType.GetItemInfo().Size.Max() > 0.25f;

                            myInventoryItems.Add(new MyTuple<MyItemType, bool, int>(itemType, isLarge, int.Parse(itemAmount)));
                        }

                        //Sort the items by size
                        myInventoryItems = myInventoryItems.OrderBy(i => i.Item2).ToList();

                        //Calculate the total volume required for the items
                        totalVolumeRequired = myInventoryItems.Sum(i => i.Item1.GetItemInfo().Volume * i.Item3);
                        output += $"totalVolumeRequired: {totalVolumeRequired}";

                        //Check if the total volume required is smaller than the total cargo volume
                        if (totalVolumeRequired >= totalCargoVolume)
                        {
                            // Let the user know that there is not enough cargo space available
                            output += "Not enough cargo space available.";
                        }
                        else
                        {
                            // make a copy of the cargoDefinition list
                            var tempCargoDefinition = cargoDefinition.ToList();

                            //Sort tempCargoDefinition by size
                            tempCargoDefinition = tempCargoDefinition.OrderBy(c => c.Item2).ToList();

                            //Clear the custom data of the cargo containers
                            tempCargoDefinition.ForEach(c => c.Item1.CustomData = "@Special Container modes:\r\n\r\n- Normal: stores wanted amount, removes excess. Usage: item=100\r\n- Minimum: stores wanted amount, ignores excess. Usage: item=100M\r\n- Limiter: doesn't store items, only removes excess. Usage: item=100L\r\n- All: stores all items it can get until it's full. Usage: item=All\r\n\r\n");


                            // Echo the tempCargoDefinition for debugging
                            Echo(string.Join("\n", tempCargoDefinition.Select(c => c.Item1.CustomName + " - " + c.Item2 + " - " + c.Item3)));
                            Echo("myInventoryItems " + myInventoryItems.Count.ToString());
                            Echo("tempCargoDefinition " + tempCargoDefinition.Count.ToString());

                            //Iterate through the items in the myInventoryItems while there are still items in the list
                            while (myInventoryItems.Count > 0 && tempCargoDefinition.Count > 0)
                            {
                                //Get the first item in the myInventoryItems list and calculate how many items can be stored in the first cargo container from the tempCargoDefinition list
                                var item = myInventoryItems[0];

                                //Test if the item amount is 0
                                if (item.Item3 == 0)
                                {
                                    myInventoryItems.RemoveAt(0);
                                    continue;
                                }

                                var itemVolume = item.Item1.GetItemInfo().Volume;
                                var cargo = tempCargoDefinition[0];
                                var cargoRemainingVolume = cargo.Item3;
                                int itemsStored = (int)Math.Floor(cargoRemainingVolume / itemVolume);

                                //Take the min of the items stored and the amount of items in the list
                                itemsStored = Math.Min(itemsStored, item.Item3);

                                Echo("itemsStored " + itemsStored.ToString());

                                // If items can be stored in the cargo container
                                if (itemsStored > 0)
                                {
                                    //Reduce the remaining volume of the cargo container by the total volume of the items stored
                                    tempCargoDefinition[0] = new MyTuple<IMyTerminalBlock, conveyorState, double>(cargo.Item1, cargo.Item2, cargo.Item3 - itemVolume * itemsStored);

                                    //If the amount of items stored is equal to the amount of items in the list, remove the item from the list
                                    if (itemsStored == item.Item3)
                                    {
                                        //Remove the item from the list
                                        myInventoryItems.RemoveAt(0);
                                    }
                                    else
                                    {
                                        //Reduce the amount of items in the list by the amount of items stored
                                        myInventoryItems[0] = new MyTuple<MyItemType, bool, int>(item.Item1, item.Item2, item.Item3 - itemsStored);
                                    }

                                    //if the cargo container doesn't have " Special" in the custom name, add it
                                    if (!cargo.Item1.CustomName.Contains(" Special"))
                                    {
                                        cargo.Item1.CustomName += " Special";
                                    }

                                    cargo.Item1.CustomData += $"Component/{item.Item1.SubtypeId}={itemsStored}\n";

                                }
                                else
                                {
                                    //If no items can be stored in the cargo container, remove the cargo container from the list
                                    tempCargoDefinition.RemoveAt(0);
                                }
                            }
                        }


                    }
                    Echo(output);
                }
            }
        }

        // The end

    }

}
 