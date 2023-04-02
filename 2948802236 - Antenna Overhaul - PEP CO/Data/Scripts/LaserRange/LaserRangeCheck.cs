using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using Sandbox.Game.Entities;
using IMyLaserAntenna = Sandbox.ModAPI.Ingame.IMyLaserAntenna;

namespace LaserRange
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_LaserAntenna), false)]
    public class LaserRangeOverride : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase ob)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            var laser = Entity as IMyLaserAntenna;
            var maxRange = ((laser as MyCubeBlock).BlockDefinition as MyLaserAntennaDefinition).MaxRange;
            if (laser.Range > maxRange)
                laser.Range = maxRange;
        }

    }
}