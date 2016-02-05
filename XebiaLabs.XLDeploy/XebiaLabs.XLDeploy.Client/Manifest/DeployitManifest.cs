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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace XebiaLabs.Deployit.Client.Manifest
{
	public sealed class DeployitManifest
    {
        public event EventHandler ApplicationNameChanged;
        public event EventHandler VersionChanged;

		public static readonly Encoding Encoding = Encoding.UTF8;
	    private string _applicationName;
	    private string _version;

	    public List<Entry> Entries { get; private set; }

		public string ApplicationName
		{
		    get { return _applicationName; }
		    set
		    {
                if (_applicationName == value) { return; }
                _applicationName = value;
		        var handler = ApplicationNameChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
		    }
		}

	    public string Version
	    {
	        get { return _version; }
	        set
	        {
	            if (value == _version) return;
                _version = value;
                var handler = VersionChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
	        }
	    }


		public DeployitManifest(string appName = null, string version = null, params Entry[] entries)
		{
            if (appName!=null) { ApplicationName = appName; }
            if (version!=null) { Version = version; }
			Entries = entries==null ? new List<Entry>() : new List<Entry>(entries);
		}


		public List<Violation> Check(bool ignoreApplicationMetadata)
		{
			var result = new List<Violation>();

			if (!ignoreApplicationMetadata)
			{
				if (string.IsNullOrWhiteSpace(ApplicationName))
				{
					result.Add(new Violation(ViolationLevel.Manifest, this, "Application name is missing"));
				}
				if (string.IsNullOrWhiteSpace(Version))
				{
					result.Add(new Violation(ViolationLevel.Manifest, this, "Application version is missing"));
				}
			}
			var q = from artifact in Entries
					let name = artifact.Name == null ? string.Empty : artifact.Name.Trim()
					group artifact by name into gp
					where gp.Count() > 1
					select gp;

			foreach (var gp in q)
			{
				var name = gp.Key;
				var violations = gp.Select(_ => new Violation(ViolationLevel.Manifest, _, string.Format("Duplicate artifact '{0}'", name)));
				result.AddRange(violations);
			}

			foreach (var entry in Entries)
			{
				entry.Check(result);
			}

			return result;
		}

		public static DeployitManifest Load(string filePath)
		{
			if (String.IsNullOrEmpty(filePath))
			{
				throw new ArgumentException("filePath is null or empty.", "filePath");
			}

			if (!File.Exists(filePath))
			{
				throw new ArgumentException(String.Format("file '{0}' does not exist", filePath));
			}

			using (var reader = new StreamReader(filePath, Encoding))
			{
				return Load(reader);
			}

		}

		public static DeployitManifest Load(TextReader inputReader)
		{
			if (inputReader == null)
				throw new ArgumentNullException("inputReader", "inputReader is null.");

			XDocument xmlData;
			try
			{
				xmlData = XDocument.Load(inputReader);
			}
			catch (Exception ex)
			{
				throw new ArgumentException("the manifest is not an well formated xml document", ex);
            }
		    return Load(xmlData);
		}

	    public static DeployitManifest FromString(string manifestText)
	    {
	        return Load(new StringReader(manifestText));
	    }

        public static DeployitManifest Load(XDocument xmlData)
        {
            var root = xmlData.Root;
            if (root == null)
            {
                throw new ArgumentException("Manifest has no root node");
            }
            if (root.Name != "udm.DeploymentPackage")
            {
                throw new ArgumentException("invalid root xml node");
            }

			var result = new DeployitManifest
			    {
			        ApplicationName = root.Attribute("application").ValueOrEmptyIfNull(),
			        Version = root.Attribute("version").ValueOrEmptyIfNull()
			    };

		    var deployables = root.Element("deployables");
		    if (deployables == null) {return result;}

		    foreach (var deployable in deployables.Elements())
		    {
		        result.Entries.Add(Entry.Load(deployable));
		    }

		    return result;
		}



		public void Save(TextWriter writer)
		{
			var manifestRoot = new XElement("udm.DeploymentPackage");
			var manifestDocument = new XDocument(manifestRoot);

			if (!string.IsNullOrWhiteSpace(ApplicationName))
			{
				manifestRoot.Add(new XAttribute("application", ApplicationName));
			}

			if (!string.IsNullOrWhiteSpace(Version))
			{
				manifestRoot.Add(new XAttribute("version", Version));
			}

			var deployables = new XElement("deployables");
			manifestRoot.Add(deployables);

			foreach (var entry in Entries)
			{
			    deployables.Add(entry.GetXmlData());
			}

			manifestDocument.Save(writer);

			writer.Flush();
		}

		static internal void SaveLine(TextWriter writer, string key, string value, bool allowEmtpyValue = false)
		{
			if (writer == null)
				throw new ArgumentNullException("writer", "writer is null.");
			if (String.IsNullOrEmpty(key))
				throw new ArgumentException("key is null or empty.", "key");
			if (String.IsNullOrEmpty(value) && !allowEmtpyValue)
				throw new ArgumentException("value is null or empty.", "value");

			writer.WriteLine("{0}: {1}", key, value);
		}
	}
}
