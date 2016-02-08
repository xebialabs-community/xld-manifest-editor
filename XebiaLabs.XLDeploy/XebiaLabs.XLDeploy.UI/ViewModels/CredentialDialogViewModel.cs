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
using Microsoft.Practices.Prism.ViewModel;
using XebiaLabs.Deployit.Client;
using XebiaLabs.Deployit.Client.Credentials;

namespace XebiaLabs.Deployit.UI.ViewModels
{
    public class CredentialDialogViewModel : NotificationObject, IDialogViewModel
    {
        public CredentialDialogViewModel(IDeployitServer server, ICredentialManager credentialManager = null, Uri uri = null)
        {
            Editor = new CredentialEditorViewModel(server, credentialManager, uri);
            Editor.IsConnectedChanged += OnIsConnectedChanged;
        }

        public CredentialEditorViewModel Editor { get; private set; }


        public string DialogTitle
        {
            get { return "DeployitServer credentials"; }
        }

        public object DialogIcon
        {
            get { return null; }
        }

        public IDialogWindow DialogWindow { get; set; }

        public bool CanClose()
        {
            return true;
        }

        private void OnIsConnectedChanged(object sender, EventArgs e)
        {
            if (!Editor.IsConnected || DialogWindow == null) return;
            DialogWindow.DialogResult = true;
            DialogWindow.Close();
        }
    }
}
