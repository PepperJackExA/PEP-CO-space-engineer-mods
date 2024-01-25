using ProtoBuf;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Lights;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace GeothermalWell
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CargoContainer), false, "GeothermalWellTip")]

    public class GeothermalWell : MyGameLogicComponent
    {

        public IMyCargoContainer GeothermalWellTip;
        public IMyInventory Inventory;
        public bool SetupDone = false;
        public float BaseGeneration = 1f;
        public MyDefinitionId HeatDefId;
        public MyObjectBuilder_InventoryItem HeatItem;
        public MyPlanet ClosestPlanet;

        //public double TotalMultiplier;
        public int EfficiencyReduction = 1;
        public double DistanceFromSurface = 0;
        public float HeatPerDecaTick = 0;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {

            base.Init(objectBuilder);

            try
            {

                NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;

                NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;

            }
            catch (Exception exc)
            {

            }

        }

        public override void UpdateBeforeSimulation100()
        {

            if (SetupDone == false)
            {

                GeothermalWellTip = Entity as IMyCargoContainer;
                Inventory = (VRage.Game.ModAPI.IMyInventory)GeothermalWellTip.GetInventory();
                HeatDefId = new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Heat");
                var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(HeatDefId);
                HeatItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };

                (Entity as IMyTerminalBlock).AppendingCustomInfo += CustomInfo;

                SetupDone = true;

            }

            try
            {

                var Grid = GeothermalWellTip.CubeGrid;

                if ((GeothermalWellTip.IsWorking) && (Grid.IsStatic))
                {

                    var WellHeadPosition = GeothermalWellTip.GetPosition();
                    //double DistanceFromSurface = 0;
                    double DistanceFromCenter = 0;
                    double SurfaceToCenter = 0;
                    //float EfficiencyReduction = 0;

                    ClosestPlanet = MyGamePruningStructure.GetClosestPlanet(WellHeadPosition);

                    DistanceFromSurface = Vector3D.Distance((ClosestPlanet.GetClosestSurfacePointGlobal(WellHeadPosition)), WellHeadPosition);

                    DistanceFromCenter = Vector3D.Distance((ClosestPlanet.PositionComp.GetPosition()), WellHeadPosition);

                    SurfaceToCenter = Vector3D.Distance((ClosestPlanet.GetClosestSurfacePointGlobal(WellHeadPosition)), (ClosestPlanet.PositionComp.GetPosition()));

                    if ((DistanceFromCenter < SurfaceToCenter) && (DistanceFromSurface > 150))
                    {

                        var EfficiencyEntityList = new List<MyEntity>();
                        var EfficiencySphere = new BoundingSphereD(WellHeadPosition, 250);
                        MyGamePruningStructure.GetAllEntitiesInSphere(ref EfficiencySphere, EfficiencyEntityList, MyEntityQueryType.Static);

                        //foreach (Entity in EfficiencyEntityList){

                        //if (Entity is IMyCubeGrid){


                        EfficiencyReduction = 0;
                        foreach (var Entity in EfficiencyEntityList)
                        {

                            if (Entity is IMyCubeBlock)
                            {

                                var Block = Entity as IMyCubeBlock;

                                if (Block.BlockDefinition.SubtypeId.ToString().Contains("GeothermalWellTip"))
                                {

                                    EfficiencyReduction++;

                                }

                            }

                        }

                        //TotalMultiplier = ((BaseGeneration * ((DistanceFromSurface - 150) / 100)) / EfficiencyReduction);
                        HeatPerDecaTick = (float)(BaseGeneration * ((BaseGeneration * ((DistanceFromSurface - 150) / 100)) / EfficiencyReduction));

                    }

                }

            }
            catch (Exception exc)
            {

            }

        }

        public override void UpdateBeforeSimulation10()
        {

            try
            {

                var Grid = GeothermalWellTip.CubeGrid;

                if ((GeothermalWellTip.IsWorking) && (Grid.IsStatic))
                {

                    //Inventory.AddItems((MyFixedPoint)BaseGeneration * (MyFixedPoint)TotalMultiplier, HeatItem.Content);
                    Inventory.AddItems((MyFixedPoint)HeatPerDecaTick, HeatItem.Content);

                }

            }
            catch (Exception exc)
            {

            }

        }

        void CustomInfo(IMyTerminalBlock block, StringBuilder info)
        {
            info.Append("Well depth: ").AppendFormat("{0:F1}", DistanceFromSurface).Append("m\n");
            if (EfficiencyReduction == 1)
            {

                info.Append("No nearby wells interfering\n");

            }
            else if (EfficiencyReduction == 2)
            {

                info.Append("1 nearby well interfering\n");

            }
            else
            {

                info.Append(EfficiencyReduction - 1).Append(" nearby wells interfering\n");

            }
            info.Append("Max gen: ").AppendFormat("{0:F2}", HeatPerDecaTick / (10f / 60)).Append(" heat/s\n");
        }
    }

}