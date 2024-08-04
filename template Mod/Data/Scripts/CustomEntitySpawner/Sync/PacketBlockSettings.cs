using ProtoBuf;
using PEPCO.iSurvival.CustomEntitySpawner;
using Sandbox.ModAPI;

namespace PEPCO.Sync
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketBlockSettings : PacketBase
    {

        public CustomEntitySpawnerSettings settings = new CustomEntitySpawnerSettings();
        [ProtoMember(2)]
        public long EntityId;

        [ProtoMember(3)]
        public BotSpawnerConfig Settings;

        public PacketBlockSettings() { } // Empty constructor required for deserialization

        public PacketBlockSettings(long entityId, BotSpawnerConfig settings)
        {
            EntityId = entityId;
            Settings = settings;
        }

        public override void Received(ref bool relay)
        {
            settings.UpdateBlockSettings(EntityId, Settings);
            relay = true;
        }

        public void Send()
        {
            //if (MyAPIGateway.Multiplayer.IsServer)
                Networking.RelayToClients(this);
            //else
                Networking.SendToServer(this);
        }
    }
}
