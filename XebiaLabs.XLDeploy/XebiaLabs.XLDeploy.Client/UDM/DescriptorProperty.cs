/// Copyright (c) 2015, XebiaLabs B.V., All rights reserved.
///
///
/// The Manifest Editor for XL Deploy is licensed under the terms of the GPLv2
/// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most XebiaLabs Libraries.
/// There are special exceptions to the terms and conditions of the GPLv2 as it is applied to
/// this software, see the FLOSS License Exception
/// <https://github.com/jenkinsci/deployit-plugin/blob/master/LICENSE>.
///
/// This program is free software; you can redistribute it and/or modify it under the terms
/// of the GNU General Public License as published by the Free Software Foundation; version 2
/// of the License.
///
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
/// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
/// See the GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License along with this
/// program; if not, write to the Free Software Foundation, Inc., 51 Franklin St, Fifth
/// Floor, Boston, MA 02110-1301  USA
///
using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;

namespace XebiaLabs.Deployit.Client.UDM
{
	[Serializable]
	[XmlRoot("property-descriptor")]
	[DebuggerDisplay("{Name}")]
	public class DescriptorProperty : UdmBase
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlAttribute("kind")]
		public string Kind { get; set; }

		[XmlAttribute("description")]
		public string Description { get; set; }

		[XmlAttribute("fqn")]
		public string FQN { get; set; }

		[XmlAttribute("label")]
		public string Label { get; set; }

		[XmlAttribute("category")]
		public string Category { get; set; }

		[XmlElement("referencedType", IsNullable = true)]
		public string ReferencedType { get; set; }

		[XmlAttribute("asContainment")]
		public string AsContainmentString { get; set; }

		public bool AsContainment
		{
			get{return string.Compare(AsContainmentString, "true", true, CultureInfo.InvariantCulture) == 0;}
		}


		[XmlAttribute("password")]
		public bool IsPassword { get; set; }

		[XmlAttribute("required")]
		public bool Required { get; set; }

		[XmlAttribute("default")]
		public string DefaultValue { get; set; }

		[XmlAttribute("hidden")]
		public bool Hidden { get; set; }



		public bool IsString
		{
            get { return Kind.Equals("STRING"); }
		}

		public bool IsStringEnum
		{
            get { return IsString && EnumValues != null && EnumValues.Length > 0; }
		}

        public bool IsBoolean
		{
            get { return Kind.Equals("BOOLEAN"); }
		}

		public bool IsMapStringString
		{
            get { return Kind.Equals("MAP_STRING_STRING"); }
		}

		public bool IsInteger
		{
            get { return Kind.Equals("INTEGER"); }
		}

		public bool IsSetOrListOfString
		{
            get { return IsSetOfString || IsListOfString; }
		}

		public bool IsListOfString
		{
            get { return Kind.Equals("LIST_OF_STRING"); }
		}

        public bool IsSetOfString
        {
            get { return Kind.Equals("SET_OF_STRING"); }
        }

        public bool IsSetOfCi
        {
            get { return Kind.Equals("SET_OF_CI"); }
        }

        public bool IsListOfCi
        {
            get { return Kind.Equals("LIST_OF_CI"); }
        }

        public bool IsCiReference
        {
            get { return Kind.Equals("CI"); }
        }


		[XmlArray("enumValues")]
		[XmlArrayItem("string")]
		public string[] EnumValues { get; set; }
	}

}
