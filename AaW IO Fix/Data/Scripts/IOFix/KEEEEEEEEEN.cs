using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using Digi;
using Sandbox.ModAPI;
using Sandbox.Definitions;
using VRage.Game;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace IOFix
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class IOFix : MySessionComponentBase
    {
        public readonly List<MyEntity> Entities = new List<MyEntity>();
        public readonly List<MyPlanet> Planets = new List<MyPlanet>();

        private Func<IMyEntity, bool> entityFilterCached;


        public override void LoadData()
        {


            var ioDetected = false;

            byte startIndex = (byte)0;

            var Planets = new List<string>() { "Planet Agaris", "Planet Crait", "Planet Lorus", "Planet Lezuno", "Planet Thora 4" };
            var lastMaterials = new List<string>() { "CrushedSulfur", "CrushedCopper", "CrushedLithium", "CrushedBauxite", "CrushedTitanium", "CrushedTantalum", "CrushedNiter", "CrushedIron", "CrushedNickel", "CrushedCobalt", "CrushedSilicon", "CrushedSilver", "CrushedGold", "CrushedPlatinum", "CrushedUranium",
             "PurifiedSulfur", "PurifiedCopper", "PurifiedLithium", "PurifiedBauxite", "PurifiedTitanium", "PurifiedTantalum", "PurifiedNiter", "PurifiedIron", "PurifiedNickel", "PurifiedCobalt", "PurifiedSilicon", "PurifiedSilver", "PurifiedGold", "PurifiedPlatinum", "PurifiedUranium"
            };
            List<MyVoxelMaterialDefinition> lastMatrialsList = new List<MyVoxelMaterialDefinition>();
            List<MyVoxelMaterialDefinition> newMatrialsList = new List<MyVoxelMaterialDefinition>();



            foreach (var mod in MyAPIGateway.Session.Mods)
            {
                if (mod.PublishedFileId == 2344068716)
                {




                    ioDetected = true;
                    break;

                }

            }

            var allVoxelMaterials = MyDefinitionManager.Static.GetVoxelMaterialDefinitions();
            foreach (MyVoxelMaterialDefinition material in allVoxelMaterials)
            {
                //Find material where the subtypeId is in the lastMaterials list
                if (!lastMaterials.Contains(material.Id.SubtypeName))
                {
                    Log.Info("IOFix: Found " + material.Id.SubtypeName + " at index " + material.Index);
                    newMatrialsList.Add(material);
                }
                else
                {
                    lastMatrialsList.Add(material);
                    Log.Info("IOFix: Skipping " + material.Id.SubtypeName);
                }
            }
            foreach (MyVoxelMaterialDefinition material in lastMatrialsList)
            {

                Log.Info("IOFix: Found " + material.Id.SubtypeName + " at index " + material.Index);
                newMatrialsList.Add(material);

            }
            try
            {
                MyAPIGateway.Entities.GetEntities(null, entityFilterCached); //Get all entities in the game and filter them to find planets
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            Log.Info("Found " + Planets.Count + " planets");

        }



        private bool EntityFilter(IMyEntity ent) //This is a filter for the entities that returns true if the entity is a planet
        {
            var p = ent as MyPlanet; //Is this entity a planet?

            if (p != null)
                Planets.Add(p); //Look, we found a planet!

            return false; // don't add to the list, it's null
        }
    }
}
