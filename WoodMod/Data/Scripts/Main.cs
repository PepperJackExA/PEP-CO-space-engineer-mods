namespace emeWood
{
	using System;
	using Sandbox.ModAPI;
	using VRage.Game;
	using VRage.Game.Components;
	using VRage.Game.Entity;
	using Sandbox.Game.Entities;
	using VRageMath;
	using VRage.ModAPI;
	using VRage.ObjectBuilders;



	[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
	public class Main : MySessionComponentBase
	{

		public static Random rand = new Random();
		
		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			if (MyAPIGateway.Session.IsServer )
			{
				MyEntities.OnEntityAdd += MyEntities_OnEntityAdd;
			}
		}

		private void MyEntities_OnEntityAdd(MyEntity obj)
		{
			
			String usestring = obj.ToString();
			
			
			if (usestring.StartsWith("MyDebrisTr")) //ee
			{

				double dropammount = 100;
				IMyEntity tree = obj as IMyEntity;
				String treetype = tree.Model.AssetName;
				
				//if (treetype.Contains("Medium"))
					//dropammount *= 0.9;
				//if (treetype.Contains("Dead"))
					//dropammount *= 0.9;
				//if (treetype.Contains("Desert"))
					//dropammount *= 0.9;
				//if (treetype.Contains("Pine"))
					//dropammount *= 1.1;
				//if (treetype.Contains("Snow"))
					//dropammount *= 2.0;

				VRage.MyFixedPoint amount = ((int)(dropammount / 5.0));

				int rnd = rand.Next(1, 5);

				MyObjectBuilder_Component Droplog = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>("WoodLog");
				rand.Next(1);
				//MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(amount, Droplog), pos+(upp+3)+(fww)+((rtt)), fww, upp);

				MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(amount, Droplog), tree.GetPosition()+(tree.WorldMatrix.Up*rnd), tree.WorldMatrix.Forward , tree.WorldMatrix.Up); 

			}

		}	

	}

}
