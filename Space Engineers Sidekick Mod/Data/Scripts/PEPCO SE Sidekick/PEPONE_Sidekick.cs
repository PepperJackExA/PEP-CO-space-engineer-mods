using Digi;
using Sandbox.Definitions;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Library.Collections;
using static Sandbox.Game.Replication.History.MySnapshotHistory;

namespace PEPONE_Sidekick
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPONE_SidekickSession : MySessionComponentBase
    {

        public ChatCommands ChatCommands;
        private static PEPONE_SidekickSession instance;
        private TextWriter writer;

        public override void LoadData()
        {
            ChatCommands = new ChatCommands(this);
        }

        public void ExportBlocks(bool fileRequired)
        {

            string outputString = "";
            List<string> blockList = new List<string>();
            List<string> componentList = new List<string>();

            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>())
            {
                if (def.Public)
                {
                    string jsonString = "";
                    string iconString = "";

                    //Remove the unnecessary parts of the icon path
                    iconString = def.Icons?.First().Split('\\').Last();
                    iconString = iconString.Split(new[] { '/' }).Last();

                    //Remove the .dds
                    iconString = iconString.Replace(".dds", "");

                    List<string> blockComponentList = new List<string>();

                    foreach (var component in def.Components)
                    {
                        string testString = $"\"componentID\": \"{component.Definition.Id}\",";
                        blockComponentList.Add($"{{" +
                            $"\"componentID\": \"{component.Definition.Id}\"," +
                            $"\"amount\": {component.Count}," +
                            $"\"displayName\": \"{component.Definition.DisplayNameText}\"}}");
                    }
                    jsonString = $"{{" +
                        $"\"typeId\": \"{def.Id.TypeId}\", " +
                        $"\"subtypeID\": \"{def.Id.SubtypeId}\", " +
                        $"\"size\": \"{def.CubeSize}\", \"displayName\": \"{def.DisplayNameText}\", " +
                        $"\"components\": [{string.Join(",", blockComponentList)}], " +
                        $"\"isCritical\": {def.CriticalGroup.ToString()}, " +
                        $"\"modContext\": \"{def.Context.ModName}({def.Context.ModId})\"," +
                        $"\"icon\": \"{iconString}\"" +
                        $"}}";

                    //Log.Info(jsonString);
                    blockList.Add($"{jsonString}");
                }
            }

            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions().OfType<MyComponentDefinition>())
            {
                if (def.Public)
                {
                    string jsonString = "";


                    jsonString = $"{{ \"componentID\": \"{def.Id}\", \"componentDisplayName\": \"{def.DisplayNameText}\", \"volume\": {def.Volume}, \"weight\": {def.Mass} }}";

                    //Log.Info(jsonString);
                    componentList.Add($"{jsonString}");
                }
            }

            outputString = $"{{\"blocks\": [{string.Join(",", blockList)}], \"components\":\r\n    [{string.Join(",", componentList)}] }}";
            //Log.Info(outputString);

            if (fileRequired)
            {
                writer = MyAPIGateway.Utilities.WriteFileInLocalStorage("sidekickExport.json", typeof(PEPONE_SidekickSession));
                writer.Write(outputString);
                writer.Flush();
                writer.Close();

                VRage.Utils.MyClipboardHelper.SetClipboard($"%AppData%/SpaceEngineers/Storage/{MyAPIGateway.Utilities.GamePaths.ModScopeName}/");
                //IMyHudNotification notify = MyAPIGateway.Utilities.CreateNotification("Your sidekick export was successful!", 10000, MyFontEnum.White);
                //notify.Show();
                MyAPIGateway.Utilities.ShowMessage(Log.ModName, $"Your sidekick export was successful!\n" +
                    $"You can find your export file here: %AppData%/SpaceEngineers/Storage/{MyAPIGateway.Utilities.GamePaths.ModScopeName}/" +
                    $"The path has been copied to your clipboard. Just go ahead and paste it into your file explorer.");
            }
            else
            {
                VRage.Utils.MyClipboardHelper.SetClipboard(outputString);
                MyAPIGateway.Utilities.ShowMessage(Log.ModName, $"Your sidekick export was successful!\n" +
                    $"The data has been copied to your clipboard. Just go ahead and paste it into the sidekick web app.");
                

            }


        }

        protected override void UnloadData()
        {
            ChatCommands?.Dispose();
        }

    }
}
