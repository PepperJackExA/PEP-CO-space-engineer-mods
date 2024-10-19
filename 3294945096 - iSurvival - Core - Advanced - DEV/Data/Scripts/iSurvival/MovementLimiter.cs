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
        private const float MaxWalkSpeed = 1f; // Desired walking speed limit in m/s
        private const float MaxFlySpeed = 5f; // Desired flying speed limit in m/s
        private const float GridCheckRadius = 100f; // Radius to check for nearby grids

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

                // Initialize the stats
                var statComp = player.Character.Components?.Get<MyEntityStatComponent>();
                MyEntityStat fatigue, stamina, strength, dexterity;

                // Retrieve each stat from the component
                if (!statComp.TryGetStat(MyStringHash.GetOrCompute("Fatigue"), out fatigue) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Stamina"), out stamina) || 
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Strength"), out strength) ||
                    !statComp.TryGetStat(MyStringHash.GetOrCompute("Dexterity"), out dexterity))
                    continue;

                // Apply speed reduction if both stats are low
                if (fatigue.Value > 20 && stamina.Value > 20)
                    continue;

                // Calculate the current velocity of the player.
                Vector3 currentVelocity = physics.LinearVelocity;
                var movementState = player.Character.CurrentMovementState;
                float speedLimit = MaxWalkSpeed;
                switch (movementState)
                {
                    case MyCharacterMovementEnum.Standing:
                    case MyCharacterMovementEnum.RotatingLeft:
                    case MyCharacterMovementEnum.RotatingRight:
                        continue;
                        break;
                    case MyCharacterMovementEnum.Sprinting:
                        // Apply sprinting speed limit (use a higher multiplier for sprinting)
                        // Speed boost based on Dexterity
                        float dexterityBoost = MathHelper.Clamp(dexterity.Value / 20f, 0.1f, 1.5f);
                        speedLimit = MaxWalkSpeed * 2f * dexterityBoost;
                        break;
                    case MyCharacterMovementEnum.Crouching:
                    case MyCharacterMovementEnum.CrouchRotatingLeft:
                    case MyCharacterMovementEnum.CrouchRotatingRight:
                        break;
                    case MyCharacterMovementEnum.CrouchWalking:
                    case MyCharacterMovementEnum.CrouchBackWalking:
                    case MyCharacterMovementEnum.CrouchWalkingLeftBack:
                    case MyCharacterMovementEnum.CrouchWalkingLeftFront:
                    case MyCharacterMovementEnum.CrouchWalkingRightBack:
                    case MyCharacterMovementEnum.CrouchWalkingRightFront:
                    case MyCharacterMovementEnum.CrouchStrafingLeft:
                    case MyCharacterMovementEnum.CrouchStrafingRight:
                        // Apply crouching speed limit (lower limit for crouching)
                        speedLimit = MaxWalkSpeed * 0.5f * MathHelper.Clamp(stamina.Value / 20f, 0.1f, 1f);
                        //MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Crouching speedLimit: {speedLimit}");
                        break;
                    case MyCharacterMovementEnum.Walking:
                    case MyCharacterMovementEnum.BackWalking:
                    case MyCharacterMovementEnum.WalkStrafingLeft:
                    case MyCharacterMovementEnum.WalkStrafingRight:
                    case MyCharacterMovementEnum.WalkingRightBack:
                    case MyCharacterMovementEnum.WalkingRightFront:
                        // Apply walking speed limit
                        speedLimit = MaxWalkSpeed * MathHelper.Clamp(stamina.Value / 20f, 0.1f, 1f);
                        //MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Walking speedLimit: {speedLimit}");
                        break;
                    case MyCharacterMovementEnum.Running:
                    case MyCharacterMovementEnum.Backrunning:
                    case MyCharacterMovementEnum.RunningLeftBack:
                    case MyCharacterMovementEnum.RunningLeftFront:
                    case MyCharacterMovementEnum.RunningRightBack:
                    case MyCharacterMovementEnum.RunningRightFront:
                    case MyCharacterMovementEnum.RunStrafingLeft:
                    case MyCharacterMovementEnum.RunStrafingRight:
                        // Apply running speed limit (increase the limit for running)
                        speedLimit = MaxWalkSpeed * 1.5f * MathHelper.Clamp(stamina.Value / 20f, 0.1f, 1f);
                        //MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Running speedLimit: {speedLimit}");
                        break;
                    case MyCharacterMovementEnum.LadderUp:
                    case MyCharacterMovementEnum.LadderDown:
                        continue;
                        break;
                    case MyCharacterMovementEnum.Flying:
                        // Apply flying speed limit
                        //speedLimit = MaxFlySpeed * MathHelper.Clamp(stamina.Value / 20f, 0.1f, 1f);
                        //MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Flying speedLimit: {speedLimit}");
                        continue;
                        break;
                    case MyCharacterMovementEnum.Falling:
                        continue;
                        break;
                    case MyCharacterMovementEnum.Jump:
                        continue;
                        break;
                    default:
                        // Handle other movement states or do nothing
                        continue;
                        break;
                }




                // Get the nearby grid velocity and check if it exists.
                Vector3? nearbyGridVelocity = GetNearbyGridVelocity(character);

                // Calculate the relative velocity based on whether a grid is nearby
                Vector3 adjustedVelocity;
                if (nearbyGridVelocity.HasValue)
                {
                    // Calculate relative velocity with the grid's velocity
                    Vector3 relativeVelocity = currentVelocity - nearbyGridVelocity.Value;

                    // Clamp the relative velocity based on the speed limit
                    Vector3 clampedRelativeVelocity = ClampVelocityToLimit(relativeVelocity, speedLimit);

                    // Calculate the final velocity by adding the clamped relative velocity to the grid's velocity
                    adjustedVelocity = nearbyGridVelocity.Value + clampedRelativeVelocity;
                }
                else
                {
                    // If no grid is nearby, clamp the current velocity directly to the speed limit
                    adjustedVelocity = ClampVelocityToLimit(currentVelocity, speedLimit);
                }

                // Apply the new velocity to the character's physics if there's a significant change
                if (!adjustedVelocity.Equals(currentVelocity))
                {
                    physics.LinearVelocity = adjustedVelocity;

                    // Optionally log or notify the client for debugging
                    //MyAPIGateway.Utilities.ShowMessage(iSurvivalLog.ModName, $"Velocity updated: {adjustedVelocity.Length()} m/s");
                }

            }
        }

        private Vector3? GetNearbyGridVelocity(IMyCharacter character)
        {
            // Get nearby grids within the check radius.
            List<MyEntity> nearbyEntities = new List<MyEntity>();
            BoundingSphereD searchSphere = new BoundingSphereD(character.GetPosition(), GridCheckRadius);

            // Use MyGamePruningStructure to get entities in the sphere.
            MyGamePruningStructure.GetAllEntitiesInSphere(ref searchSphere, nearbyEntities);

            IMyCubeGrid closestLargestGrid = null;
            double closestDistance = double.MaxValue;

            foreach (MyEntity entity in nearbyEntities)
            {
                IMyCubeGrid grid = entity as IMyCubeGrid;
                if (grid != null && grid.Physics != null)
                {
                    // Ensure the grid is either a small or large grid.
                    if (grid.GridSizeEnum == MyCubeSize.Large || grid.GridSizeEnum == MyCubeSize.Small)
                    {
                        // Calculate the distance between the player and the current grid.
                        double distanceToGrid = Vector3D.Distance(character.GetPosition(), grid.GetPosition());

                        // Check if this grid is larger or closer compared to the previously found grid.
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

            // If a closest and largest grid is found, return its velocity.
            if (closestLargestGrid != null)
            {
                return closestLargestGrid.Physics.LinearVelocity;
            }

            // No suitable grid found within the radius.
            return null;
        }

        private Vector3 ClampVelocityToLimit(Vector3 velocity, float speedLimit)
        {
            // Get the current magnitude of the velocity.
            float currentSpeed = velocity.Length();

            // If the current speed is greater than the speed limit, clamp it.
            if (currentSpeed > speedLimit)
            {
                // Scale the velocity vector down to the speed limit while preserving the direction.
                velocity = velocity * (speedLimit / currentSpeed);
            }

            return velocity;
        }
    }
}
