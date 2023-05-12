using Sandbox.Definitions;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using VRage.Network;
using SpaceEngineers.Game;
using static ModAdjuster.DefinitionStructure.BlockDef.BlockAction;
using static ModAdjuster.DefinitionStructure.GasDef.GasAction;

namespace ModAdjuster
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, int.MaxValue - 1)]
    public class AdjustSBC : MySessionComponentBase
    {
        internal BlockDefinitions BlockDefs = new BlockDefinitions();

        internal GasDefinitions GasDefs = new GasDefinitions();

        internal MyGasProperties GasDef = new MyGasProperties();

        internal MyCubeBlockDefinition BlockDef = new MyCubeBlockDefinition();

        internal MyDefinitionId ScrapId = new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Scrap");

        internal Dictionary<MyCubeBlockDefinition, float> Resists = new Dictionary<MyCubeBlockDefinition, float>();

        public override void LoadData()
        {
            MyLog.Default.WriteLine($"[ModAdjuster] Starting changes!");
            AdjustBlocks();
            MyLog.Default.WriteLine($"[ModAdjuster] Changes complete!");
        }

        public override void BeforeStart()
        {
            BeforeStartStuff();

            Clean();
        }

        internal void Clean()
        {
            BlockDefs.Definitions.Clear();
            BlockDefs = null;
            BlockDef = null;

            GasDefs.Definitions.Clear();
            GasDefs = null;
            GasDef = null;

            Resists.Clear();
        }
        internal void AdjustGas()
        {
            foreach (var Gas in GasDefs.Definitions)
            {
                var defId = MyDefinitionId.Parse(Gas.GasName);
                if (MyDefinitionManager.Static.TryGetDefinition(defId, out GasDef))
                {
                    foreach (var action in Gas.GasActions)
                    {
                        switch (action.Action)
                        {
                            case GasMod.ChangeDisplayName:
                                GasDef.DisplayNameEnum = null;
                                GasDef.DisplayNameString = action.NewText;
                                break;

                            case GasMod.ChangeEnergyDensity:
                                (GasDef as MyGasProperties).EnergyDensity = action.Value;
                                break;

                        }
                    }

                }

                else MyLog.Default.WriteLine($"[ModAdjuster] Block {Gas.GasName} not found!");
            }
        }
        internal void AdjustBlocks()
        {
            foreach (var block in BlockDefs.Definitions)
            {
                var defId = MyDefinitionId.Parse(block.BlockName);
                if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out BlockDef))
                {
                    foreach (var action in block.BlockActions)
                    {
                        MyDefinitionId id;
                        MyComponentDefinition comp = null;
                        MyPhysicalItemDefinition item = null;

                        if (action.Component != null)
                        {
                            if (!MyDefinitionId.TryParse(action.Component, out id))
                            {
                                MyLog.Default.WriteLine($"[ModAdjuster] No valid TypeId in {action.Component}");
                                continue;
                            }

                            if (id.TypeId.ToString() == "MyObjectBuilder_Component")
                                comp = MyDefinitionManager.Static.GetComponentDefinition(id);
                            else
                                item = MyDefinitionManager.Static.GetPhysicalItemDefinition(id);

                            if (comp == null && item == null)
                            {
                                MyLog.Default.WriteLine($"[ModAdjuster] Failed to find definition for {action.Component}");
                                continue;
                            }

                        }

                        var maxIndex = action.Action == BlockMod.InsertComponent ? BlockDef.Components.Length : BlockDef.Components.Length - 1;
                        if (action.Index > maxIndex || action.Index < 0)
                        {
                            MyLog.Default.WriteLine($"[ModAdjuster] Index out of range: {block.BlockName}, Index = {action.Index}");
                            continue;
                        }

                        switch (action.Action)
                        {
                            case BlockMod.DisableBlockDefinition:
                                BlockDef.Enabled = false;
                                break;

                            case BlockMod.ChangeBlockPublicity:
                                BlockDef.Public = !BlockDef.Public;
                                break;

                            case BlockMod.ChangeBlockGuiVisibility:
                                BlockDef.GuiVisible = !BlockDef.GuiVisible;
                                break;

                            case BlockMod.ChangeBlockName:
                                BlockDef.DisplayNameEnum = null;
                                BlockDef.DisplayNameString = action.NewText;
                                break;

                            case BlockMod.InsertComponent:
                                InsertComponent(action.Index, comp, action.Count);
                                break;

                            case BlockMod.ReplaceComponent:
                                ReplaceComponent(action.Index, comp, action.Count);
                                break;

                            case BlockMod.RemoveComponent:
                                RemoveComponent(action.Index);
                                break;

                            case BlockMod.ChangeCriticalComponentIndex:
                                BlockDef.CriticalGroup = (ushort)action.Index;
                                SetRatios(action.Index);
                                break;

                            case BlockMod.ChangeComponentDeconstructId:
                                BlockDef.Components[action.Index].DeconstructItem = item ?? comp;
                                break;

                            case BlockMod.ChangeBlockDescription:
                                BlockDef.DescriptionEnum = null;
                                BlockDef.DescriptionString = action.NewText;
                                break;

                            case BlockMod.ChangeIcon:
                                BlockDef.Icons = new string[] { action.NewText };
                                break;

                            case BlockMod.ChangePCU:
                                BlockDef.PCU = (int)action.Value;
                                break;

                            case BlockMod.ChangeDeformationRatio:
                                BlockDef.DeformationRatio = action.Value;
                                break;

                            case BlockMod.ChangeResistance:
                                Resists[BlockDef] = action.Value;
                                break;

                            case BlockMod.ChangeBuildTime:
                                BlockDef.IntegrityPointsPerSec = BlockDef.MaxIntegrity / action.Value;
                                break;





                            case BlockMod.ChangeFuelMultiplier:
                                (BlockDef as MyHydrogenEngineDefinition).FuelProductionToCapacityMultiplier = action.Value;
                                break;
                            case BlockMod.ChangeFuelCapacity:
                                (BlockDef as MyHydrogenEngineDefinition).FuelCapacity = action.Value;
                                break;




                            case BlockMod.ChangeMaxPowerOutput:
                                (BlockDef as MyPowerProducerDefinition).MaxPowerOutput = action.Value;
                                break;
                            case BlockMod.ChangeOptimalWindSpeed:
                                (BlockDef as MyWindTurbineDefinition).OptimalWindSpeed = action.Value;
                                break;




                            case BlockMod.ChangeBatteryPowerStorage:
                                (BlockDef as MyBatteryBlockDefinition).MaxStoredPower = action.Value;
                                break;
                            case BlockMod.ChangeBatteryMaxoutput:
                                (BlockDef as MyBatteryBlockDefinition).MaxPowerOutput = action.Value;
                                break;
                            case BlockMod.ChangeBatteryMaxRequiredInput:
                                (BlockDef as MyBatteryBlockDefinition).RequiredPowerInput = action.Value;
                                break;




                            case BlockMod.ChangeThrustForce:
                                (BlockDef as MyThrustDefinition).ForceMagnitude = action.Value;
                                break;

                            case BlockMod.ChangeThrustPowerConsumption:
                                (BlockDef as MyThrustDefinition).MaxPowerConsumption = action.Value;
                                break;
                            case BlockMod.ChangeThrustFuelId:
                                MyDefinitionId fuelId;
                                if (MyDefinitionId.TryParse(action.Component, out fuelId))
                                    (BlockDef as MyThrustDefinition).FuelConverter.FuelId = (SerializableDefinitionId)fuelId;
                                break;
                            case BlockMod.ChangeThrustFuelEfficiency:
                                (BlockDef as MyThrustDefinition).FuelConverter.Efficiency = action.Value;
                                break;



                        }
                    }
                }
                else MyLog.Default.WriteLine($"[ModAdjuster] Block {block.BlockName} not found!");
            }

        }

        internal void InsertComponent(int index, MyComponentDefinition comp, int count)
        {
            float intDiff = comp.MaxIntegrity * count;
            float massDiff = comp.Mass * count;

            if (index <= BlockDef.CriticalGroup)
            {
                BlockDef.CriticalGroup += 1;
            }

            BlockDef.MaxIntegrity += intDiff;
            BlockDef.Mass += massDiff;

            var newComps = new MyCubeBlockDefinition.Component[BlockDef.Components.Length + 1];

            int i;
            for (i = 0; i < newComps.Length; i++)
            {
                if (i < index)
                    newComps[i] = BlockDef.Components[i];
                else if (i == index)
                    newComps[i] = new MyCubeBlockDefinition.Component();
                else
                    newComps[i] = BlockDef.Components[i - 1];
            }
            newComps[index].Definition = comp;
            newComps[index].DeconstructItem = comp;
            newComps[index].Count = count;

            BlockDef.Components = newComps;

            SetRatios(BlockDef.CriticalGroup);
        }

        internal void ReplaceComponent(int index, MyComponentDefinition newComp, int newCount)
        {
            var comp = BlockDef.Components[index];
            int oldCount = comp.Count;
            float intDiff;
            float massDiff;
            if (newCount > 0)
            {
                intDiff = newComp.MaxIntegrity * newCount - comp.Definition.MaxIntegrity * oldCount;
                massDiff = newComp.Mass * newCount - comp.Definition.Mass * oldCount;

                BlockDef.Components[index].Count = newCount;
            }
            else
            {
                intDiff = (newComp.MaxIntegrity - comp.Definition.MaxIntegrity) * oldCount;
                massDiff = (newComp.Mass - comp.Definition.Mass) * oldCount;
            }

            comp.Definition = newComp;
            comp.DeconstructItem = newComp;

            BlockDef.MaxIntegrity += intDiff;
            BlockDef.Mass += massDiff;

            SetRatios(BlockDef.CriticalGroup);
        }

        internal void RemoveComponent(int index)
        {
            var comp = BlockDef.Components[index];
            var def = comp.Definition;
            var count = comp.Count;
            float intDiff = def.MaxIntegrity * count;
            float massDiff = def.Mass * count;

            if (index <= BlockDef.CriticalGroup)
            {
                BlockDef.CriticalGroup -= 1;
            }

            BlockDef.MaxIntegrity -= intDiff;
            BlockDef.Mass -= massDiff;

            var newComps = new MyCubeBlockDefinition.Component[BlockDef.Components.Length - 1];

            int i;
            for (i = 0; i < newComps.Length; i++)
            {
                var j = i < index ? i : i + 1;
                newComps[i] = BlockDef.Components[j];
            }

            BlockDef.Components = newComps;

            SetRatios(BlockDef.CriticalGroup);
        }

        internal void ChangeCompCount(int index, int newCount)
        {
            var comp = BlockDef.Components[index];
            int oldCount = comp.Count;
            float intDiff = comp.Definition.MaxIntegrity * (newCount - oldCount);
            float massDiff = comp.Definition.Mass * (newCount - oldCount);

            comp.Count = newCount;

            BlockDef.MaxIntegrity += intDiff;
            BlockDef.Mass += massDiff;

            SetRatios(BlockDef.CriticalGroup);
        }

        internal void SetRatios(int criticalIndex)
        {
            var criticalIntegrity = 0f;
            var ownershipIntegrity = 0f;
            for (var index = 0; index <= criticalIndex; index++)
            {
                var component = BlockDef.Components[index];
                if (ownershipIntegrity == 0f && component.Definition.Id.SubtypeName == "Computer")
                {
                    ownershipIntegrity = criticalIntegrity + component.Definition.MaxIntegrity;
                }
                criticalIntegrity += component.Count * component.Definition.MaxIntegrity;
                if (index == criticalIndex)
                {
                    criticalIntegrity -= component.Definition.MaxIntegrity;
                }
            }

            BlockDef.CriticalIntegrityRatio = criticalIntegrity / BlockDef.MaxIntegrity;
            BlockDef.OwnershipIntegrityRatio = ownershipIntegrity / BlockDef.MaxIntegrity;

            var count = BlockDef.BuildProgressModels.Length;
            for (var index = 0; index < count; index++)
            {
                var buildPercent = (index + 1f) / count;
                BlockDef.BuildProgressModels[index].BuildRatioUpperBound = buildPercent * BlockDef.CriticalIntegrityRatio;
            }
        }


        internal void BeforeStartStuff()
        {
            foreach (var blockDef in Resists.Keys)
            {
                blockDef.GeneralDamageMultiplier = Resists[blockDef];
            } 
        }

    }
}
