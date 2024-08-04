using PEPCO.iSurvival.CustomEntitySpawner;
using ProtoBuf;
using Sandbox.ModAPI;

namespace PEPCO.Sync
{
    [ProtoInclude(2, typeof(PacketBlockSettings))]
    [ProtoContract(UseProtoMembersOnly = true)]
    public abstract class PacketBase
    {
        [ProtoMember(1)]
        public readonly ulong SenderId;

        protected Networking Networking => CustomEntitySpawner.NetworkingInstance; // Use the singleton instance

        protected PacketBase()
        {
            SenderId = MyAPIGateway.Multiplayer.MyId;
        }

        public abstract void Received(ref bool relay);
    }
}
