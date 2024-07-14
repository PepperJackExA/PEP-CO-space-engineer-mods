using Digi;
using Sandbox.Definitions;
using Sandbox.Game;
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
using CollisionLayers = Sandbox.Engine.Physics.MyPhysics.CollisionLayers;
using VRageMath;
using VRage;

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
                        //string testString = $"\"componentID\": \"{component.Definition.Id}\",";
                        blockComponentList.Add($"{{" +
                            $"\"componentID\": \"{component.Definition.Id}\"," +
                            $"\"amount\": {component.Count}," +
                            $"\"displayName\": \"{component.Definition.DisplayNameText}\"}}");
                    }
                    jsonString = $"{{" +
                        //Shouldn't need this anymore, but leaving it in for now
                        //$"\"typeId\": \"{def.Id.TypeId}\", " +
                        //$"\"subtypeID\": \"{def.Id.SubtypeId}\", " +
                        $"\"uniqueID\": \"{def.Id.TypeId}/{def.Id.SubtypeId}\", " +
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
                    string iconString = "";

                    //Remove the unnecessary parts of the icon path
                    iconString = def.Icons?.First().Split('\\').Last();
                    iconString = iconString.Split(new[] { '/' }).Last();

                    //Remove the .dds
                    iconString = iconString.Replace(".dds", "");


                    jsonString = $"{{ " +
                        $"\"componentID\": \"{def.Id}\", " +
                        $"\"componentDisplayName\": \"{def.DisplayNameText}\", " +
                        $"\"volume\": {def.Volume}, " +
                        $"\"weight\": {def.Mass}, " +
                        $"\"icon\": \"{iconString}\"" +
                        $"}}";

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
                    $"The path has been copied to your clipboard. Just go ahead and paste it into your file explorer or into the upload prompt within the web app.");
            }
            else
            {
                VRage.Utils.MyClipboardHelper.SetClipboard(outputString);
                MyAPIGateway.Utilities.ShowMessage(Log.ModName, $"Your sidekick export was successful!\n" +
                    $"The data has been copied to your clipboard. Just go ahead and paste it into the sidekick web app.");

                MyVisualScriptLogicProvider.OpenSteamOverlayLocal("https://steamcommunity.com/sharedfiles/filedetails/?id=3283057514");
            }


        }

        public void ExportGrid()
        {
            MatrixD camWM = MyAPIGateway.Session.Camera.WorldMatrix;

            const double MaxDistance = 50;

            IMyCubeGrid aimedGrid = null;

            List<IHitInfo> hits = new List<IHitInfo>(16);
            MyAPIGateway.Physics.CastRay(camWM.Translation, camWM.Translation + camWM.Forward * MaxDistance, hits, CollisionLayers.NoVoxelCollisionLayer);

            // find first grid hit, ignore everything else
            foreach (IHitInfo hit in hits)
            {
                aimedGrid = hit.HitEntity as IMyCubeGrid;
                if (aimedGrid != null)
                    break;
            }

            if (aimedGrid == null)
            {
                MyAPIGateway.Utilities.ShowMessage(Log.ModName, "No grid found in front of you.");
                return;
            }
            else
            {
                List<MyTuple<string, int>> outputString = new List<MyTuple<string, int>>();

                //Get the grid's blocks
                List<IMySlimBlock> blockList = new List<IMySlimBlock>();
                aimedGrid.GetBlocks(blockList);

                blockList.ForEach(block =>
                {
                    //Test if block already exists in the output list and increment the count
                    if (outputString.Any(x => x.Item1 == block.BlockDefinition.Id.ToString()))
                    {
                        //Replace the old tuple with a new one that has the incremented count
                        outputString = outputString.Select(x => x.Item1 == block.BlockDefinition.Id.ToString() ? new MyTuple<string, int>(x.Item1, x.Item2 + 1) : x).ToList();
                    }
                    else
                    {
                        outputString.Add(new MyTuple<string, int>(block.BlockDefinition.Id.ToString(), 1));
                    }

                });

                string jsonString = "[" +string.Join(",", outputString.Select(tuple => $"{{\"uniqueID\": \"{tuple.Item1}\", \"amount\": {tuple.Item2}}}")) + "]";

                MyAPIGateway.Utilities.ShowMessage(Log.ModName, $"Found {blockList.Count} blocks in grid: {aimedGrid.CustomName}");
                VRage.Utils.MyClipboardHelper.SetClipboard(jsonString);
                Log.Info(jsonString); // Fix: Pass the string to the Log.Info method

                return;
            }
        }

        protected override void UnloadData()
        {
            ChatCommands?.Dispose();
        }

    }
}
