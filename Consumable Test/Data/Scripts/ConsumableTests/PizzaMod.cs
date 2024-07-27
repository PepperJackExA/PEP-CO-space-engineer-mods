using Digi;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;
using VRage.Game.Entity;
using VRage.Utils;
using VRage.Game;
using VRage.ModAPI;

namespace ConsumableTests
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class PizzaMod : MySessionComponentBase
    {

        List<IMyFloatingObject> floatingObjects;
        public override void LoadData()
        {

        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                Log.Info($"PizzaMod: { MyAPIGateway.Session?.Camera?.Position }", "PizzaMod: update", 10000);
                var collectPos = MyAPIGateway.Session.Camera.Position;
                var sphere = new BoundingSphereD(collectPos, 100 + 10);

                var entities = new List<MyEntity>();

                MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref sphere, entities, MyEntityQueryType.Dynamic);

                Log.Info($"PizzaMod: { entities.Count }", "PizzaMod: update", 10000);

                //Identify the pizza items
                entities.ForEach(x =>
                {
                    var myTest = x as IMyEntity;

                    //if (myTest != null && myTest)
                    //    Log.Info($"PizzaMod: Found entity {myTest.GetType().}", "PizzaMod: update", 10000);


                    //Log.Info($"PizzaMod: Found supbart {x.Components.Entity.TryGetSubpart("Lid", out lid)}", "PizzaMod: update", 10000);
                    //if (lid != null)
                    //{
                    //    Log.Info($"PizzaMod: Found lid {lid.Name}", "PizzaMod: update", 10000);
                    //    Matrix matrix;
                    //    matrix = lid.PositionComp.LocalMatrixRef;

                        //    matrix = matrix * Matrix.CreateFromAxisAngle(new Vector3D(0,-0.1,0), 1);

                        //    lid.PositionComp.SetLocalMatrix(ref matrix, null, true);
                        //}
                } );

                Log.Info($"PizzaMo: True pizza items - {entities.Count}", "PizzaMod: update", 10000);

            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
