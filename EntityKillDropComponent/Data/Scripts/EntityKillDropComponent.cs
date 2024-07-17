using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using VRage.ObjectBuilders;
using VRage.Game;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;
using VRage;
using VRage.Game.Entity;

namespace YourNamespace
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class EntityKillDropComponent : MySessionComponentBase
    {
        private static readonly MyStringHash RequiredItemSubtypeId = MyStringHash.GetOrCompute("Cow");
        private static readonly MyStringHash SpecificCharacterSubtypeId = MyStringHash.GetOrCompute("Cow_Bot");
        private static readonly MyDefinitionId DropComponentId = new MyDefinitionId(typeof(MyObjectBuilder_Component), "Cow");
        private const int DropAmount = 1;

        public override void LoadData()
        {
            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
        }

        private void OnEntityAdd(VRage.ModAPI.IMyEntity entity)
        {
            var character = entity as IMyCharacter;
            if (character != null && character.Definition.Id.SubtypeId == SpecificCharacterSubtypeId)
            {
                character.OnClosing += OnCharacterClosing;
            }
        }

        private void OnCharacterClosing(VRage.ModAPI.IMyEntity entity)
        {
            var character = entity as IMyCharacter;
            if (character != null)
            {
                var player = MyAPIGateway.Players.GetPlayerControllingEntity(character);

                if (player != null && HasRequiredItem(player))
                {
                    DropComponent(character.GetPosition());
                }
            }
        }

        private bool HasRequiredItem(IMyPlayer player)
        {
            var inventory = player.Character.GetInventory() as VRage.Game.ModAPI.IMyInventory;
            if (inventory != null)
            {
                var items = new List<MyInventoryItem>();
                inventory.GetItems(items);
                foreach (var item in items)
                {
                        if (item.Type.TypeId == typeof(MyObjectBuilder_Component).Name && item.Type.SubtypeId == RequiredItemSubtypeId.ToString())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void DropComponent(Vector3D position)
        {
            // Create the item object builder for the component to drop
            var itemObjectBuilder = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(DropComponentId);

            // Create the inventory item with the specified amount
            var inventoryItem = new MyPhysicalInventoryItem((VRage.MyFixedPoint)DropAmount, itemObjectBuilder);

            // Spawn the item at the given position with specified orientation
            MyFloatingObjects.Spawn(inventoryItem, position, Vector3D.Up, Vector3D.Up);
        }
    }
}
