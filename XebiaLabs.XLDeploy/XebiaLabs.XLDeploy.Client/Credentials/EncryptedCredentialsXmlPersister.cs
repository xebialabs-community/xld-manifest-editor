// Copyright (c) 2015, XebiaLabs B.V., All rights reserved.
//
//
// The Manifest Editor for XL Deploy is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most XebiaLabs Libraries.
// There are special exceptions to the terms and conditions of the GPLv2 as it is applied to
// this software, see the FLOSS License Exception
// <https://github.com/jenkinsci/deployit-plugin/blob/master/LICENSE>.
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
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace XebiaLabs.Deployit.Client.Credentials
{
    internal static class EncryptedCredentialsXmlPersister
    {
        public static NetworkCredential Get(Uri serverUri, string fileName)
        {
            if (serverUri == null)
            {
                throw new ArgumentNullException("serverUri", "Cannot get credentials for a null server URI.");
            }

            try
            {
                var document = GetCredentialDocument(fileName);
                var node = FindServerNode(document, serverUri);

                if (node == null)
                {
                    return null;
                }

                var username = node.Element("UserName").Value;
                var password = node.Element("Password").Value;
                return new NetworkCredential(username, password);
            }
            catch
            {
                return null;
            }
        }

        public static void Set(Uri uri, NetworkCredential credentials, string fileName)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri", "Cannot set credentials on a null URI.");
            }
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials", "Cannot set null credentials on a URI.");
            }

            var document = GetCredentialDocument(fileName) ?? new XDocument(new XElement("Servers"));
            if (document.Root == null)
            {
                document.Add(new XElement("Servers"));
            }

            var serverNode = FindServerNode(document, uri);
            if (serverNode == null)
            {
                serverNode = new XElement("Server", new XAttribute("url", uri.ToString()),
                    new XElement("UserName", credentials.UserName), new XElement("Password", credentials.Password));
                document.Root.Add(serverNode);
            }
            else
            {
                serverNode.Element("UserName").Value = credentials.UserName;
                serverNode.Element("Password").Value = credentials.Password;
            }

            SaveCredentialDocument(document, fileName);
        }

        private static void SaveCredentialDocument(XDocument document, string fileName)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document", "document is null.");
            }

            var data = Encoding.UTF8.GetBytes(document.ToString());
            var protectedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            var directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes(fileName, protectedData);
        }

        private static XDocument GetCredentialDocument(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            var data = File.ReadAllBytes(fileName);
            var unprotectedData = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            var xmlData = Encoding.UTF8.GetString(unprotectedData);
            return XDocument.Parse(xmlData);
        }

        private static XElement FindServerNode(XDocument document, Uri serverUri)
        {
            return (document == null || document.Root == null)
                ? null
                : (from serverNode in document.Root.Elements("Server")
                    let url = serverNode.Attribute("url").Value
                    let uri = new Uri(url)
                    where Uri.Compare(uri, serverUri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped,
                        StringComparison.InvariantCultureIgnoreCase) == 0
                    select serverNode
                    ).FirstOrDefault();
        }
    }
}
