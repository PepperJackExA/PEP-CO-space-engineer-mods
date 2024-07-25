using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace PEPCO
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class MercatorMapBlockSettings
    {
        [ProtoMember(1)]
        public long mercatorMapOffset;

        [ProtoMember(5)]
        public float mercatorMapChevronScale;

        [ProtoMember(15)]
        public float mercatorMapChevronStrength;

        [ProtoMember(25)]
        public Color mercatorMapChevronColor;
    }
}
