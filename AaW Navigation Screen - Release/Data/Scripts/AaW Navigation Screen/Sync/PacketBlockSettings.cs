using ProtoBuf;
using Sandbox.ModAPI;
using PEPCO;


namespace PEPCO.Sync
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketBlockSettings : PacketBase
    {
        [ProtoMember(1)]
        public long EntityId;

        [ProtoMember(2)]
        public NavigationScreenBlockSettings Settings;

        public PacketBlockSettings() { } // Empty constructor required for deserialization

        public void Send(long entityId, NavigationScreenBlockSettings settings)
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

            var logic = block.GameLogic?.GetAs<NavigationScreenLogic>();

            if (logic == null)
                return;

            logic.Settings.NavigationScreenOffset = this.Settings.NavigationScreenOffset;
            logic.Settings.NavigationScreenChevronScale = this.Settings.NavigationScreenChevronScale;
            logic.Settings.NavigationScreenChevronStrength = this.Settings.NavigationScreenChevronStrength;
            logic.Settings.NavigationScreenChevronColor = this.Settings.NavigationScreenChevronColor;
            logic.Settings.NavigationScreenZoom = this.Settings.NavigationScreenZoom;

            relay = true;
        }
    }
}

