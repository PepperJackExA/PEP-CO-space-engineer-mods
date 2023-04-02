using System.Collections.Generic;
using static ModAdjuster.DefinitionStructure;
using static ModAdjuster.DefinitionStructure.BlockDef;
using static ModAdjuster.DefinitionStructure.BlockDef.BlockAction.BlockMod;

namespace ModAdjuster
{
    public class BlockDefinitions
    {
        public string AdminComponent = "MyObjectBuilder_Component/admin_Fluxkondensator_Pepco";
        public List<string> DisabledBlocks = new List<string>()
        {
        };

        public List<BlockDef> Definitions = new List<BlockDef>()
        {
            // Beacons
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_Beacon/LargeBlockBeacon",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Beacon Range = 50K",
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Large Beacon",
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Beacon Range = 50K",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 2
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 7
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 5,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Lightbulb",
                        Index = 4,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 14
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 4
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 50000
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_Beacon/SmallBlockBeacon",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Small Beacon"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Beacon Range = 25K",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 1
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 7,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 6,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 5,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Lightbulb",
                        Index = 4,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 4
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 25000
                    },
                }
            },

              // Normal Antenna
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/LargeBlockRadioAntennaDish",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 50K",
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Large Antenna Dish"
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 50
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 8,
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 8,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 7,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 6,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 5,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 4,
                        Count = 500
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 3,
                        Count = 250
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 2,
                        Count = 30
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 7
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 50000
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/LargeBlockRadioAntenna",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Large Antenna Tower"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 10K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 10000
                    },



                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/OmnidirectionalAntenna",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Omni Antenna Tower"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 10K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 15
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 15
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 15
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 75
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 38
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 15
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 7500
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/AntennaCube",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 5K",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 25
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 5
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 5000
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/Antenna45Corner",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 5K",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 25
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 5
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 5000
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/AntennaSlope",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 5K",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 25
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 5
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 5000
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/AntennaCorner",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 5K",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 25
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 5
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 5000
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/LBShortRadioAntenna",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 2K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 2
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 2000
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/SBAngledRadioAntenna",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 2K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 2
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 2000
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/SBLongRadioAntenna",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 2K",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 5,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 2
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 2000
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/SmallBlockRadioAntenna",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 500m",
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 6,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 5,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 5
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 1
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 500
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_RadioAntenna/OmnidirectionalAntennaSmall",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 200m",
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 4,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 3
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 1
                    },
                    new BlockAction
                    {
                        Action = ChangeBroadcastRadius,
                        Value = 200
                    },
                }
            },
        
            // Laser Antenna 
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_LaserAntenna/LargeBlockLaserAntenna",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Fast Laser Antenna Gimble"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 25K",
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 11,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 10,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 9,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 8,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 7,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Motor",
                        Index = 6,
                        Count = 16
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 5,
                        Count = 35
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 4,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 3,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 2,
                        Count = 6
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/InteriorPlate",
                        Index = 0,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 8
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 200
                    },
                    new BlockAction
                    {
                        Action = ChangeLaserMaxRange,
                        Value = 25000
                    },

                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_LaserAntenna/SmallBlockLaserAntenna",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Fast Laser Antenna Gimble"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 15K With Fast turning speeds",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 50
                    },
                    new BlockAction
                    {
                        Action = ChangeLaserMaxRange,
                        Value = 15000
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 11,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 10,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 9,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 8,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 7,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 6,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Motor",
                        Index = 5,
                        Count = 6
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 6
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/InteriorPlate",
                        Index = 0,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 7
                    },

                }
            },
                        new BlockDef()
            {
                BlockName = "MyObjectBuilder_LaserAntenna/Phoenix_LargeBlockRadioAntennaLarge",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Deep Space Laser Antenna Dish"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = Infinit",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 200
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 9,
                        Count = 200
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/AdvancedComputer",
                        Index = 9,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 8,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Ceramic",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Motor",
                        Index = 6,
                        Count = 200
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 5,
                        Count = 325
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 4,
                        Count = 200
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 3,
                        Count = 400
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 2,
                        Count = 350
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 500
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/InteriorPlate",
                        Index = 0,
                        Count = 5000
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 9
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_LaserAntenna/Phoenix_LargeBlockRadioAntennaMed",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Planetary Laser Antenna Dish"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 10M",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 200
                    },
                    new BlockAction
                    {
                        Action = ChangeLaserMaxRange,
                        Value = 10000000
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 9,
                        Count = 200
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/AdvancedComputer",
                        Index = 9,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 8,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Ceramic",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Motor",
                        Index = 6,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 5,
                        Count = 205
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 4,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 3,
                        Count = 250
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 2,
                        Count = 250
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 300
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/InteriorPlate",
                        Index = 0,
                        Count = 3000
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 9
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_LaserAntenna/Phoenix_LargeBlockRadioAntenna",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Laser Antenna Dish"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 10K",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 200
                    },
                    new BlockAction
                    {
                        Action = ChangeLaserMaxRange,
                        Value = 10000
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 11,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 10,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 9,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 8,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Motor",
                        Index = 6,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 5,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 4,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 3,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 2,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Steelplate",
                        Index = 0,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 7
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_LaserAntenna/Phoenix_SmallBlockRadioAntenna",
                BlockActions = new[]
                {
                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Laser Antenna Dish"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 5K",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 200
                    },
                    new BlockAction
                    {
                        Action = ChangeLaserMaxRange,
                        Value = 5000
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 11,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 10,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 9,
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 8,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Computer",
                        Index = 6,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Motor",
                        Index = 5,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 0,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 6
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_LaserAntenna/DB_LaserCommLarge",
                BlockActions = new[]
                {
                    // Large Advanced Laser Antenna Dish
                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Advanced Laser Antenna Dish"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 300K Rotates fast",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 200
                    },
                    new BlockAction
                    {
                        Action = ChangeLaserMaxRange,
                        Value = 300000
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 8,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/AdvancedComputer",
                        Index = 8,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 8,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/Superconductor",
                        Index = 8,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Ceramic",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Motor",
                        Index = 6,
                        Count = 24
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 5,
                        Count = 50
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 4,
                        Count = 40
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 3,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 2,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/InteriorPlate",
                        Index = 0,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 10
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_LaserAntenna/DB_LaserCommSmall",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Advanced Laser Antenna Dish"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 100K Rotates fast",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 200
                    },
                    new BlockAction
                    {
                        Action = ChangeLaserMaxRange,
                        Value = 100000
                    },
                    new BlockAction
                    {
                        Action = RemoveComponent,
                        Index = 11,
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 8,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/AdvancedComputer",
                        Index = 8,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 8,
                        Count = 2
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Ceramic",
                        Index = 7,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Superconductor",
                        Index = 6,
                        Count = 1
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Motor",
                        Index = 5,
                        Count = 6
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 4,
                        Count = 6
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 3,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 2,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 1,
                        Count = 8
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/InteriorPlate",
                        Index = 0,
                        Count = 4
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 9
                    },
                }
            },
            new BlockDef()
            {
                BlockName = "MyObjectBuilder_LaserAntenna/DB_HumongousAntenna",
                BlockActions = new[]
                {

                    new BlockAction
                    {
                        Action = ChangeBlockName,
                        NewText = "Advanced Deep Space Laser Antenna Dish"
                    },
                    new BlockAction
                    {
                        Action = ChangeBlockDescription,
                        NewText = "Antenna Range = 100M",
                    },
                    new BlockAction
                    {
                        Action = ChangePCU,
                        Value = 200
                    },
                    new BlockAction
                    {
                        Action = ChangeLaserMaxRange,
                        Value = 100000000
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SteelPlate",
                        Index = 7,
                        Count = 200
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/QuantumComputer",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/RadioCommunication",
                        Index = 7,
                        Count = 100
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/Superconductor",
                        Index = 7,
                        Count = 20
                    },
                    new BlockAction
                    {
                        Action = InsertComponent,
                        Component = "MyObjectBuilder_Component/Ceramic",
                        Index = 7,
                        Count = 10
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Motor",
                        Index = 6,
                        Count = 200
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Electromagnet",
                        Index = 5,
                        Count = 445
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/CopperWire",
                        Index = 4,
                        Count = 250
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/Construction",
                        Index = 3,
                        Count = 250
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/SmallTube",
                        Index = 2,
                        Count = 250
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/LargeTube",
                        Index = 1,
                        Count = 200
                    },
                    new BlockAction
                    {
                        Action = ReplaceComponent,
                        Component = "MyObjectBuilder_Component/InteriorPlate",
                        Index = 0,
                        Count = 2000
                    },
                    new BlockAction
                    {
                        Action = ChangeCriticalComponentIndex,
                        Index = 10
                    },
                }
            },
        };

    }
}