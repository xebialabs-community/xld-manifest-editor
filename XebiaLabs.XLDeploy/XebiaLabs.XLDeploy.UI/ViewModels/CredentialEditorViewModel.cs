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
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using XebiaLabs.Deployit.Client;
using XebiaLabs.Deployit.Client.Credentials;
using XebiaLabs.Deployit.UI.Validation;

namespace XebiaLabs.Deployit.UI.ViewModels
{
    public class CredentialEditorViewModel : ValidatableViewModel
    {
        public event EventHandler IsConnectedChanged;

        private readonly ICredentialManager _credentialManager;
        private readonly IDeployitServer _server;
        private bool _saveCredentials;
        private string _url;
        private string _username;

        /// <summary>
        ///     Initializes a new instance of the CredentialEditorViewModel class.
        /// </summary>
        public CredentialEditorViewModel(IDeployitServer server, ICredentialManager credentialManager, Uri uri = null)
        {
            if (server == null)
                throw new ArgumentNullException("server", "server is null.");
            if (credentialManager == null)
                throw new ArgumentNullException("credentialManager", "credentialManager is null.");

            _server = server;
            _credentialManager = credentialManager;

            ErrorMessage = _server.LastConnectionStatus == ConnectionStatus.Connected ? "Connected" : "Not connected";

            if (uri != null)
            {
                _url = uri.ToString();
            }
            CheckConnectionCommand = new DelegateCommand<PasswordBox>(DoCheckConnection);
        }

        public string URL
        {
            get { return _url; }
            set
            {
                if (_url == value)
                    return;
                _url = value;
                RaisePropertyChanged(() => URL);
            }
        }

        [Required]
        public string UserName
        {
            get { return _username; }
            set
            {
                if (_username == value)
                    return;
                _username = value;
                RaisePropertyChanged(() => UserName);
            }
        }


        public string ErrorMessage { get; private set; }

        public bool SaveCredentials
        {
            get { return _saveCredentials; }
            set
            {
                if (_saveCredentials == value)
                    return;
                _saveCredentials = value;
                RaisePropertyChanged(() => SaveCredentials);
            }
        }

        public bool IsCredentialManagerDefined
        {
            get { return _credentialManager != null; }
        }

        public bool IsConnected
        {
            get { return _server.LastConnectionStatus == ConnectionStatus.Connected; }
        }

        public ICommand CheckConnectionCommand { get; private set; }

        public string ValidateURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "URL is empty";
            }
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return "Invalid URL format";
            }

            if (!url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                return "URL must use http:// or https:// scheme";
            }

            return null;
        }

        private void DoCheckConnection(PasswordBox passwordBox)
        {
            _server.Disconnect();
            var serverUri = new Uri(_url);

            var credential = new NetworkCredential(UserName, passwordBox.Password);
            try
            {
                _server.Connect(serverUri, credential);
            }
            catch (DeployitServerConnectionException dsce)
            {
                ErrorMessage = dsce.Message;
            }
            RaisePropertyChanged(() => ErrorMessage);
            if (_credentialManager != null && _server.LastConnectionStatus == ConnectionStatus.Connected && SaveCredentials)
            {
                _credentialManager.Set(serverUri, credential);
            }
            RaisePropertyChanged(() => IsConnected);
            EventHandler handler = IsConnectedChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
