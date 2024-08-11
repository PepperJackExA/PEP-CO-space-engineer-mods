using ProtoBuf;
using Sandbox.ModAPI;


namespace PEPCO_Propulsion.Sync
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketBlockSettings : PacketBase
    {
        [ProtoMember(1)]
        public long EntityId;

        [ProtoMember(2)]
        public VectorThrustEnginesBlockSettings Settings;

        public PacketBlockSettings() { } // Empty constructor required for deserialization

        public void Send(long entityId, VectorThrustEnginesBlockSettings settings)
        {
            EntityId = entityId;
            Settings = settings;

            if (MyAPIGateway.Multiplayer.IsServer)
                Networking.RelayToClients(this);
            else
                Networking.SendToServer(this);
        }

        public override void Received(ref bool relay)
        {
            var block = MyAPIGateway.Entities.GetEntityById(this.EntityId) as IMyThrust;

            if (block == null)
                return;

            var logic = block.GameLogic?.GetAs<PEPCOWaterThrustBlock>();

            if (logic == null)
                return;

            logic.Settings.VectorThrust_Toggle = this.Settings.VectorThrust_Toggle;
            logic.Settings.VectorThrustReverse_Toggle = this.Settings.VectorThrustReverse_Toggle;
            logic.Settings.VectorThrust_Angle = this.Settings.VectorThrust_Angle;

            relay = true;
        }
    }
}

