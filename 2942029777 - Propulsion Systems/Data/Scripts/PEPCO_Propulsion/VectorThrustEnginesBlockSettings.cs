using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPCO_Propulsion
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class VectorThrustEnginesBlockSettings
    {
        [ProtoMember(1)]
        public bool VectorThrust_Toggle;

        [ProtoMember(2)]
        public bool VectorThrustReverse_Toggle;

        [ProtoMember(3)]
        public float VectorThrust_Angle;
    }
}
