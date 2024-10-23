using System;
using System.IO;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities; // this ingame namespace is safe to use in mods as it has nothing to collide with
using VRage.Utils;
using Draygo.API;
using static Draygo.API.HudAPIv2.MenuRootCategory;
using VRage.Game;
using VRageMath;
using System.Collections.Generic;
using VRage.ModAPI;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRage.Render.Scene;
using Sandbox.Game.Entities.EnvironmentItems;
using Sandbox.Game.WorldEnvironment;
using VRage.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game.WorldEnvironment.ObjectBuilders;
using VRage.Game.ModAPI;

namespace PEPCO
{
    // This example is minimal code required for it to work and with comments so you can better understand what is going on.

    // The gist of it is: ini file is loaded/created that admin can edit, SetVariable is used to store that data in sandbox.sbc which gets automatically sent to joining clients.
    // Benefit of this is clients will be getting this data before they join, very good if you need it during LoadData()
    // This example does not support reloading config while server runs, you can however implement that by sending a packet to all online players with the ini data for them to parse.

    

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class IngameCrafting : MySessionComponentBase
    {
        ExampleSettings Settings = new ExampleSettings();
        public HudAPIv2 TextHUD;


        static readonly MyObjectBuilder_PhysicalObject ItemOB = new MyObjectBuilder_Ore()
        {
            SubtypeName = "Ice",
        };

        public override void LoadData()
        {
            Settings.Load();

            TextHUD = new HudAPIv2(InitTextHud);
        }

        public void InitTextHud()
        {
            if (TextHUD.Heartbeat)
            {
                var rootMenu = new HudAPIv2.MenuRootCategory("Rich Hud Test", MenuFlag.PlayerMenu, "Rich hud stuff");
                var subMenu = new HudAPIv2.MenuSubCategory("Rich Hud Test", rootMenu, "Hi PepperJack");

                var craftingMenu = new HudAPIv2.MenuRootCategory("Crafting", MenuFlag.PlayerMenu, "Crafting");
                var craftingMenuStone = new HudAPIv2.MenuSubCategory("Stone crafting", craftingMenu, "Stone crafting");
                var craftingMenuWood = new HudAPIv2.MenuSubCategory("Wood crafting", craftingMenu, "Wood crafting");
                new HudAPIv2.MenuItem("Craft stone block (100 stone)", craftingMenuStone, () => CraftItem("stone block"));
                new HudAPIv2.MenuItem("Craft camp fire (20 wood)", craftingMenuWood, () => CraftItem("camp fire"));
            }
        }

        public void SendChat(string test)
        {
            //Send a chat to all players
            MyAPIGateway.Utilities.ShowMessage("Rich Hud Test", "You want to craft a "+test);
        }

        public void CraftItem(string item) 
        {
            var player = MyAPIGateway.Session?.LocalHumanPlayer;
            if (player != null)
            {
                List<MyEntity> entities = new List<MyEntity>();
                var playerPosition = player.GetPosition();
                BoundingSphereD sphere = new BoundingSphereD(playerPosition, 40d);
                MyGamePruningStructure.GetAllEntitiesInSphere(ref sphere, entities);


                MyAPIGateway.Utilities.ShowMessage("Total", entities.Count.ToString());


                foreach (var entity in entities)
                {
                    //return the entity type name
                    var newEnt = entity as Sandbox.Game.WorldEnvironment.MyEnvironmentSector;
                    if (newEnt != null) {
                        MyAPIGateway.Utilities.ShowMessage("Entity", newEnt.EnvironmentDefinition.Id.ToString());
                        MyAPIGateway.Utilities.ShowMessage("Entity", newEnt.EnvironmentDefinition.Id.SubtypeId.ToString());
                        var c = entity as Sandbox.Game.WorldEnvironment.MyEnvironmentSector;
                        var assetName = c.Model.ToString();
                        MyAPIGateway.Utilities.ShowMessage("Entity", assetName);
                    }
                }

                //player.Character?.GetInventory()?.AddItems(100, ItemOB);   
            }

            //MyAPIGateway.Utilities.ShowMessage("Craft master", "You want to craft a " + item+"?\nYou only get ice. Ouch!");
        }
    }

    public class ExampleSettings
    {
        const string VariableId = nameof(IngameCrafting); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)
        const string FileName = "Config.ini"; // the file that gets saved to world storage under your mod's folder
        const string IniSection = "Config";

        // settings you'd be reading, and their defaults.
        public float SomeNumber = 1f;
        public bool ToggleThings = true;

        void LoadConfig(MyIni iniParser)
        {
            // repeat for each setting field
            SomeNumber = iniParser.Get(IniSection, nameof(SomeNumber)).ToSingle(SomeNumber);

            ToggleThings = iniParser.Get(IniSection, nameof(ToggleThings)).ToBoolean(ToggleThings);
        }

        void SaveConfig(MyIni iniParser)
        {
            // repeat for each setting field
            iniParser.Set(IniSection, nameof(SomeNumber), SomeNumber);
            iniParser.SetComment(IniSection, nameof(SomeNumber), "This number does something for sure"); // optional

            iniParser.Set(IniSection, nameof(ToggleThings), ToggleThings);
        }

        // nothing to edit below this point

        public ExampleSettings()
        {
        }

        public void Load()
        {
            if (MyAPIGateway.Session.IsServer)
                LoadOnHost();
            else
                LoadOnClient();
        }

        void LoadOnHost()
        {
            MyIni iniParser = new MyIni();

            // load file if exists then save it regardless so that it can be sanitized and updated

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(ExampleSettings)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(ExampleSettings)))
                {
                    string text = file.ReadToEnd();

                    MyIniParseResult result;
                    if (!iniParser.TryParse(text, out result))
                        throw new Exception($"Config error: {result.ToString()}");

                    LoadConfig(iniParser);
                }
            }

            iniParser.Clear(); // remove any existing settings that might no longer exist

            SaveConfig(iniParser);

            string saveText = iniParser.ToString();

            MyAPIGateway.Utilities.SetVariable<string>(VariableId, saveText);

            using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(ExampleSettings)))
            {
                file.Write(saveText);
            }
        }

        void LoadOnClient()
        {
            string text;
            if (!MyAPIGateway.Utilities.GetVariable<string>(VariableId, out text))
                throw new Exception("No config found in sandbox.sbc!");

            MyIni iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(text, out result))
                throw new Exception($"Config error: {result.ToString()}");

            LoadConfig(iniParser);
        }
    }
}