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
using System.Globalization;
using System.Xml.Linq;
using XebiaLabs.Deployit.Client.Http;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.Client
{
	internal sealed class RepositoryService : ServiceBase, IRepositoryService
	{
		public RepositoryService(DeployitServer server)
			: base(server,"repository")
		{
		}

        public bool Exists(string id)
        {
            var command = BuildCommand("exists/{0}", id);
            var response = ExecuteHttp<UdmBoolean, UDMHttpContent<UdmBoolean>, string, StringHttpContent>(new GetHttpResponseProvider(), command);
            return response.AsBoolean();
		}

		public ConfigurationItemId[] Query(CIQueryParameters parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException("parameters", "parameters is null.");

			var parameterDictionary = new Dictionary<string, string>();

			if (!string.IsNullOrWhiteSpace(parameters.CIType))
			{
				parameterDictionary.Add("type", parameters.CIType);
			}

			if (!string.IsNullOrWhiteSpace(parameters.ParentId))
			{
				parameterDictionary.Add("parent", parameters.ParentId);
			}

			if (!string.IsNullOrWhiteSpace(parameters.Pattern))
			{
				parameterDictionary.Add("namePattern", parameters.Pattern);
			}

			parameterDictionary.Add("page", parameters.Page.ToString(CultureInfo.InvariantCulture));

			parameterDictionary.Add("resultsPerPage", "" + (parameters.ResultPerPage ?? -1));

			var command = BuildCommand("query", parameterDictionary);

            var response = ExecuteHttp<ConfigurationItemIdCollection, UDMHttpContent<ConfigurationItemIdCollection>, string, StringHttpContent>(
                new GetHttpResponseProvider(), command);
            return response.CIs;

		}

		public XElement Get(string id)
		{
		    var command = BuildCommand("ci/{0}", id);
		    var response = ExecuteHttp<XDocument, XmlHttpContent, string, StringHttpContent>(new GetHttpResponseProvider(), command);
            return response.Root;
		}
	}
}
