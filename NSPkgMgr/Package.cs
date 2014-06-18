/*
===============================================================================
    NSPkgMgr
    Copyright (C) 2014  Gian Sass

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
===============================================================================
 */

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
