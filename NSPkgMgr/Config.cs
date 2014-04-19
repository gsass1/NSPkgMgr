using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NSPkgMgr
{
    [Serializable]
    public class Config
    {
        [XmlAttribute]
        public String MirrorUrl { get; set; }

        [XmlAttribute]
        public String IncludePath { get; set; }

        [XmlAttribute]
        public String LibraryPath { get; set; }

        public Config()
        {
            MirrorUrl = "http://nukesoftware.de/packages.xml";
            IncludePath = "C:\\Include";
            LibraryPath = "C:\\Libraries";
        }
    }
}
