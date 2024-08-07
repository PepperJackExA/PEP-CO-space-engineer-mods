using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Utils;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.Entities;
using VRage.ObjectBuilders;
using VRage.Game.ObjectBuilders.Definitions;
using System.Collections.Generic;
using System;
using System.Linq;
using Sandbox.Game.Entities.Blocks;
using VRage.Game.Components;
using VRage.Game;

namespace PEPCO
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class StoreBlockModifier : MySessionComponentBase
    {
        private const string ServerConfigFileName = "PEPCOServerSettings.ini";
        private static MyIni serverIni = new MyIni();
        private bool isInitialized = false;
        private Dictionary<long, List<MyStoreItem>> storeItemsSnapshot = new Dictionary<long, List<MyStoreItem>>();

        public override void BeforeStart()
        {
            if (!isInitialized)
            {
                // Find all store blocks
                List<IMyStoreBlock> storeBlocks = FindAllStoreBlocks();

                if (storeBlocks.Count > 0)
                {
                    // Write store block details to the server config
                    WriteStoreBlocksToServerConfig(storeBlocks);

                    // Example: Clear and add offers/orders to the first store block
                    ClearStoreOffers(storeBlocks[0]);
                    AddStoreOffers(storeBlocks[0]);

                    // Take initial snapshot of store items
                    TakeStoreItemsSnapshot(storeBlocks);

                    // Register for updates to check for transactions
                    MyAPIGateway.Utilities.InvokeOnGameThread(Update);

                    isInitialized = true;
                }
            }
        }

        private void Update()
        {
            List<IMyStoreBlock> storeBlocks = FindAllStoreBlocks();
            foreach (var storeBlock in storeBlocks)
            {
                CheckForTransactions(storeBlock);
            }
            MyAPIGateway.Utilities.InvokeOnGameThread(Update);
        }

        private List<IMyStoreBlock> FindAllStoreBlocks()
        {
            List<IMyStoreBlock> storeBlocks = new List<IMyStoreBlock>();

            // Specify the namespace explicitly to avoid ambiguity
            var grid = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.GetTopMostParent() as VRage.Game.ModAPI.IMyCubeGrid;

            if (grid != null)
            {
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid).GetBlocksOfType(storeBlocks);

                if (storeBlocks.Count == 0)
                {
                    MyLog.Default.WriteLineAndConsole("No store blocks found.");
                }
            }
            else
            {
                MyLog.Default.WriteLineAndConsole("Grid is null.");
            }

            return storeBlocks;
        }

        private void ClearStoreOffers(IMyStoreBlock storeBlock)
        {
            List<MyStoreItem> itemsToRemove = new List<MyStoreItem>();
            storeBlock.GetPlayerStoreItems(itemsToRemove, itemType: MyStoreItemTypes.Offer);

            foreach (var item in itemsToRemove)
            {
                storeBlock.CancelStoreItem(item.Id);
            }
        }

        private void AddStoreOffers(IMyStoreBlock storeBlock)
        {
            // Example sell offer
            var sellOffer = new MyObjectBuilder_StoreItem
            {
                Amount = 100,
                PricePerUnit = 50,
                StoreItemType = StoreItemTypes.Offer,
                Item = new MyObjectBuilder_InventoryItem { Amount = 100, Content = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>("SteelPlate") }
            };
            storeBlock.CreateStoreItem(sellOffer, ownerId: 0);

            // Example buy order
            var buyOrder = new MyObjectBuilder_StoreItem
            {
                Amount = 500,
                PricePerUnit = 10,
                StoreItemType = StoreItemTypes.Order,
                Item = new MyObjectBuilder_InventoryItem { Amount = 500, Content = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>("Iron") }
            };
            storeBlock.CreateStoreItem(buyOrder, ownerId: 0);
        }

        private void WriteStoreBlocksToServerConfig(List<IMyStoreBlock> storeBlocks)
        {
            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(ServerConfigFileName, typeof(StoreBlockModifier)))
                {
                    using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ServerConfigFileName, typeof(StoreBlockModifier)))
                    {
                        writer.Write(""); // Create empty file if it does not exist
                    }
                }

                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ServerConfigFileName, typeof(StoreBlockModifier)))
                {
                    var text = reader.ReadToEnd();
                    serverIni.TryParse(text);
                }

                // Clear existing store block sections
                var sections = new List<string>();
                serverIni.GetSections(sections);
                foreach (var section in sections)
                {
                    if (section.StartsWith("StoreBlock"))
                    {
                        serverIni.DeleteSection(section);
                    }
                }

                // Write new store block details
                for (int i = 0; i < storeBlocks.Count; i++)
                {
                    var storeBlock = storeBlocks[i];
                    var sectionName = $"StoreBlock_{i}";

                    serverIni.Set(sectionName, "EntityId", storeBlock.EntityId.ToString());
                    serverIni.Set(sectionName, "DisplayName", storeBlock.DisplayNameText);
                    serverIni.Set(sectionName, "Position", storeBlock.GetPosition().ToString());

                    // Write sell offers
                    List<MyStoreItem> sellOffers = new List<MyStoreItem>();
                    storeBlock.GetPlayerStoreItems(sellOffers, itemType: MyStoreItemTypes.Offer);

                    int j = 0;
                    foreach (var offer in sellOffers)
                    {
                        var offerSection = $"{sectionName}_Sell_{j}";
                        serverIni.Set(offerSection, "ItemId", offer.Item.Content.GetId().ToString());
                        serverIni.Set(offerSection, "Amount", offer.Amount.ToString());
                        serverIni.Set(offerSection, "PricePerUnit", offer.PricePerUnit.ToString());
                        j++;
                    }

                    // Write buy orders
                    List<MyStoreItem> buyOrders = new List<MyStoreItem>();
                    storeBlock.GetPlayerStoreItems(buyOrders, itemType: MyStoreItemTypes.Order);

                    int k = 0;
                    foreach (var order in buyOrders)
                    {
                        var orderSection = $"{sectionName}_Buy_{k}";
                        serverIni.Set(orderSection, "ItemId", order.Item.Content.GetId().ToString());
                        serverIni.Set(orderSection, "Amount", order.Amount.ToString());
                        serverIni.Set(orderSection, "PricePerUnit", order.PricePerUnit.ToString());
                        k++;
                    }
                }

                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ServerConfigFileName, typeof(StoreBlockModifier)))
                {
                    writer.Write(serverIni.ToString());
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in WriteStoreBlocksToServerConfig: {e.Message}");
            }
        }

        private void ReadStoreBlocksFromServerConfig()
        {
            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(ServerConfigFileName, typeof(StoreBlockModifier)))
                {
                    MyLog.Default.WriteLineAndConsole("Server config file not found.");
                    return;
                }

                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ServerConfigFileName, typeof(StoreBlockModifier)))
                {
                    var text = reader.ReadToEnd();
                    serverIni.TryParse(text);
                }

                var sections = new List<string>();
                serverIni.GetSections(sections);
                foreach (var section in sections)
                {
                    if (section.StartsWith("StoreBlock"))
                    {
                        string entityIdStr = serverIni.Get(section, "EntityId").ToString();
                        long entityId = long.Parse(entityIdStr);

                        IMyStoreBlock storeBlock = MyAPIGateway.Entities.GetEntityById(entityId) as IMyStoreBlock;
                        if (storeBlock != null)
                        {
                            // Clear existing offers and orders
                            List<MyStoreItem> itemsToRemove = new List<MyStoreItem>();
                            storeBlock.GetPlayerStoreItems(itemsToRemove);

                            foreach (var item in itemsToRemove)
                            {
                                storeBlock.CancelStoreItem(item.Id);
                            }

                            // Read sell offers
                            int j = 0;
                            while (true)
                            {
                                var offerSection = $"{section}_Sell_{j}";
                                if (!serverIni.ContainsSection(offerSection)) break;

                                var itemIdStr = serverIni.Get(offerSection, "ItemId").ToString();
                                var amountStr = serverIni.Get(offerSection, "Amount").ToString();
                                var pricePerUnitStr = serverIni.Get(offerSection, "PricePerUnit").ToString();

                                var itemId = MyDefinitionId.Parse(itemIdStr);
                                var amount = int.Parse(amountStr);
                                var pricePerUnit = int.Parse(pricePerUnitStr);

                                var sellOffer = new MyObjectBuilder_StoreItem
                                {
                                    Amount = amount,
                                    PricePerUnit = pricePerUnit,
                                    StoreItemType = StoreItemTypes.Offer,
                                    Item = new MyObjectBuilder_InventoryItem { Amount = amount, Content = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>(itemId.SubtypeName) }
                                };
                                storeBlock.CreateStoreItem(sellOffer, ownerId: 0);

                                j++;
                            }

                            // Read buy orders
                            int k = 0;
                            while (true)
                            {
                                var orderSection = $"{section}_Buy_{k}";
                                if (!serverIni.ContainsSection(orderSection)) break;

                                var itemIdStr = serverIni.Get(orderSection, "ItemId").ToString();
                                var amountStr = serverIni.Get(orderSection, "Amount").ToString();
                                var pricePerUnitStr = serverIni.Get(orderSection, "PricePerUnit").ToString();

                                var itemId = MyDefinitionId.Parse(itemIdStr);
                                var amount = int.Parse(amountStr);
                                var pricePerUnit = int.Parse(pricePerUnitStr);

                                var buyOrder = new MyObjectBuilder_StoreItem
                                {
                                    Amount = amount,
                                    PricePerUnit = pricePerUnit,
                                    StoreItemType = StoreItemTypes.Order,
                                    Item = new MyObjectBuilder_InventoryItem { Amount = amount, Content = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(itemId.SubtypeName) }
                                };
                                storeBlock.CreateStoreItem(buyOrder, ownerId: 0);

                                k++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in ReadStoreBlocksFromServerConfig: {e.Message}");
            }
        }

        private void TakeStoreItemsSnapshot(List<IMyStoreBlock> storeBlocks)
        {
            foreach (var storeBlock in storeBlocks)
            {
                List<MyStoreItem> items = new List<MyStoreItem>();
                storeBlock.GetPlayerStoreItems(items);
                storeItemsSnapshot[storeBlock.EntityId] = items;
            }
        }

        private void CheckForTransactions(IMyStoreBlock storeBlock)
        {
            List<MyStoreItem> currentItems = new List<MyStoreItem>();
            storeBlock.GetPlayerStoreItems(currentItems);

            List<MyStoreItem> previousItems;
            if (storeItemsSnapshot.TryGetValue(storeBlock.EntityId, out previousItems))
            {
                foreach (var prevItem in previousItems)
                {
                    var currItem = currentItems.FirstOrDefault(item => item.Id == prevItem.Id);

                    if (currItem != null && currItem.Amount != prevItem.Amount)
                    {
                        var amountChanged = currItem.Amount - prevItem.Amount;
                        var isPurchase = amountChanged < 0;

                        AdjustPriceAndStock(storeBlock, currItem.Id, isPurchase, Math.Abs(amountChanged));
                    }
                }
            }

            storeItemsSnapshot[storeBlock.EntityId] = currentItems;
        }

        private void AdjustPriceAndStock(IMyStoreBlock storeBlock, long itemId, bool isPurchase, int amount)
        {
            List<MyStoreItem> items = new List<MyStoreItem>();
            storeBlock.GetPlayerStoreItems(items);

            // Adjust sell offer prices and stock
            foreach (var item in items)
            {
                if (item.Id == itemId && item.StoreItemType == StoreItemTypes.Offer)
                {
                    if (isPurchase)
                    {
                        item.PricePerUnit += 1; // Increase price on purchase
                        item.Amount -= amount; // Decrease stock on purchase
                    }
                    else
                    {
                        item.PricePerUnit -= 1; // Decrease price on sale
                        item.Amount += amount; // Increase stock on sale
                    }
                    item.PricePerUnit = Math.Max(1, item.PricePerUnit); // Ensure price is at least 1
                    item.Amount = Math.Max(0, item.Amount); // Ensure stock is not negative
                    UpdateStoreConfig(storeBlock, item);
                    break;
                }
            }

            // Adjust buy order prices and stock
            foreach (var item in items)
            {
                if (item.Id == itemId && item.StoreItemType == StoreItemTypes.Order)
                {
                    if (isPurchase)
                    {
                        item.PricePerUnit -= 1; // Decrease price on purchase
                        item.Amount += amount; // Increase stock on purchase
                    }
                    else
                    {
                        item.PricePerUnit += 1; // Increase price on sale
                        item.Amount -= amount; // Decrease stock on sale
                    }
                    item.PricePerUnit = Math.Max(1, item.PricePerUnit); // Ensure price is at least 1
                    item.Amount = Math.Max(0, item.Amount); // Ensure stock is not negative
                    UpdateStoreConfig(storeBlock, item);
                    break;
                }
            }
        }

        private void UpdateStoreConfig(IMyStoreBlock storeBlock, MyStoreItem item)
        {
            try
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ServerConfigFileName, typeof(StoreBlockModifier)))
                {
                    var text = reader.ReadToEnd();
                    serverIni.TryParse(text);
                }

                var sections = new List<string>();
                serverIni.GetSections(sections);

                foreach (var section in sections)
                {
                    if (section.Contains(storeBlock.EntityId.ToString()))
                    {
                        string prefix = item.StoreItemType == StoreItemTypes.Offer ? "Sell" : "Buy";

                        for (int i = 0; ; i++)
                        {
                            var itemSection = $"{section}_{prefix}_{i}";
                            if (!serverIni.ContainsSection(itemSection)) break;

                            var itemIdStr = serverIni.Get(itemSection, "ItemId").ToString();
                            if (itemIdStr == item.Item.Content.GetId().ToString())
                            {
                                serverIni.Set(itemSection, "PricePerUnit", item.PricePerUnit.ToString());
                                serverIni.Set(itemSection, "Amount", item.Amount.ToString());
                                break;
                            }
                        }
                    }
                }

                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ServerConfigFileName, typeof(StoreBlockModifier)))
                {
                    writer.Write(serverIni.ToString());
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in UpdateStoreConfig: {e.Message}");
            }
        }

        protected override void UnloadData()
        {
            // Clean up if needed
        }
    }
}
