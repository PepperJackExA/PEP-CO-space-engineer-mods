using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PEPCOXMLConfigTesting
{
    [ProtoContract, XmlRoot("PepperConfig")]
    public class PepperConfigAPI
    {
        [ProtoMember(1)]
        public PepperConfig[] PepperConfigs;

        [ProtoMember(5)]
        public UzarConfig[] UzarConfigs;

    }
}
