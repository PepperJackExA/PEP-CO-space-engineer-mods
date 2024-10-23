using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using System;
using VRage.Game;
using System.Collections.Generic;
using PEPCO.iSurvival.stats; // Reference to iSurvival for accessing Calories stat

using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Components;

namespace YourModNamespace
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class CatchConsumables : MySessionComponentBase
    {
        private const string CaloriesStatName = "Calories"; // iSurvival's Calories stat

        public override void LoadData()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, OnItemConsumed);
        }

        protected override void UnloadData()
        {
            //MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, OnItemConsumed);
        }

        private void OnItemConsumed(object target, ref MyDamageInformation info)
        {
            // Check if the action involves a consumable item
            if (target is IMyCharacter character && info.Type == MyDamageType.Environment)
            {
                var player = MyAPIGateway.Players.GetPlayerControllingEntity(character);
                if (player != null)
                {
                    // Intercept and cancel the hunger effect
                    bool handled = HandleConsumable(player);
                    if (handled)
                    {
                        info.Amount = 0; // Cancel the vanilla hunger change
                    }
                }
            }
        }

        private bool HandleConsumable(IMyPlayer player)
        {
            try
            {
                // Access the player's stats component
                var statComp = player.Character?.Components?.Get<MyEntityStatComponent>();
                if (statComp == null) return false;

                // Get the Calories stat
                MyEntityStat caloriesStat;
                if (statComp.TryGetStat(MyStringHash.GetOrCompute(CaloriesStatName), out caloriesStat))
                {
                    // Example: Increase Calories by 10 per consumption (adjust to your preference)
                    Core.iSurvivalSession.ApplyStatChange(player, caloriesStat, 1.0, 10.0);
                    MyAPIGateway.Utilities.ShowMessage("Calories", $"Calories increased by 10. Current: {caloriesStat.Value}/{caloriesStat.MaxValue}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"[CatchConsumables] Error handling consumable: {ex.Message}");
            }
            return false;
        }
    }
}
