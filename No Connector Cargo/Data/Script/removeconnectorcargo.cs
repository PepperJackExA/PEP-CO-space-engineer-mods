using ObjectBuilders.Definitions;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using IMyShipConnector = Sandbox.ModAPI.Ingame.IMyShipConnector;

namespace PEPCO.removecargo.No_Connector_Cargo.Data.Script
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_ShipConnector), false)]
    public class NoConnectorCargo : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase ob)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }
        public override void UpdateOnceBeforeFrame()
        {
            var Cargo = Entity as IMyShipConnector;
                Cargo.CustomName = "Test123";
        }
    }
}
