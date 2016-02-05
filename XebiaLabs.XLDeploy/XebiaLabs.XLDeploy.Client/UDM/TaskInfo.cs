//? Copyright (c) 2015, XebiaLabs B.V., All rights reserved.
//?
//?
//? The Manifest Editor for XL Deploy is licensed under the terms of the GPLv2
//? <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most XebiaLabs Libraries.
//? There are special exceptions to the terms and conditions of the GPLv2 as it is applied to
//? this software, see the FLOSS License Exception
//? <https://github.com/jenkinsci/deployit-plugin/blob/master/LICENSE>.
//?
//? This program is free software; you can redistribute it and/or modify it under the terms
//? of the GNU General Public License as published by the Free Software Foundation; version 2
//? of the License.
//?
//? This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
//? without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//? See the GNU General Public License for more details.
//?
//? You should have received a copy of the GNU General Public License along with this
//? program; if not, write to the Free Software Foundation, Inc., 51 Franklin St, Fifth
//? Floor, Boston, MA 02110-1301  USA
//?
using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace XebiaLabs.Deployit.Client.UDM
{
    [Serializable]
    [XmlRoot("task")]
    public class TaskInfo: UdmBase
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("currentStep")]
        public int CurrentStep { get; set; }

        [XmlAttribute("totalSteps")]
        public int TotalSteps { get; set; }

        [XmlAttribute("failures")]
        public int Failures { get; set; }

        [XmlAttribute("state")]
        public string State { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("startDate")]
        public string StartDate { get; set; }

        [XmlElement("completionDate")]
        public string CompletionDate { get; set; }

        [XmlAnyElement("metadata")]
        public XmlElement Metadata { get; set; }

        [XmlArray("steps")]
        [XmlArrayItem("step")]
        public TaskStep[] Steps { get; set; }

        public string GetMetadata(string key)
        {
            if (Metadata == null)
                return null;

            var metadataNode = Metadata.ChildNodes.OfType<XmlNode>()
                .FirstOrDefault(_ => string.Compare(_.Name, key, true, CultureInfo.InvariantCulture) == 0);

            if (metadataNode == null)
                return null;

            var textxml = metadataNode.FirstChild as XmlText;
            return textxml == null ? null : textxml.Value;
        }
    }

}
