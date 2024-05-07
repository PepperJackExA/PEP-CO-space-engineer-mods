using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.ModAPI;

namespace XMLScriptTest
{
    public class Wheel7x7Config
    {
        public float generalDamageMultiplier;
        public float disassembleRatio;
        public float wheelSuspensionPower;
        public List<List<string>> wheelSuspensionComponents;
        public List<List<string>> wheelBlockComponents;
        public List<List<string>> wheelRealComponents;

        public Wheel7x7Config(float generalDamageMultiplier, float disassembleRatio, float wheelSuspensionPower, List<List<string>> wheelSuspensionComponents, List<List<string>> wheelBlockComponents, List<List<string>> wheelRealComponents)
        {
            this.generalDamageMultiplier = generalDamageMultiplier;
            this.disassembleRatio = disassembleRatio;
            this.wheelSuspensionPower = wheelSuspensionPower;
            this.wheelSuspensionComponents = wheelSuspensionComponents;
            this.wheelBlockComponents = wheelBlockComponents;
            this.wheelRealComponents = wheelRealComponents;
        }

        public Wheel7x7Config()
        {
            var settings = Wheel7x7Settings.getSettings();
            generalDamageMultiplier = settings.generalDamageMultiplier;
            disassembleRatio = settings.disassembleRatio;
            wheelSuspensionPower = settings.wheelSuspensionPower;
            wheelSuspensionComponents = settings.wheelSuspensionComponents;
            wheelBlockComponents = settings.wheelBlockComponents;
            wheelRealComponents = settings.wheelRealComponents;
        }

        public static Wheel7x7Config LoadSettings()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("ModConfig.xml", typeof(Wheel7x7Config)) == true)
            {
                var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("ModConfig.xml", typeof(Wheel7x7Config));
                return MyAPIGateway.Utilities.SerializeFromXML<Wheel7x7Config>(reader.ReadToEnd());
            }

