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
        public int mercatorMapChevronColorRed;

        [ProtoMember(15)]
        public int mercatorMapChevronColorGreen;

        [ProtoMember(25)]
        public int mercatorMapChevronColorBlue;

        [ProtoMember(35)]
        public Color mercatorMapChevronColor;


    }
}
