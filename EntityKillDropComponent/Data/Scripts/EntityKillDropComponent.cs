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
using VRage.Game.ModAPI.Interfaces;

namespace YourNamespace
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class EntityHitDropComponent : MySessionComponentBase
    {
        private static readonly MyStringHash RequiredItemSubtypeId = MyStringHash.GetOrCompute("Cow");
        private static readonly MyStringHash SpecificCharacterSubtypeId = MyStringHash.GetOrCompute("Cow_Bot");
        private static readonly MyDefinitionId DropComponentId = new MyDefinitionId(typeof(MyObjectBuilder_Component), "Cow");
        private const int DropAmount = 1;
        private bool isInitialized = false;

        public override void LoadData()
        {
            MyLog.Default.WriteLine("EntityHitDropComponent Mod Loaded");
            MyAPIGateway.Utilities.ShowMessage("EntityHitDropComponent", "Mod Loaded");
        }

        protected override void UnloadData()
        {
            MyLog.Default.WriteLine("EntityHitDropComponent Mod Unloaded");
            MyAPIGateway.Utilities.ShowMessage("EntityHitDropComponent", "Mod Unloaded");
            // No need to unregister the damage handler
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            if (MyAPIGateway.Session?.DamageSystem != null)
            {
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, OnEntityHit);
                isInitialized = true;
            }
        }

        private void OnEntityHit(object target, ref MyDamageInformation info)
        {
            var character = target as IMyCharacter;
            if (character != null && character.Definition.Id.SubtypeId == SpecificCharacterSubtypeId)
            {
                MyAPIGateway.Utilities.ShowMessage("EntityHitDropComponent", "Cow_Bot was hit");

                var attackerEntity = MyAPIGateway.Entities.GetEntityById(info.AttackerId);
                var player = MyAPIGateway.Players.GetPlayerControllingEntity(attackerEntity);

                if (player != null)
                {
                    MyAPIGateway.Utilities.ShowMessage("EntityHitDropComponent", $"Cow_Bot hit by player: {player.DisplayName}");
                    if (HasRequiredItem(player))
                    {
                        MyAPIGateway.Utilities.ShowMessage("EntityHitDropComponent", "Player has the required item");
                        DropComponent(character.GetPosition());
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage("EntityHitDropComponent", "Player does not have the required item");
                    }
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
                    if (item.Type.TypeId.ToString() == typeof(MyObjectBuilder_Component).Name && item.Type.SubtypeId.ToString() == RequiredItemSubtypeId.ToString())
                    {
                        MyAPIGateway.Utilities.ShowMessage("EntityHitDropComponent", "Found required item in player inventory");
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

            MyAPIGateway.Utilities.ShowMessage("EntityHitDropComponent", $"Dropped component at position: {position}");
        }
    }
}
