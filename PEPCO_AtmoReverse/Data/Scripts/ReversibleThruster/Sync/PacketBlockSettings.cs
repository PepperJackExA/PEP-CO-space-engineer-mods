using PEPCO.AtmoReversible;
using ProtoBuf;
using Sandbox.ModAPI;

namespace PEPCO.AtmoReversible.Sync
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketBlockSettings : PacketBase
    {
        [ProtoMember(1)]
        public long EntityId;

        [ProtoMember(2)]
        public AtmoReversibleBlockSettings Settings;

        public PacketBlockSettings() { } // Empty constructor required for deserialization

        public void Send(long entityId, AtmoReversibleBlockSettings settings)
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

            var logic = block.GameLogic?.GetAs<ThrustBlock>();

            if (logic == null)
                return;

            logic.Settings.Thrust_ReverseToggle = this.Settings.Thrust_ReverseToggle;

            relay = true;
        }
    }
}
