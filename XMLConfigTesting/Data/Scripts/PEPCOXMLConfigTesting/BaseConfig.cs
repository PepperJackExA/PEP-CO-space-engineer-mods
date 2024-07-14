using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Game.ModAPI;
using VRage.Game;

namespace PEPCOXMLConfigTesting
{
    [ProtoContract]
    public class BaseConfig
    {
        [XmlAttribute("TheString")]
        public string TheString = "(null)";
        [XmlAttribute("TheOtherString")]
        public string TheOtherString = "(null)";

        [ProtoIgnore, XmlIgnore]
        public MyDefinitionId DefinitionId;

        [ProtoIgnore, XmlIgnore]
        public IMyModContext ModContext;

        public virtual void Init(IMyModContext modContext)
        {
            ModContext = modContext;

            MyDefinitionId.TryParse(TheString + "/" + SubtypeId, out DefinitionId);
        }

        public BaseConfig(string typeId, string subtypeId)
        {
            TypeId = typeId;
            SubtypeId = subtypeId;
        }

        public BaseConfig() { }
    }
}
