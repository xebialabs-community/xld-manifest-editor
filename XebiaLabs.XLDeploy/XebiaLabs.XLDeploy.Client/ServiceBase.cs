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
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using XebiaLabs.Deployit.Client.Http;

namespace XebiaLabs.Deployit.Client
{
	internal class ServiceBase
	{
		private readonly string _commandPrefix;
		internal DeployitServer Server { get; private set; }

		/// <summary>
		/// Initializes a new instance of the ServiceBase class.
		/// </summary>
		internal ServiceBase(DeployitServer server, string commandPrefix)
		{
			if (server == null)
				throw new ArgumentNullException("server", "server is null.");
			if (String.IsNullOrEmpty(commandPrefix))
				throw new ArgumentException("commandPrefix is null or empty.", "commandPrefix");

			Server = server;
			_commandPrefix = commandPrefix;
		}

		protected string BuildCommand(string command, Dictionary<string, string> parameters, params object[] args)
		{
			var commandWithoutParameters = BuildCommand(command, args);

			if (parameters.Count == 0)
			{
				return commandWithoutParameters;
			}
			var sb = new StringBuilder(commandWithoutParameters);

			var i = 0;
			foreach (var param in parameters)
			{
				sb.Append(i == 0 ? "?" : "&");
				sb.AppendFormat("{0}={1}", HttpUtility.UrlEncode(param.Key), HttpUtility.UrlEncode(param.Value));
				i++;
			}

			return sb.ToString();
		}

		protected string BuildCommand(string command, params object[] args)
		{
			var encodedArgs = args.Select(arg => HttpUtility.UrlEncode(arg.ToString()).Replace("+", "%20")).ToArray();

			var partialCommandUrl = string.Format(command, encodedArgs);
			if (string.IsNullOrWhiteSpace(partialCommandUrl))
			{
				return string.Format("/{0}", _commandPrefix);
			}
			var fullCommand = string.Format("/{0}/{1}", _commandPrefix, partialCommandUrl);
			return fullCommand;
		}

        protected void EnsureServerResponseIsOK<T>(ServerResponse<T> response, bool allowNoContent = false)
        {
            if (response == null)
                throw new ArgumentNullException("response", "response is null.");

            if (allowNoContent && response.StatusCode == HttpStatusCode.NoContent)
            {
                return;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Error code " + response.StatusCode);
            }
        }

        protected void EnsureServerResponseIsOK<T, TError>(ComplexServerResponse<T, TError> response)
        {
            if (response == null)
                throw new ArgumentNullException("response", "response is null.");

            if (!response.IsSuccessfull)
            {
                throw new DeployitErrorException(response.StatusCode, Convert.ToString(response.Error));
            }
        }

        protected T ExecuteHttp<T, TContent, TError, TErrorContent>(IHttpResponseProvider http, String command, Func<HttpStatusCode, bool> isSuccessful = null)
            where TContent : IOutputHttpContent<T>, new()
            where TErrorContent : IOutputHttpContent<TError>, new()
        {
            using (var request = Server.ExecuteRequestAsync<T, TContent, TError, TErrorContent>(command, http, isSuccessful ?? DeployitServer.CheckIfStatusCodeIsOK))
            {
                request.Wait();
                var response = request.Result;
                EnsureServerResponseIsOK(response);
                return response.Data;
            }
        }
	}
}
