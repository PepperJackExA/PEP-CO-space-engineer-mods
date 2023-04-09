namespace ModAdjuster
{
    public class DefinitionStructure
    {
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

                }
            }
        }
    }
}