            var settings = new Wheel7x7Config();
            SaveSettings(settings);
            return settings;
        }

        private static void SaveSettings(Wheel7x7Config settings)
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("ModConfig.xml", typeof(Wheel7x7Config)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(settings));
            }
        }
    }

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Wheel7x7Settings : MySessionComponentBase
    {
        private static float generalDamageMultiplier = 1;
        private static float disassembleRatio = 1;
        private static float wheelSuspensionPower = 1;
        private static List<List<string>> wheelSuspensionComponents = new List<List<string>>
        {
            new List<string> { "Suspension3x3" },
            new List<string> { "Suspension5x5" },
            new List<string> { "Suspension7x7" },
            new List<string> { "Suspension1x1" },
            new List<string> { "SmallSuspension3x3" },
            new List<string> { "SmallSuspension5x5" },
            new List<string> { "SmallSuspension7x7" },
            new List<string> { "SmallSuspension1x1" },
            new List<string> { "Suspension3x3mirrored" },
            new List<string> { "Suspension5x5mirrored" },
            new List<string> { "Suspension7x7mirrored" },
            new List<string> { "Suspension1x1mirrored" },
            new List<string> { "SmallSuspension3x3mirrored" },
            new List<string> { "SmallSuspension5x5mirrored" },
            new List<string> { "SmallSuspension7x7mirrored" },
            new List<string> { "SmallSuspension1x1mirrored" },
            new List<string> { "OffroadSuspension3x3" },
            new List<string> { "OffroadSuspension5x5" },
            new List<string> { "OffroadSuspension7x7" },
            new List<string> { "OffroadSuspension1x1" },
            new List<string> { "OffroadSmallSuspension3x3" },
            new List<string> { "OffroadSmallSuspension5x5" },
            new List<string> { "OffroadSmallSuspension7x7" },
            new List<string> { "OffroadSmallSuspension1x1" },
            new List<string> { "OffroadSuspension3x3mirrored" },
            new List<string> { "OffroadSuspension5x5mirrored" },
            new List<string> { "OffroadSuspension7x7mirrored" },
            new List<string> { "OffroadSuspension1x1mirrored" },
            new List<string> { "OffroadSmallSuspension3x3mirrored" },
            new List<string> { "OffroadSmallSuspension5x5mirrored" },
            new List<string> { "OffroadSmallSuspension7x7mirrored" },
            new List<string> { "OffroadSmallSuspension1x1mirrored" },
        };
        private static List<List<string>> wheelBlockComponents = new List<List<string>>
        {
            new List<string> { "Wheel1x1" },
            new List<string> { "SmallWheel1x1" },
            new List<string> { "Wheel3x3" },
            new List<string> { "SmallWheel3x3" },
            new List<string> { "Wheel5x5" },
            new List<string> { "SmallWheel5x5" },
            new List<string> { "Wheel7x7" },
            new List<string> { "SmallWheel7x7" },
            new List<string> { "OffroadWheel1x1" },
            new List<string> { "OffroadSmallWheel1x1" },
            new List<string> { "OffroadWheel3x3" },
            new List<string> { "OffroadSmallWheel3x3" },
            new List<string> { "OffroadWheel5x5" },
            new List<string> { "OffroadSmallWheel5x5" },
            new List<string> { "OffroadWheel7x7" },
            new List<string> { "OffroadSmallWheel7x7" }
        };
        private static List<List<string>> wheelRealComponents = new List<List<string>>
        {
            new List<string> { "SmallRealWheel1x1" },
            new List<string> { "SmallRealWheel" },
            new List<string> { "SmallRealWheel5x5" },
            new List<string> { "SmallRealWheel7x7" },
            new List<string> { "RealWheel1x1" },
            new List<string> { "RealWheel" },
            new List<string> { "RealWheel5x5" },
            new List<string> { "RealWheel7x7" },
            new List<string> { "SmallRealWheel1x1mirrored" },
            new List<string> { "SmallRealWheelmirrored" },
            new List<string> { "SmallRealWheel5x5mirrored" },
            new List<string> { "SmallRealWheel7x7mirrored" },
            new List<string> { "RealWheel1x1mirrored" },
            new List<string> { "RealWheelmirrored" },
            new List<string> { "RealWheel5x5mirrored" },
            new List<string> { "RealWheel7x7mirrored" },
            new List<string> { "OffroadSmallRealWheel1x1" },
            new List<string> { "OffroadSmallRealWheel" },
            new List<string> { "OffroadSmallRealWheel5x5" },
            new List<string> { "OffroadSmallRealWheel7x7" },
            new List<string> { "OffroadRealWheel1x1" },
            new List<string> { "OffroadRealWheel" },
            new List<string> { "OffroadRealWheel5x5" },
            new List<string> { "OffroadRealWheel7x7" },
            new List<string> { "OffroadSmallRealWheel1x1mirrored" },
            new List<string> { "OffroadSmallRealWheelmirrored" },
            new List<string> { "OffroadSmallRealWheel5x5mirrored" },
            new List<string> { "OffroadSmallRealWheel7x7mirrored" },
            new List<string> { "OffroadRealWheel1x1mirrored" },
            new List<string> { "OffroadRealWheelmirrored" },
            new List<string> { "OffroadRealWheel5x5mirrored" },
            new List<string> { "OffroadRealWheel7x7mirrored" }
        };


        public override void LoadData()
        {
            GetDefinitions();

            if (MyAPIGateway.Multiplayer.IsServer || MyAPIGateway.Utilities.IsDedicated)
                SaveSettings();
            LoadSettings();

            SetDefinitions();
        }

        private void GetDefinitions()
        {
            foreach (var block in wheelSuspensionComponents)
            {
                var builder = new MyObjectBuilder_MotorSuspension { SubtypeName = block[0] };
                var blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(builder);
                GetComponents(blockDefinition, block);
            }
            foreach (var block in wheelBlockComponents)
            {
                var builder = new MyObjectBuilder_Wheel { SubtypeName = block[0] };
                var blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(builder);
                GetComponents(blockDefinition, block);
            }
            foreach (var block in wheelRealComponents)
            {
                var builder = new MyObjectBuilder_Wheel { SubtypeName = block[0] };
                var blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(builder);
                GetComponents(blockDefinition, block);
            }
        }

        private void GetComponents(MyCubeBlockDefinition blockDefinition, List<string> block)
        {
            generalDamageMultiplier = blockDefinition.GeneralDamageMultiplier;
            disassembleRatio = blockDefinition.DisassembleRatio;

            var total = 0;
            var hasMainComponent = false;
            foreach (var component in blockDefinition.Components)
                block.Add(component.Definition.Id.SubtypeName + "=" + component.Count);
            block[blockDefinition.CriticalGroup + 1] += "=critical";
        }

        private void SetDefinitions()
        {
            foreach (var block in wheelSuspensionComponents)
            {
                var builder = new MyObjectBuilder_MotorSuspension { SubtypeName = block[0] };
                var blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(builder) as MyMotorSuspensionDefinition;
                blockDefinition.PropulsionForce *= wheelSuspensionPower;
                SetComponents(blockDefinition, block);
            }
            foreach (var block in wheelBlockComponents)
            {
                var builder = new MyObjectBuilder_Wheel { SubtypeName = block[0] };
                var blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(builder);
                SetComponents(blockDefinition, block);
            }

            foreach (var block in wheelRealComponents)
            {
                var builder = new MyObjectBuilder_Wheel { SubtypeName = block[0] };
                var blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(builder);
                SetComponents(blockDefinition, block);
            }
        }

        private void SetComponents(MyCubeBlockDefinition blockDefinition, List<string> block)
        {
            try
            {
                blockDefinition.GeneralDamageMultiplier = generalDamageMultiplier;
                blockDefinition.DisassembleRatio = disassembleRatio;

                var mass = 0f;
                var maxIntegrity = 0;
                var criticalRatio = 0f;
                var ownershipRatio = 0f;
                var newComps = new List<MyCubeBlockDefinition.Component>();
                foreach (var component in block.Where(component => component.Contains('=')))
                {
                    var compProperties = component.Split('=').ToList();
                    var compName = compProperties[0];
                    var compCount = Int32.Parse(compProperties[1]);
                    var compBuilder = new MyObjectBuilder_Component { SubtypeName = compName };
                    var compDef = MyDefinitionManager.Static.GetComponentDefinition(compBuilder.GetObjectId());
                    var newComp = new MyCubeBlockDefinition.Component
                    {
                        Definition = compDef,
                        Count = compCount,
                        DeconstructItem = compDef
                    };

                    newComps.Add(newComp);
                    mass += newComp.Definition.Mass * newComp.Count;
                    ownershipRatio = compName.Equals("Computer") ? maxIntegrity : ownershipRatio;
                    maxIntegrity += newComp.Definition.MaxIntegrity * newComp.Count;
                    criticalRatio = compProperties.Contains("critical") ? maxIntegrity : criticalRatio;
                }

                blockDefinition.Mass = mass;
                blockDefinition.MaxIntegrity = maxIntegrity;
                blockDefinition.Components = newComps.ToArray();
                blockDefinition.CriticalIntegrityRatio = criticalRatio / maxIntegrity;
                blockDefinition.OwnershipIntegrityRatio = ownershipRatio / maxIntegrity;
            }
            catch (Exception e)
            {
                //MyVisualScriptLogicProvider.SendChatMessage(e.ToString());
            }
        }

        public static Wheel7x7Config getSettings()
        {
            return new Wheel7x7Config(
                generalDamageMultiplier,
                disassembleRatio,
                wheelSuspensionPower,
                wheelSuspensionComponents,
                wheelBlockComponents,
                wheelRealComponents);
        }

        private void SaveSettings()
        {
            var configuration = Wheel7x7Config.LoadSettings();
            generalDamageMultiplier = configuration.generalDamageMultiplier;
            disassembleRatio = configuration.disassembleRatio;
            wheelSuspensionPower = configuration.wheelSuspensionPower;
            wheelSuspensionComponents = configuration.wheelSuspensionComponents.GetRange(wheelSuspensionComponents.Count / 2, wheelSuspensionComponents.Count / 2);
            wheelBlockComponents = configuration.wheelBlockComponents.GetRange(wheelBlockComponents.Count / 2, wheelBlockComponents.Count / 2);
            wheelRealComponents = configuration.wheelRealComponents.GetRange(wheelRealComponents.Count / 2, wheelRealComponents.Count / 2);
            MyAPIGateway.Utilities.SetVariable("Wheel7x7_generalDamageMultiplier", generalDamageMultiplier);
            MyAPIGateway.Utilities.SetVariable("Wheel7x7_disassembleRatio", disassembleRatio);
            MyAPIGateway.Utilities.SetVariable("Wheel7x7_wheelSuspensionPower", wheelSuspensionPower);
            for (var i = 0; i < wheelSuspensionComponents.Count; i++)
                MyAPIGateway.Utilities.SetVariable("Wheel7x7_wheelSuspensionComponents" + i, wheelSuspensionComponents[i].ToArray());
            for (var i = 0; i < wheelBlockComponents.Count; i++)
                MyAPIGateway.Utilities.SetVariable("Wheel7x7_wheelBlockComponents" + i, wheelBlockComponents[i].ToArray());
            for (var i = 0; i < wheelRealComponents.Count; i++)
                MyAPIGateway.Utilities.SetVariable("Wheel7x7_wheelRealComponents" + i, wheelRealComponents[i].ToArray());
        }

        private void LoadSettings()
        {
            try
            {
                MyAPIGateway.Utilities.GetVariable("Wheel7x7_generalDamageMultiplier", out generalDamageMultiplier);
                MyAPIGateway.Utilities.GetVariable("Wheel7x7_disassembleRatio", out disassembleRatio);
                MyAPIGateway.Utilities.GetVariable("Wheel7x7_wheelSuspensionPower", out wheelSuspensionPower);
                for (var i = 0; i < wheelSuspensionComponents.Count; i++)
                {
                    string[] newList;
                    MyAPIGateway.Utilities.GetVariable("Wheel7x7_wheelSuspensionComponents" + i, out newList);
                    wheelSuspensionComponents[i] = new List<string>(newList);
                }
                for (var i = 0; i < wheelBlockComponents.Count; i++)
                {
                    string[] newList;
                    MyAPIGateway.Utilities.GetVariable("Wheel7x7_wheelBlockComponents" + i, out newList);
                    wheelBlockComponents[i] = new List<string>(newList);
                }
                for (var i = 0; i < wheelRealComponents.Count; i++)
                {
                    string[] newList;
                    MyAPIGateway.Utilities.GetVariable("Wheel7x7_wheelRealComponents" + i, out newList);
                    wheelRealComponents[i] = new List<string>(newList);
                }
            }
            catch (Exception e)
            {
                //MyVisualScriptLogicProvider.SendChatMessage(e.ToString());
            }
        }
    }
}
