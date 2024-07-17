using ProtoBuf;
using Sandbox.ModAPI;
using SubmarineStuff;


namespace SubmarineStuff.Sync
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketBlockSettings : PacketBase
    {
        [ProtoMember(1)]
        public long EntityId;

        [ProtoMember(2)]
        public BallastTankBlockSettings Settings;

        public PacketBlockSettings() { } // Empty constructor required for deserialization

        public void Send(long entityId, BallastTankBlockSettings settings)
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
            var block = MyAPIGateway.Entities.GetEntityById(this.EntityId) as IMyTerminalBlock;

            if (block == null)
                return;

            var logic = block.GameLogic?.GetAs<BallastTankLogic>();

            if (logic == null)
                return;

            logic.Settings.ballastTank_Fill = this.Settings.ballastTank_Fill;

            relay = true;
        }
    }
}

