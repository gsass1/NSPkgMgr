using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NSPkgMgr
{
    [Serializable]
    public class Package
    {
        [XmlAttribute]
        public string Dependencies { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Version { get; set; }

        [XmlAttribute]
        public string URL { get; set; }

        [XmlAttribute]
        public int Size { get; set; }

        [XmlAttribute]
        public string IncludeDir { get; set; }

        [XmlAttribute]
        public string LibraryDir { get; set; }

        [XmlAttribute]
        public string IncludeOutputDir { get; set; }
    }
}
