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
using System.Xml.Linq;
using XebiaLabs.Deployit.Client.Http;

namespace XebiaLabs.Deployit.Client
{
    internal sealed class PackageService : ServiceBase, IPackageService
    {
        public PackageService(DeployitServer server)
            : base(server,"package")
        {
        }


        public string Upload(string packageFilePath, string packageName)
        {
            if (String.IsNullOrEmpty(packageFilePath))
                throw new ArgumentException("packageFilePath is null or empty.", "packageFilePath");
            if (String.IsNullOrEmpty(packageName))
                throw new ArgumentException("packageName is null or empty.", "packageName");

            var command = BuildCommand("upload/{0}", packageName);
            var postHttpResponseProvider = new PostHttpResponseProvider(new ImportFilePostInputContent(packageFilePath));
            var response = ExecuteHttp<XDocument, XmlHttpContent, string, StringHttpContent>(postHttpResponseProvider, command);
            return response.Root.Attribute("id").Value;
        }
    }
}
