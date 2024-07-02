using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

[MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
public class BotSpawner : MySessionComponentBase
{
    private static HashSet<string> validBotIds = new HashSet<string>();
    private static BotSpawnerConfig config;
    private long totalUpdateTicks = 0;

    public override void LoadData()
    {
        base.LoadData();
        Setup();
        LoadConfig();
        LoadValidBotIds();
    }

    public override void UpdateAfterSimulation()
    {
        totalUpdateTicks++;
        DropItemsNearBlocks();
    }

    protected override void UnloadData()
    {
        base.UnloadData();
        Unload();
    }

    public static void Setup()
    {
        MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
    }

    private static void OnMessageEntered(string messageText, ref bool sendToOthers)
    {
        var player = MyAPIGateway.Session.Player;

        if (player == null || !MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
        {
            MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Only admins can use this command.");
            return;
        }

        if (messageText.StartsWith("/pepco spawn"))
        {
            var parameters = messageText.Split(' ');
            if (parameters.Length == 3)
            {
                string botId = parameters[2];
                if (validBotIds.Contains(botId))
                {
                    SpawnBot(botId);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Invalid bot ID: {botId}");
                }
            }
        }
        else if (messageText.Equals("/pepco listBots"))
        {
            ListValidBotIds();
        }
    }

    public static void SpawnBot(string botId)
    {
        Random random = new Random();
        double randomHeight = random.NextDouble() * (config.MaxHeight - config.MinHeight) + config.MinHeight;
        double randomRadius = random.NextDouble() * (config.MaxRadius - config.MinRadius) + config.MinRadius;
        double angle = random.NextDouble() * Math.PI * 2;

        Vector3D offset = new Vector3D(
            randomRadius * Math.Cos(angle),
            randomHeight,
            randomRadius * Math.Sin(angle)
        );

        Vector3D spawnPosition = MyAPIGateway.Session.Player.GetPosition() + offset;
        MyVisualScriptLogicProvider.SpawnBot(botId, spawnPosition);
        MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Spawned bot: {botId} at {spawnPosition}");
    }

    private static void LoadValidBotIds()
    {
        var botDefinitions = MyDefinitionManager.Static.GetBotDefinitions();
        foreach (var botDefinition in botDefinitions)
        {
            validBotIds.Add(botDefinition.Id.SubtypeName);
        }
    }

    private static void ListValidBotIds()
    {
        MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Valid bot IDs:");
        foreach (var botId in validBotIds)
        {
            MyAPIGateway.Utilities.ShowMessage("BotSpawner", botId);
        }
    }

    public static void Unload()
    {
        MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
    }

    private static void LoadConfig()
    {
        config = BotSpawnerConfig.Load();
        config.Save(); // Ensure a default config file is written
    }

    private void DropItemsNearBlocks()
    {
        long baseUpdateCycles = totalUpdateTicks / config.SpawnTriggerInterval;

        List<IMyPlayer> players = new List<IMyPlayer>();
        MyAPIGateway.Players.GetPlayers(players);

        foreach (var entity in MyEntities.GetEntities())
        {
            var grid = entity as IMyCubeGrid;
            if (grid != null)
            {
                var blocks = new List<IMySlimBlock>();
                grid.GetBlocks(blocks, b => b.FatBlock != null && IsValidBlock(b.FatBlock.BlockDefinition.TypeIdString, b.FatBlock.BlockDefinition.SubtypeId));

                foreach (var block in blocks)
                {
                    var blockSettings = GetDropSettingsForBlock(block.FatBlock.BlockDefinition.TypeIdString, block.FatBlock.BlockDefinition.SubtypeId);
                    if (blockSettings != null && blockSettings.Enabled && baseUpdateCycles % blockSettings.SpawnTriggerInterval == 0 && IsEnvironmentSuitable(grid, block))
                    {
                        if (!IsPlayerInRange(block, players, blockSettings.PlayerDistanceCheck))
                        {
                            continue;
                        }

                        float blockHealthPercentage = block.Integrity / block.MaxIntegrity;
                        if (blockHealthPercentage >= blockSettings.MinHealthPercentage && blockHealthPercentage <= blockSettings.MaxHealthPercentage)
                        {
                            if (CheckInventoryForRequiredItems(block, blockSettings))
                            {
                                int dropAmount = DropItems(block, blockSettings);
                                if (dropAmount > 0)
                                {
                                    RemoveItemsFromInventory(block, blockSettings, dropAmount);
                                    block.DoDamage(blockSettings.DamageAmount * dropAmount, MyDamageType.Grind, true);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static bool IsValidBlock(string typeIdString, string subtypeId)
    {
        // Implement your validation logic for blocks here
        return true;
    }

    private static bool IsEnvironmentSuitable(IMyCubeGrid grid, IMySlimBlock block)
    {
        // Implement your logic to check if the environment is suitable for spawning
        return true;
    }

    private static bool IsPlayerInRange(IMySlimBlock block, List<IMyPlayer> players, int distance)
    {
        foreach (var player in players)
        {
            if (Vector3D.Distance(player.GetPosition(), block.FatBlock.GetPosition()) <= distance)
            {
                return true;
            }
        }
        return false;
    }

    private static bool CheckInventoryForRequiredItems(IMySlimBlock block, BotSpawnerConfig blockSettings)
    {
        // Implement your logic to check the inventory for required items
        return true;
    }

    private static int DropItems(IMySlimBlock block, BotSpawnerConfig blockSettings)
    {
        // Implement your logic to drop items from the block
        return 0;
    }

    private static void RemoveItemsFromInventory(IMySlimBlock block, BotSpawnerConfig blockSettings, int dropAmount)
    {
        // Implement your logic to remove items from the block's inventory
    }

    private static BotSpawnerConfig GetDropSettingsForBlock(string typeIdString, string subtypeId)
    {
        // Implement your logic to get the drop settings for the block
        return config; // Use config for now as a placeholder
    }
}

public class BotSpawnerConfig
{
    private const string ConfigFileName = "BotSpawnerConfig.xml";
    public string BlockId { get; set; }
    public string BlockType { get; set; }
    public List<string> EntityID { get; set; } = new List<string>();
    public int MinAmount { get; set; } = 1;
    public int MaxAmount { get; set; } = 1;
    public bool UseWeightedDrops { get; set; } = false;
    public float DamageAmount { get; set; } = 0;
    public float MinHealthPercentage { get; set; } = 0.1f;
    public float MaxHealthPercentage { get; set; } = 1.0f;
    public double MinHeight { get; set; } = 1.0;
    public double MaxHeight { get; set; } = 2.0;
    public double MinRadius { get; set; } = 0.5;
    public double MaxRadius { get; set; } = 2;
    public int SpawnTriggerInterval { get; set; } = 10;
    public bool EnableAirtightAndOxygen { get; set; } = false;
    public bool Enabled { get; set; } = true;
    public bool StackItems { get; set; } = false;
    public bool SpawnInsideInventory { get; set; } = false;
    public List<string> RequiredItemTypes { get; set; } = new List<string>();
    public List<string> RequiredItemIds { get; set; } = new List<string>();
    public List<int> RequiredItemAmounts { get; set; } = new List<int>();
    public int PlayerDistanceCheck { get; set; } = 1000;

    public static BotSpawnerConfig Load()
    {
        if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ConfigFileName, typeof(BotSpawnerConfig)))
        {
            try
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ConfigFileName, typeof(BotSpawnerConfig)))
                {
                    string xmlText = reader.ReadToEnd();
                    return MyAPIGateway.Utilities.SerializeFromXML<BotSpawnerConfig>(xmlText);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to load config: {e.Message}");
            }
        }

        // Return default config if file not found or failed to load
        return new BotSpawnerConfig();
    }

    public void Save()
    {
        try
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConfigFileName, typeof(BotSpawnerConfig)))
            {
                string xmlText = MyAPIGateway.Utilities.SerializeToXML(this);
                writer.Write(xmlText);
            }
        }
        catch (Exception e)
        {
            MyLog.Default.WriteLine($"Failed to save config: {e.Message}");
        }
    }
}
