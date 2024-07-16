using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarineStuff
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class SubmarineStuffBlockSettings
    {
        [ProtoMember(1)]
        public float ballastTank_Fill;

    }
}
