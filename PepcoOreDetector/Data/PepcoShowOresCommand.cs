using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.Voxels;
using VRageMath;
using VRageRender;

namespace YourName.ModName.PepcoOreDetector.Data
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class PepcoShowOresCommand : MySessionComponentBase
    {
        private const double MaxRadius = 500; // Limit the maximum radius to avoid crashes
        private List<Vector3D> orePositions = new List<Vector3D>();
        private BoundingBoxD scanArea;
        private bool drawScanArea = false;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
            }
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            base.UnloadData();
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            var player = MyAPIGateway.Session.Player;

            if (player == null || !MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
            {
                MyAPIGateway.Utilities.ShowMessage("Pepco", "Only admins can use this command.");
                return;
            }

            if (messageText.StartsWith("/pepco show ores", StringComparison.OrdinalIgnoreCase))
            {
                var parameters = messageText.Split(' ');
                double radius;
                if (parameters.Length == 4 && double.TryParse(parameters[3], out radius))
                {
                    if (radius > MaxRadius)
                    {
                        MyAPIGateway.Utilities.ShowMessage("Pepco", $"Radius too large. Maximum allowed radius is {MaxRadius} meters.");
                    }
                    else
                    {
                        ShowOresInRadius(radius);
                    }
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("Pepco", "Usage: /pepco show ores <radius>");
                }
                sendToOthers = false;
            }
        }

        private void ShowOresInRadius(double radius)
        {
            var player = MyAPIGateway.Session.Player;
            if (player == null)
            {
                MyAPIGateway.Utilities.ShowMessage("Pepco", "Player not found.");
                return;
            }

            Vector3D playerPosition = player.GetPosition();
            orePositions.Clear();

            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyVoxelBase);

            var oresFound = new Dictionary<string, double>();

            foreach (var entity in entities)
            {
                var voxel = entity as IMyVoxelBase;
                if (voxel != null && Vector3D.Distance(voxel.GetPosition(), playerPosition) <= radius)
                {
                    var oreDetector = new MyOreDetector();
                    oreDetector.DetectOre(voxel, ref oresFound, playerPosition, radius, orePositions);
                }
            }

            if (oresFound.Count > 0)
            {
                foreach (var ore in oresFound)
                {
                    MyAPIGateway.Utilities.ShowMessage("Pepco", $"{ore.Key}: {ore.Value:F2} meters");
                }
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage("Pepco", "No ores found in the specified radius.");
            }

            // Define the scan area for visualization
            scanArea = new BoundingBoxD(playerPosition - new Vector3D(radius), playerPosition + new Vector3D(radius));

            // Set the flag to draw the scan area and ore positions
            drawScanArea = true;
        }

        public override void UpdateAfterSimulation()
        {
            if (drawScanArea)
            {
                DrawScanArea();
            }
        }

        private void DrawScanArea()
        {
            var colorRed = Color.Red;
            var colorGreen = Color.Green;
            MySimpleObjectDraw.DrawTransparentBox(ref MatrixD.Identity, ref scanArea, ref colorRed, MySimpleObjectRasterizer.SolidAndWireframe, 1, 0.04f, null, null);

            foreach (var position in orePositions)
            {
                var matrix = MatrixD.CreateTranslation(position);
                MySimpleObjectDraw.DrawTransparentSphere(ref matrix, 0.5f, ref colorGreen, MySimpleObjectRasterizer.SolidAndWireframe, 20, null, null);
            }
        }

        private class MyOreDetector
        {
            public void DetectOre(IMyVoxelBase voxel, ref Dictionary<string, double> oresFound, Vector3D playerPosition, double radius, List<Vector3D> orePositions)
            {
                var voxelMap = voxel as MyVoxelBase;
                if (voxelMap == null) return;

                var storage = voxel.Storage;
                if (storage == null) return;

                Vector3D minWorldCoord = playerPosition - new Vector3D(radius);
                Vector3D maxWorldCoord = playerPosition + new Vector3D(radius);

                // Convert world coordinates to voxel coordinates manually
                Vector3I minVoxelCoord = Vector3I.Floor((minWorldCoord - voxel.PositionLeftBottomCorner) / 1.0);
                Vector3I maxVoxelCoord = Vector3I.Ceiling((maxWorldCoord - voxel.PositionLeftBottomCorner) / 1.0);

                var materialData = new MyStorageData();
                materialData.Resize(Vector3I.One);

                for (int x = minVoxelCoord.X; x <= maxVoxelCoord.X; x += 2) // Smaller increments for better scan
                {
                    for (int y = minVoxelCoord.Y; y <= maxVoxelCoord.Y; y += 2)
                    {
                        for (int z = minVoxelCoord.Z; z <= maxVoxelCoord.Z; z += 2)
                        {
                            var voxelCoord = new Vector3I(x, y, z);
                            storage.ReadRange(materialData, MyStorageDataTypeFlags.Material, 0, voxelCoord, voxelCoord);
                            byte materialIdx = materialData.Material(0);

                            var material = MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialIdx);
                            if (material != null && material.IsRare)
                            {
                                Vector3D worldPosition = voxel.PositionLeftBottomCorner + voxelCoord * 1.0; // Corrected to match VoxelCoord unit to World Position unit
                                var distance = Vector3D.Distance(worldPosition, playerPosition);
                                if (distance <= radius)
                                {
                                    orePositions.Add(worldPosition);
                                    if (!oresFound.ContainsKey(material.MinedOre))
                                    {
                                        oresFound[material.MinedOre] = distance;
                                    }
                                    else if (oresFound[material.MinedOre] > distance)
                                    {
                                        oresFound[material.MinedOre] = distance;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
