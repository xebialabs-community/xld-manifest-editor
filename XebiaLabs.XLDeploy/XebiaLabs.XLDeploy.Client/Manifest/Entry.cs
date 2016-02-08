// Copyright (c) 2015, XebiaLabs B.V., All rights reserved.
//
//
// The Manifest Editor for XL Deploy is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most XebiaLabs Libraries.
// There are special exceptions to the terms and conditions of the GPLv2 as it is applied to
// this software, see the FLOSS License Exception
// <https://github.com/xebialabs-community/xld-manifest-editor/blob/master/LICENSE>.
//
// This program is free software; you can redistribute it and/or modify it under the terms
// of the GNU General Public License as published by the Free Software Foundation; version 2
// of the License.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this
// program; if not, write to the Free Software Foundation, Inc., 51 Franklin St, Fifth
// Floor, Boston, MA 02110-1301  USA
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace XebiaLabs.Deployit.Client.Manifest
{
    [DebuggerDisplay("'Name: {Name,nq}'")]
    public sealed class Entry
    {
        private static string ReplaceBackslashBySlash(string value)
        {
            return value.Replace('\\', '/');
        }

        public string Name { get; set; }
        public List<EntryProperty> Properties { get; private set; }
        public string Type { get; set; }

        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                if (value == null)
                {
                    _path = string.Empty;
                }
                _path = ReplaceBackslashBySlash(value);
            }
        }

        public EntryProperty GetProperty(string name)
        {
            return Properties.FirstOrDefault(_ => _.Name.Equals(name, StringComparison.InvariantCulture));
        }

        public void RemoveProperty(string name)
        {
            var entry = GetProperty(name);
            if (entry != null)
            {
                Properties.Remove(entry);
            }
        }

        public EntryProperty GetPropertyAndCreateIfN(string name)
        {
            var result = GetProperty(name);
            if (result == null)
            {
                result = new EntryProperty(name);
                Properties.Add(result);
            }
            return result;
        }


        public Entry()
        {
            Properties = new List<EntryProperty>();
        }

        internal void Check(List<Violation> violations)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                violations.Add(new Violation(ViolationLevel.Entry, this, "Name is empty"));
            }

            foreach (var property in Properties)
            {
                property.Check(violations);
            }
        }

        internal static Entry Load(XElement xmlElement)
        {
            var result = new Entry
                {
                    Type = xmlElement.Name.ToString(),
                    Name = xmlElement.Attribute("name").Value
                };

            var pathAttribute = xmlElement.Attribute("file");
            if (pathAttribute != null)
            {
                result.Path = pathAttribute.Value;
            }

            var p = from property in xmlElement.Elements()
                    select new EntryProperty(property);

            result.Properties.AddRange(p);

            return result;
        }

        internal XElement GetXmlData()
        {
            var result = new XElement(Type);

            if (!string.IsNullOrWhiteSpace(Name))
            {
                result.Add(new XAttribute("name", Name));
            }

            if (!string.IsNullOrWhiteSpace(Path))
            {
                result.Add(new XAttribute("file", Path));
            }

            var xmlProperties = Properties.Select(_ => _.GetXmlValue());
            result.Add(xmlProperties);

            return result;
        }
    }
}
