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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using XebiaLabs.Deployit.Client.Http;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.Client
{
    public sealed class DeployitServer : IDeployitServer
    {
        public event EventHandler ConnectionChanged = (sender, args) => { };

        private readonly Lazy<IDeploymentService> _deploymentService;
        private readonly Lazy<IMetadataService> _metadataService;
        private readonly Lazy<IRepositoryService> _repositoryService;
        private readonly Lazy<IPackageService> _packageService;
        private readonly Lazy<ITaskService> _taskService;

        private NetworkCredential _credentials;
        private ConnectionStatus _lastConnectionStatus = ConnectionStatus.Disconnected;
        private DeployitServerConfig _configuration;

        public Uri URL { get; private set; }

        public DateTime ConnectionDateTime { get; private set; }

        public DeployitServerConfig Configuration
        {
            get { return _configuration; }
        }

        public string UserName
        {
            get { return _credentials != null ? _credentials.UserName : null; }
        }

        public ConnectionStatus LastConnectionStatus
        {
            get { return _lastConnectionStatus; }
            private set
            {
                lock (this)
                {
                    _lastConnectionStatus = value;

                    var handler = ConnectionChanged;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        ///     Initializes a new instance of the DeployitServer class.
        /// </summary>
        public DeployitServer(): this(new DeployitServerConfig())
        {
        }

        public DeployitServer(DeployitServerConfig configuration)
        {
            _metadataService = new Lazy<IMetadataService>(() => new MetadataService(this), true);
            _packageService = new Lazy<IPackageService>(() => new PackageService(this), true);
            _repositoryService = new Lazy<IRepositoryService>(() => new RepositoryService(this), true);
            _deploymentService = new Lazy<IDeploymentService>(() => new DeploymentService(this), true);
            _taskService = new Lazy<ITaskService>(() => new TaskService(this), true);
            _configuration = configuration;
        }

        public void Disconnect()
        {
            URL = null;
            _credentials = null;
            LastConnectionStatus = ConnectionStatus.Disconnected;
        }

        public ConnectionStatus Connect(Uri url, NetworkCredential credentials,
                                        bool checkConnection = true)
        {
            if (URL != null)
            {
                throw new InvalidOperationException("Already connected");
            }

            ConnectionDateTime = DateTime.Now;

            if (!checkConnection)
            {
                _credentials = credentials;
                URL = url;
                LastConnectionStatus = ConnectionStatus.Connected;
                return ConnectionStatus.Connected;
            }

            try
            {
                var task = ExecuteRequestAsyncCore<ServerInfo, UDMHttpContent<ServerInfo>>(
                        url, "/server/info", new GetHttpResponseProvider(), credentials, _configuration, CheckIfStatusCodeIsOK);
                try
                {
                    task.Wait();
                }
                catch (AggregateException ae)
                {
                    _credentials = null;
                    LastConnectionStatus = ConnectionStatus.Disconnected;
                    throw new DeployitServerConnectionException(ae);
                }

                var ret = task.Result;

                if (ret.StatusCode == HttpStatusCode.OK)
                {
                    URL = url;
                    _credentials = credentials;
                    LastConnectionStatus = ConnectionStatus.Connected;
                    return ConnectionStatus.Connected;
                }

                _credentials = null;

                if (ret.StatusCode == HttpStatusCode.Unauthorized)
                {
                    LastConnectionStatus = ConnectionStatus.Unauthorized;
                    throw new DeployitServerConnectionException("Invalid login/password provided");
                }

                LastConnectionStatus = ConnectionStatus.Disconnected;
                var errorMessage = ret.StatusCode == HttpStatusCode.Unused
                    ? "Cannot connect to the server"
                    : string.Format("error code {0}", ret.StatusCode);
                throw new DeployitServerConnectionException(errorMessage);
            }
            catch (WebException webEx)
            {
                LastConnectionStatus = ConnectionStatus.UnExpectedError;
                URL = null;
                _credentials = null;
                throw new DeployitServerConnectionException(webEx.Message);
            }
        }

        public IPackageService PackageService
        {
            get { return _packageService.Value; }
        }

        public IMetadataService MetadataService
        {
            get { return _metadataService.Value; }
        }

        public IRepositoryService RepositoryService
        {
            get { return _repositoryService.Value; }
        }

        public IDeploymentService DeploymentService
        {
            get { return _deploymentService.Value; }
        }

        public ITaskService TaskService
        {
            get { return _taskService.Value; }
        }

        private static HttpClient CreateHttpClient(ICredentials credentials, DeployitServerConfig configuration)
        {
            var requestHandler = new WebRequestHandler
            {
                Credentials = credentials,
                PreAuthenticate = true,
            };

            if (configuration.ReadWriteTimeout != 0)
                requestHandler.ReadWriteTimeout = configuration.ReadWriteTimeout*1000;

            var client = new HttpClient(requestHandler);

            if (configuration.ConnectionTimeout != 0)
                client.Timeout = TimeSpan.FromSeconds(configuration.ConnectionTimeout);

            return client;
        }

        private static string BuildUrl(Uri uri, string command)
        {
            return uri.ToString().TrimEnd('/') + "/deployit/" + command.TrimStart('/');
        }

        /// <summary>
        ///     Default status code handler
        /// </summary>
        internal static Func<HttpStatusCode, bool> CheckIfStatusCodeIsOK
        {
            get { return statusCode => statusCode == HttpStatusCode.OK; }
        }

        /// <summary>
        ///     Status code handler when no data is returned
        /// </summary>
        internal static Func<HttpStatusCode, bool> CheckIfStatusCodeIsOKorNotContent
        {
            get
            {
                return statusCode => statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.NoContent;
            }
        }


        /// <summary>
        ///     Generic method to handle GET request to deployit
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TOutputgen"></typeparam>
        /// <param name="command"></param>
        /// <param name="input"></param>
        /// <param name="isRequestSuccessullPredicate">if null, the method always try to generate an OUTPUT object from the request</param>
        /// <returns></returns>
        internal Task<ServerResponse<TOutput>> ExecuteRequestAsync<TOutput, TOutputgen>(
                string command, IHttpResponseProvider input, Func<HttpStatusCode, bool> isRequestSuccessullPredicate =null)
            where TOutputgen : IOutputHttpContent<TOutput>, new()
        {
            return ExecuteRequestAsyncCore<TOutput, TOutputgen>(URL, command, input, _credentials, _configuration, isRequestSuccessullPredicate);
        }


        private static Task<ServerResponse<TOutput>> ExecuteRequestAsyncCore<TOutput, TOutputgen>(Uri uri, string command, IHttpResponseProvider input, NetworkCredential credentials, DeployitServerConfig configuration, Func<HttpStatusCode, bool> isRequestSuccessful)
            where TOutputgen : IOutputHttpContent<TOutput>, new()
        {
            if (uri == null)
                throw new ArgumentNullException("uri", "uri is null.");
            if (String.IsNullOrEmpty(command))
                throw new ArgumentException("command is null or empty.", "command");

            var client = CreateHttpClient(credentials, configuration);
            var request = BuildUrl(uri, command);

            return input.GetTask(client, request).ContinueWith(requestTask =>
            {
                try
                {
                    var statusCode = requestTask.Result.StatusCode;
                    if (isRequestSuccessful != null && isRequestSuccessful(statusCode))
                    {
                        var output = new TOutputgen().Deserialize(requestTask.Result.Content);
                        return new ServerResponse<TOutput>(statusCode, output);
                    }
                    else
                    {
                        return new ServerResponse<TOutput>(statusCode, default(TOutput));
                    }
                }
                finally
                {
                    client.Dispose();
                }
            });
        }


        internal Task<ComplexServerResponse<TOutput, TError>> ExecuteRequestAsync<TOutput, TOutputgen, TError, TErrorgen>(
                string command, IHttpResponseProvider input, Func<HttpStatusCode, bool> isRequestSuccessullPredicate)
            where TOutputgen : IOutputHttpContent<TOutput>, new()
            where TErrorgen : IOutputHttpContent<TError>, new()
        {
            return ExecuteRequestAsyncCore<TOutput, TOutputgen, TError, TErrorgen>(
                URL, command, input, _credentials, _configuration, isRequestSuccessullPredicate
            );
        }


        private static Task<ComplexServerResponse<TOutput, TError>> ExecuteRequestAsyncCore<TOutput, TOutputgen, TError, TErrorgen>(
                Uri uri, string command, IHttpResponseProvider input, NetworkCredential credentials, DeployitServerConfig configuration,
                Func<HttpStatusCode, bool> isRequestSuccessful)
            where TOutputgen : IOutputHttpContent<TOutput>, new()
            where TErrorgen : IOutputHttpContent<TError>, new()
        {
            if (uri == null)
                throw new ArgumentNullException("uri", "uri is null.");
            if (String.IsNullOrEmpty(command))
                throw new ArgumentException("command is null or empty.", "command");

            var client = CreateHttpClient(credentials, configuration);
            var request = BuildUrl(uri, command);

            return input.GetTask(client, request).ContinueWith(requestTask =>
            {
                try
                {
                    var statusCode = requestTask.Result.StatusCode;
                    if (isRequestSuccessful != null && isRequestSuccessful(statusCode))
                    {
                        var output = new TOutputgen().Deserialize(requestTask.Result.Content);
                        return new ComplexServerResponse<TOutput, TError>(statusCode, true, output, default(TError));
                    }
                    else
                    {
                        var error = new TErrorgen().Deserialize(requestTask.Result.Content);
                        return new ComplexServerResponse<TOutput, TError>(statusCode, false, default(TOutput), error);
                    }
                }
                finally
                {
                    client.Dispose();
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
