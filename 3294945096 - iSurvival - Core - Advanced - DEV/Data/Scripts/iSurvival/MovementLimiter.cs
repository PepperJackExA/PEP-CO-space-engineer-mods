using PEPCO.iSurvival.Log;
using PEPCO.iSurvival.settings;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace PEPCO.iSurvival.MovementLimiter
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class PlayerMovementLimiter : MySessionComponentBase
    {
        private const float MaxWalkSpeed = 1f; // Walking speed limit in m/s
        private const float MaxFlySpeed = 12.5f; // Flying speed limit in m/s
        private const float GridCheckRadius = 5f; // Radius to check for nearby grids
        private const float WeightImpactFactor = 0.005f; // Factor to decrease speed based on weight

        public override void UpdateAfterSimulation()
        {
            if (!MyAPIGateway.Multiplayer.IsServer) // Only run on the server
                return;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                var character = player.Character;
                if (character == null)
                    continue;

                var physics = character?.Physics;
                if (physics == null)
                    continue;

                // Get player stats
                var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
                MyEntityStat fatigue, stamina, strength, dexterity;

                // Retrieve the necessary stats
                if (!statComp.TryGetStat(MyStringHash.GetOrCompute("Fatigue"), out fatigue) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Stamina"), out stamina) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Strength"), out strength) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Dexterity"), out dexterity))
                    continue;

                

                // Step 1: Calculate player's weight (inventory mass)
                // GetInventory().CurrentMass returns MyFixedPoint, so we cast it to float
                float playerWeight = (float)character.GetInventory().CurrentMass;


                

                // Step 3: Calculate movement speed based on movement state
                Vector3 currentVelocity = physics.LinearVelocity;
                var movementState = player.Character.CurrentMovementState;
                float speedLimit = MaxWalkSpeed;
                float staminaMultiplier = MathHelper.Clamp(stamina.Value / 20f, 0.1f, 2f);
                switch (movementState)
                {
                    case MyCharacterMovementEnum.Sprinting:
                        if (fatigue.Value > 20 && stamina.Value > 20) continue;
                        speedLimit = MaxWalkSpeed + (MaxWalkSpeed * 2 * staminaMultiplier);
                        break;

                    case MyCharacterMovementEnum.Walking:
                        if (fatigue.Value > 20 && stamina.Value > 20) continue;
                        speedLimit = MaxWalkSpeed + (MaxWalkSpeed * staminaMultiplier);
                        break;

                    case MyCharacterMovementEnum.Running:
                        if (fatigue.Value > 20 && stamina.Value > 20) continue;
                        speedLimit = MaxWalkSpeed + (MaxWalkSpeed * 1.5f * staminaMultiplier);
                        break;

                    case MyCharacterMovementEnum.Flying:
                        // Check if dampeners are enabled (this is a common example of a stabilizer)
                        bool stabilizersEnabled = player.Character.EnabledDamping;
                        
                        // Step 2: Check if player is in a gravity field
                        float naturalGravityInterference;  // DECLARE naturalGravityInterference
                        Vector3 gravity = MyAPIGateway.Physics.CalculateNaturalGravityAt(player.GetPosition(), out naturalGravityInterference);

                        // Check if gravity strength exceeds the threshold (0.5 in this case)
                        if (gravity.Length() > 1)
                        {
                            // Set the base fly speed
                            speedLimit = MaxFlySpeed;
                            float gravityFactor = 1.0f - MathHelper.Clamp(playerWeight * WeightImpactFactor, 0.0f, 0.5f);
                            speedLimit *= gravityFactor;
                            if (!stabilizersEnabled)
                            {
                                speedLimit *= 2;
                            }
                            break;
                        }
                        continue;

                    default:
                        continue;
                }

                // Get nearby grid velocity if applicable
                Vector3? nearbyGridVelocity = GetNearbyGridVelocity(character);

                // Calculate and apply velocity adjustments
                Vector3 adjustedVelocity;
                if (nearbyGridVelocity.HasValue)
                {
                    Vector3 relativeVelocity = currentVelocity - nearbyGridVelocity.Value;
                    Vector3 clampedRelativeVelocity = ClampVelocityToLimit(relativeVelocity, speedLimit);
                    adjustedVelocity = nearbyGridVelocity.Value + clampedRelativeVelocity;
                }
                else
                {
                    adjustedVelocity = ClampVelocityToLimit(currentVelocity, speedLimit);
                }

                if (!adjustedVelocity.Equals(currentVelocity))
                {
                    physics.LinearVelocity = adjustedVelocity;
                }
            }
        }


        private Vector3? GetNearbyGridVelocity(IMyCharacter character)
        {
            // Get nearby grids within the check radius
            List<MyEntity> nearbyEntities = new List<MyEntity>();
            BoundingSphereD searchSphere = new BoundingSphereD(character.GetPosition(), GridCheckRadius);
            MyGamePruningStructure.GetAllEntitiesInSphere(ref searchSphere, nearbyEntities);

            IMyCubeGrid closestLargestGrid = null;
            double closestDistance = double.MaxValue;

            foreach (MyEntity entity in nearbyEntities)
            {
                IMyCubeGrid grid = entity as IMyCubeGrid;
                if (grid != null && grid.Physics != null)
                {
                    if (grid.GridSizeEnum == MyCubeSize.Large || grid.GridSizeEnum == MyCubeSize.Small)
                    {
                        double distanceToGrid = Vector3D.Distance(character.GetPosition(), grid.GetPosition());
                        if (closestLargestGrid == null ||
                            grid.GridSizeEnum == MyCubeSize.Large && closestLargestGrid.GridSizeEnum != MyCubeSize.Large ||
                            (grid.GridSizeEnum == closestLargestGrid.GridSizeEnum && distanceToGrid < closestDistance))
                        {
                            closestLargestGrid = grid;
                            closestDistance = distanceToGrid;
                        }
                    }
                }
            }

            return closestLargestGrid?.Physics.LinearVelocity;
        }

        private Vector3 ClampVelocityToLimit(Vector3 velocity, float speedLimit)
        {
            float currentSpeed = velocity.Length();
            if (currentSpeed > speedLimit)
            {
                velocity = velocity * (speedLimit / currentSpeed);
            }
            return velocity;
        }
    }
}
