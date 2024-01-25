using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

[MyEntityComponentDescriptor(typeof(MyObjectBuilder_ShipWelder), false, "LargeShipHeavyWelder", "SmallShipHeavyWelder")]

public class HeavyShipWelder : HeavyShipTool
{
    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    {
        base.Init(objectBuilder);

        CanAffectOwnGrid = true;

        if (((IMyCubeBlock)Entity).CubeGrid.GridSizeEnum == MyCubeSize.Large)
        {
            ToolSphereOffset = 3.0f;
            ToolSphereRadius = 1.5f;
        }
        else
        {
            ToolSphereOffset = 2.0f;
            ToolSphereRadius = 1.0f;
        }

    }

    protected override void HeavyAction(float HeavyMultiplier, BoundingSphereD sphere, IMyCubeGrid targetGrid, IMyCubeBlock welderBlock)
    {
        var inventory = ((MyEntity)welderBlock).GetInventory();
        bool isHelping = ((IMyShipWelder)welderBlock).HelpOthers;
        var amount = MyAPIGateway.Session.WelderSpeedMultiplier * HeavyMultiplier;
        var blocks = targetGrid.GetBlocksInsideSphere(ref sphere);
        foreach (var block in blocks)
            block.IncreaseMountLevel(amount, welderBlock.OwnerId, inventory, 0.6f, isHelping);
    }
}

[MyEntityComponentDescriptor(typeof(MyObjectBuilder_ShipGrinder), false, "LargeShipHeavyGrinder", "SmallShipHeavyGrinder")]
public class HeavyShipGrinder : HeavyShipTool
{
    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    {
        base.Init(objectBuilder);

        CanAffectOwnGrid = false;

        if (((IMyCubeBlock)Entity).CubeGrid.GridSizeEnum == MyCubeSize.Large)
        {
            ToolSphereOffset = 3.0f;
            ToolSphereRadius = 1.5f;
        }
        else
        {
            ToolSphereOffset = 2.0f;
            ToolSphereRadius = 1.0f;
        }
    }

    protected override void HeavyAction(float HeavyMultiplier, BoundingSphereD sphere, IMyCubeGrid targetGrid, IMyCubeBlock thisBlock)
    {
        var inventory = ((MyEntity)thisBlock).GetInventory();
        var amount = MyAPIGateway.Session.GrinderSpeedMultiplier * HeavyMultiplier;
        var blocks = targetGrid.GetBlocksInsideSphere(ref sphere);
        foreach (var block in blocks)
            block.DecreaseMountLevel(amount, inventory);
    }
}


public abstract class HeavyShipTool : MyGameLogicComponent
{

    protected float ToolSphereOffset = 4f;
    protected float ToolSphereRadius = 1.6f;

    protected bool CanAffectOwnGrid = true;

    protected float HeavyMultiplier;

    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    {
        NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;

        HeavyMultiplier = 0.25f;

    }

    public override void UpdateBeforeSimulation10()
    {
        if (((Entity as IMyGunObject<MyToolBase>).IsShooting || (Entity as IMyFunctionalBlock).Enabled))
        {
            //List of grids that are in range of the grinder
            List<IMyEntity> potentialGrids;
            BoundingSphereD ToolSphere = new BoundingSphereD(Entity.GetPosition() + Entity.WorldMatrix.Forward * ToolSphereOffset, ToolSphereRadius);

            potentialGrids = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref ToolSphere);

            //If own grid is in the list, remove it.
            if (!CanAffectOwnGrid && potentialGrids.Contains(((IMyCubeBlock)Entity).CubeGrid))
                potentialGrids.Remove(((IMyCubeBlock)Entity).CubeGrid);

            foreach (IMyEntity e in potentialGrids)
                if (e is IMyCubeGrid && e.Physics != null)
                {

                    HeavyAction(HeavyMultiplier, ToolSphere, (IMyCubeGrid)e, (IMyCubeBlock)Entity);

                }
        }
    }

    protected abstract void HeavyAction(float HeavyMultiplier, BoundingSphereD sphere, IMyCubeGrid targetGrid, IMyCubeBlock thisBlock);
}


