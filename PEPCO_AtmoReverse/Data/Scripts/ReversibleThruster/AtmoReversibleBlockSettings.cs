using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPCO.AtmoReversible
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class AtmoReversibleBlockSettings
    {
        [ProtoMember(1)]
        public bool Thrust_ReverseToggle;

    }
}
