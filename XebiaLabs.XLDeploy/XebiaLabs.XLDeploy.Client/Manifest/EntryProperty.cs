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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace XebiaLabs.Deployit.Client.Manifest
{
	[DebuggerDisplay("'{Name,nq}'")]
	public sealed class EntryProperty
	{
	    private readonly XElement _xmlData;

		public string Name { get; private set; }

		private void ClearNode()
		{
			_xmlData.RemoveAll();
			_xmlData.Value = string.Empty;
		}
		public string GetStringValue()
		{
			return _xmlData.Value;
		}

		public void SetStringValue(string value)
		{
			_xmlData.RemoveAll();
			_xmlData.Value = value ?? string.Empty;
		}

		public int? GetIntValue()
		{
		    return int.Parse(GetStringValue());
		}

		public void SetIntValue(int? value)
		{
			if (value == null)
			{
				ClearNode();
			}
			else
			{
				SetStringValue(value.ToString());
			}
		}

		public bool? GetBoolValue()
		{
			var stringValue = GetStringValue();

			if (string.IsNullOrWhiteSpace(stringValue))
			{
				return null;
			}

			if (string.Compare(stringValue, "true", StringComparison.InvariantCulture) == 0)
			{
				return true;
			}
			if (string.Compare(stringValue, "false", StringComparison.InvariantCulture) == 0)
			{
				return false;
			}
			throw new InvalidOperationException(String.Format("Invalid boolean value {0}", stringValue));
		}

		public void SetBoolValue(bool? value)
		{
			ClearNode();
			if (value.HasValue)
			{
				SetStringValue(value.Value ? "true" : "false");
			}
		}

		public string GetCIReferenceValue()
		{
			var xci = _xmlData.Element("ci");
			if (xci == null) { return null; }

			var xref = xci.Attribute("ref");
			return xref == null ? null : xref.Value;
		}

		public void SetCIReferenceValue(string value)
		{
			ClearNode();
			if (!string.IsNullOrWhiteSpace(value))
			{
				_xmlData.Add(new XElement("ci", new XAttribute("ref", value)));
			}
        }

        public HashSet<string> GetSetOfCI()
        {
            return new HashSet<string>(
                        from ci in _xmlData.Elements("ci")
                        let refAttr = ci.Attribute("ref")
                        where refAttr != null
                        select refAttr.Value
                   );
        }

        public void SetSetOfCi(IEnumerable<string> ciRefs)
        {
            ClearNode();
            foreach (var ciRef in new HashSet<string>(ciRefs))
            {
                _xmlData.Add(new XElement("ci", new XAttribute("ref", ciRef)));
            }
        }

        public List<string> GetListOfCI()
        {
            return (from ci in _xmlData.Elements("ci")
                   let refAttr = ci.Attribute("ref")
                   where refAttr != null
                   select refAttr.Value).ToList();
        }

        public void SetListOfCi(IEnumerable<string> ciRefs)
        {
            ClearNode();
            foreach (var ciRef in ciRefs)
            {
                _xmlData.Add(new XElement("ci", new XAttribute("ref", ciRef)));
            }
        }


		public List<Entry> GetListOrSetOfEmbEntry()
		{
			var q = from element in _xmlData.Elements()
					select Entry.Load(element);

			return q.ToList();
		}

		public void SetListOrSetOfEmbCI(IEnumerable<Entry> entries)
		{
			ClearNode();
			var nodes = entries.Select(_ => _.GetXmlData());
			_xmlData.Add(nodes);
		}

		public List<string> GetListofStringValue()
		{
			var q = from xval in _xmlData.Elements("value")
					select xval.Value;

			return q.ToList();
		}

		public void SetListOfStringValue(List<string> values)
		{
			ClearNode();
			if (values != null && values.Count > 0)
			{
				_xmlData.Add(values.Select(_ => new XElement("value", _)));
			}
		}

		public HashSet<string> GetSetOfStringValue()
		{

			var q = from xval in _xmlData.Elements("value")
					select xval.Value;

			return new HashSet<string>(q);
		}

		public void SetSetOfStringValue(HashSet<string> values)
		{
			ClearNode();
			if (values != null && values.Count > 0)
			{
				_xmlData.Add( values.Select(_ => new XElement("value", _)));
			}
		}

		public Dictionary<string, string> GetMapOfStringValue()
		{

			var q = from xkey in _xmlData.Elements("entry")
					let key = xkey.Attribute("key").Value
					let value = xkey.Value
					select new { key, value };

			return q.ToDictionary(_ => _.key, _ => _.value);
		}

		public void SetMapOfStringValue(Dictionary<string, string> values)
		{
			ClearNode();
			if (values != null && values.Count > 0)
			{
				_xmlData.Add( values.Select(_ => new XElement("entry", _.Value, new XAttribute("key", _.Key))));
			}
		}

		public string GetEnumValue()
		{
			return _xmlData.Value;
		}

		public void SetEnumValue(string value)
		{
			ClearNode();
			if (!string.IsNullOrWhiteSpace(value))
			{
				_xmlData.Value = value;
			}
		}

		public List<Entry> GetEntryListValue()
		{
			var q = from element in _xmlData.Elements()
					let entry = Entry.Load(element)
					select entry;

			return q.ToList();
		}

		public void SetEntryListValue(IEnumerable<Entry> entries)
		{
			ClearNode();

			var q = from entry in entries
					select entry.GetXmlData();

			_xmlData.Add(q);

		}


		/// <summary>
		/// Initializes a new instance of the ArtifactProperty class.
		/// </summary>
		///<param name="xmlNode">The parameter is copied </param>
		internal EntryProperty(XElement xmlNode)
		{
			if (xmlNode == null)
				throw new ArgumentNullException("xmlNode", "xmlNode is null.");
			_xmlData = new XElement(xmlNode);
			Name = _xmlData.Name.LocalName;
		}

		public EntryProperty(string name)
		{
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentException("name must not be empty.", "name");
			Name = name;
			_xmlData = new XElement(name);
		}

		internal void Check(List<Violation> violations)
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				violations.Add(new Violation(ViolationLevel.Property, this, "Name is empty"));
			}
		}

		internal XElement GetXmlValue()
		{
			return new XElement(_xmlData);
		}
	}
}
