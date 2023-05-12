namespace ModAdjuster
{
    public class DefinitionStructure
    {
        public struct GasDef
        {
            internal string GasName;
            internal GasAction[] GasActions;

            public struct GasAction
            {
                internal GasMod Action;
                internal float Value;
                internal string NewText;

                public enum GasMod
                {
                    ChangeEnergyDensity,
                    ChangeDisplayName,
                }
            }
        }
        public struct BlockDef
        {
            internal string BlockName;
            internal BlockAction[] BlockActions;

            public struct BlockAction
            {
                internal BlockMod Action;
                internal int Index;
                internal string Component;
                internal int Count;
                internal float Value;
                internal string NewText;

                public enum BlockMod
                {
                    DisableBlockDefinition,
                    ChangeBlockName,
                    ChangeBlockDescription,
                    ChangeBlockPublicity,
                    ChangeBlockGuiVisibility,
                    ChangePCU,
                    ChangeBuildTime,
                    ChangeDeformationRatio,
                    ChangeResistance,
                    ChangeIcon,

                    InsertComponent,
                    ReplaceComponent,
                    RemoveComponent,
                    ChangeComponentDeconstructId,
                    ChangeCriticalComponentIndex,

                    ChangeFuelMultiplier,
                    ChangeFuelCapacity,

                    ChangeMaxPowerOutput,
                    ChangeOptimalWindSpeed,

                    ChangeBatteryPowerStorage,
                    ChangeBatteryMaxoutput,
                    ChangeBatteryMaxRequiredInput,

                    ChangeThrustForce,
                    ChangeThrustPowerConsumption,
                    ChangeThrustFuelId,
                    ChangeThrustFuelEfficiency,


                }
            }
        }
    }
}
