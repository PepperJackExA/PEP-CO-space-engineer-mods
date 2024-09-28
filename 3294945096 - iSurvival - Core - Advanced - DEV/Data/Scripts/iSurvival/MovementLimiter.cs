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
using VRageMath;

namespace PEPCO.iSurvival.MovementLimiter
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class PlayerMovementLimiter : MySessionComponentBase
    {
        private const float MaxWalkSpeed = 1f; // Desired walking speed limit in m/s
        private const float MaxFlySpeed = 10f; // Desired flying speed limit in m/s
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

                // Get the current velocity of the player.
                Vector3 currentVelocity = physics.LinearVelocity;

                // Check if the player is flying or walking.
                bool isFlying = character.CurrentMovementState == MyCharacterMovementEnum.Flying;

                // Determine the speed limit based on movement type.
                float speedLimit = isFlying ? MaxFlySpeed : MaxWalkSpeed;

                // Get the nearby grid velocity and check if it exists.
                Vector3? nearbyGridVelocity = GetNearbyGridVelocity(character);

                if (nearbyGridVelocity.HasValue)
                {
                    // Calculate the maximum allowed relative speed difference.
                    Vector3 relativeVelocity = currentVelocity - nearbyGridVelocity.Value;
                    float maxRelativeSpeed = speedLimit;

                    // Clamp the relative velocity to ensure it does not exceed the allowed difference.
                    Vector3 clampedRelativeVelocity = ClampVelocityToLimit(relativeVelocity, maxRelativeSpeed);

                    // Calculate the new velocity based on the grid's velocity and the clamped relative velocity.
                    Vector3 newVelocity = nearbyGridVelocity.Value + clampedRelativeVelocity;

                    // Apply the new velocity to the character's physics if it has been clamped.
                    if (newVelocity != currentVelocity)
                    {
                        physics.LinearVelocity = newVelocity;
                        // Optionally send a message to the client for a notification or other visual feedback.
                    }
                }
                else
                {
                    // If no nearby grid, just clamp the player's speed to the standard speed limit.
                    Vector3 clampedVelocity = ClampVelocityToLimit(currentVelocity, speedLimit);

                    // Apply the new velocity to the character's physics if it has been clamped.
                    if (clampedVelocity != currentVelocity)
                    {
                        physics.LinearVelocity = clampedVelocity;
                        // Optionally send a message to the client for a notification or other visual feedback.
                    }
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
