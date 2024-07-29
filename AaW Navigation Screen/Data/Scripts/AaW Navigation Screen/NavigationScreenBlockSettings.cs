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
    public class NavigationScreenBlockSettings
    {
        [ProtoMember(1)]
        public long NavigationScreenOffset;

        [ProtoMember(2)]
        public float NavigationScreenChevronScale;

        [ProtoMember(3)]
        public float NavigationScreenChevronStrength;

        [ProtoMember(4)]
        public Color NavigationScreenChevronColor;

        [ProtoMember(5)]
        public int NavigationScreenZoom;
    }
}
