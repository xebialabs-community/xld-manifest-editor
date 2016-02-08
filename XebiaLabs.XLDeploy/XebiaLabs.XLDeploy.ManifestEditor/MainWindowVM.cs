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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using XebiaLabs.Deployit.Client;
using XebiaLabs.Deployit.Client.Credentials;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.Package;
using XebiaLabs.Deployit.ManifestEditor.Properties;
using XebiaLabs.Deployit.UI;
using XebiaLabs.Deployit.UI.ViewModels;

namespace XebiaLabs.Deployit.ManifestEditor
{
    public class MainWindowVM : NotificationObject
    {
        private readonly ICredentialManager _credentialManager;
        private readonly IDeployitServer _server;
        private ManifestEditorViewModel _manifest;
        private string _openedManifestFileName;

        public MainWindowVM(IDeployitServer server, ICredentialManager credentialManager)
        {
            if (credentialManager == null)
            {
                throw new ArgumentNullException("credentialManager", @"credentialManager is null.");
            }
            if (server == null)
            {
                throw new ArgumentNullException("server", @"server is null.");
            }

            _credentialManager = credentialManager;
            _server = server;
            _server.ConnectionChanged += (_, __) => ServerConnectionChanged();
            TryConnect();
            ConnectToDeployitCommand = new DelegateCommand(DoConnectToDeployit);
            NewManifestCommand = new DelegateCommand(DoNewManifest);
            OpenManifestCommand = new DelegateCommand(DoOpenManifest);
            SaveManifestCommand = new DelegateCommand(() => DoSaveManifest());
            UploadPackageCommand = new DelegateCommand(DoUploadPackage);
            QuitCommand = new DelegateCommand(() => DoQuit());
            Application.Current.MainWindow.Closing += DoQuit;

            if (_server.LastConnectionStatus != ConnectionStatus.Connected)
            {
                DoConnectToDeployit();
            }
            if (_server.LastConnectionStatus != ConnectionStatus.Connected)
            {
                Application.Current.MainWindow.Close();
            }
            else
            {
                DoNewManifest();
            }
        }


        public ICommand ConnectToDeployitCommand { get; private set; }
        public ICommand NewManifestCommand { get; private set; }
        public ICommand OpenManifestCommand { get; private set; }
        public ICommand SaveManifestCommand { get; private set; }
        public ICommand UploadPackageCommand { get; private set; }
        public ICommand QuitCommand { get; private set; }

        public Uri ConnectionUrl { get; set; }
        public DateTime ConnectionTime { get; set; }

        private void ServerConnectionChanged()
        {
            RaisePropertyChanged(() => Title);
            RaisePropertyChanged(() => ConnectionDateTime);
        }

        public ManifestEditorViewModel Manifest
        {
            get { return _manifest; }
            set
            {
                if (_manifest == value)
                {
                    return;
                }
                _manifest = value;
                RaisePropertyChanged(() => Manifest);
                RaisePropertyChanged(() => IsManifestLoaded);
                RaisePropertyChanged(() => ManifestEditorVisiblity);
                RaisePropertyChanged(() => IsEditMenuEnabled);
            }
        }

        public bool IsManifestLoaded
        {
            get { return Manifest != null; }
        }

        public Visibility ManifestEditorVisiblity
        {
            get { return Manifest != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool IsConnected
        {
            get { return _server.LastConnectionStatus == ConnectionStatus.Connected; }
        }


        public string URL
        {
            get
            {
                var uri = _server.URL;
                return uri == null ? null : uri.ToString();
            }
        }

        public DateTime ConnectionDateTime
        {
            get { return _server.ConnectionDateTime; }
        }

        public string UserName
        {
            get { return _server.UserName; }
        }

        private static Uri DefaultUri
        {
            get
            {
                var url = Settings.Default.DefaultDeployitServerURL;
                return Uri.IsWellFormedUriString(url, UriKind.Absolute) ? new Uri(url) : null;
            }
        }

        public string Title
        {
            get
            {
                var title = "XL Deploy manifest editor - ";
                if (!IsConnected)
                {
                    return title + "Not connected";
                }

                if (Manifest == null)
                {
                    return title + "No manifest";
                }

                title += Manifest.ManifestAppName + " " + Manifest.ManifestVersion;

                title += " - " + (string.IsNullOrEmpty(_openedManifestFileName) ? "New file" : _openedManifestFileName);
                return title;
            }
        }

        public bool IsEditMenuEnabled
        {
            get { return Manifest != null && Manifest.SelectedTreeItem != null; }
        }

        public List<MenuItemViewModel> EditMenuItems
        {
            get
            {
                var selectedTreeItem = Manifest.SelectedTreeItem;
                var menuItemLabel = selectedTreeItem == null ? "No CI selected" : selectedTreeItem.TreeItemLabel;
                var subMenus = selectedTreeItem == null ? null : selectedTreeItem.MenuItems;
                return new List<MenuItemViewModel> {new MenuItemViewModel(menuItemLabel, null, subMenus)};
            }
        }

        private void TryConnect()
        {
            var uri = DefaultUri;
            if (uri == null)
            {
                return;
            }

            var credentials = _credentialManager.Get(uri);
            if (credentials == null)
            {
                return;
            }

            try
            {
                _server.Connect(uri, credentials);
            }
            catch (DeployitServerConnectionException)
            {
                // do nothing with it
            }
        }

        private void DoConnectToDeployit()
        {
            new DialogWindow
            {
                ViewModel = new CredentialDialogViewModel(_server, _credentialManager, DefaultUri)
            }.ShowDialog();

            if (IsConnected)
            {
                Settings.Default.DefaultDeployitServerURL = _server.URL.ToString();
                Settings.Default.Save();
            }

            RaisePropertyChanged(() => IsConnected);
            RaisePropertyChanged(() => URL);
            RaisePropertyChanged(() => UserName);
        }

        private void DoOpenManifest()
        {
            string filename = FileDialogs.OpenManifestFileDialog("Select a manifest file");
            if (filename == null)
            {
                return;
            }

            using (var reader = new StreamReader(filename, DeployitManifest.Encoding))
            {
                DeployitManifest manifest = DeployitManifest.Load(reader);
                _openedManifestFileName = filename;
                SetManifest(manifest);
                RaisePropertyChanged(() => Title);
            }
        }


        private void SetManifest(DeployitManifest manifest)
        {
            Manifest = new ManifestEditorViewModel(manifest, _server);
            Manifest.PropertyChanged += ManifestOnPropertyChanged;
        }

        private void ManifestOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var changedProp = propertyChangedEventArgs.PropertyName;
            if (changedProp == "ManifestAppName" || changedProp == "ManifestVersion")
            {
                RaisePropertyChanged(() => Title);
            }
            if (changedProp == "SelectedTreeItem")
            {
                RaisePropertyChanged(() => EditMenuItems);
            }
        }

        private void DoNewManifest()
        {
            var manifest = new DeployitManifest("My application", "1.0");
            SetManifest(manifest);
            _openedManifestFileName = null;
            RaisePropertyChanged(() => Title);
        }


        private void SaveToFile(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(fileStream, DeployitManifest.Encoding))
                {
                    _manifest.Manifest.Save(writer);
                    writer.Flush();
                    writer.Close();
                }
            }
            _openedManifestFileName = filename;
            RaisePropertyChanged(() => Title);
        }

        private bool DoSaveManifest()
        {
            if (Manifest == null)
            {
                return true;
            }

            var errors = Manifest.SaveChangesToManifest().ToList();
            if (errors.Count != 0)
            {
                var answer = MessageBox.Show("Manifest contains the following errors:\n\n * "
                    + string.Join("\n * ", errors.Take(5))
                    + (errors.Count > 5 ? ("\n * (... " + (errors.Count - 5) + " more)\n") : "\n")
                    + "\n"
                    + "Are you sure you want to save? (You'll be on your own!)", "Fix errors first?",
                                             MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                if (answer == MessageBoxResult.No)
                {
                    return false;
                }
            }

            if (_openedManifestFileName != null)
            {
                SaveToFile(_openedManifestFileName);
                return true;
            }
            var filename = FileDialogs.SaveManifestFileDialog("Save manifest", "deployit-manifest.xml");
            if (filename == null)
            {
                return false;
            }
            SaveToFile(filename);
            return true;
        }

        private void DoUploadPackage()
        {
            if (Manifest == null)
            {
                return;
            }

            string packageRootFolder = FileDialogs.GetFolder("Package Root");
            if (packageRootFolder == null)
            {
                return;
            }

            string file = Path.GetTempFileName();
            file = Path.ChangeExtension(file, "dar");

            PackageBuilder.Build(Manifest.Manifest, packageRootFolder, file);
            _server.PackageService.Upload(file, Manifest.Manifest.ApplicationName + ".dar");
        }

        private void DoQuit(object sender = null, CancelEventArgs args = null)
        {
            if (args == null)
            {
                Application.Current.MainWindow.Close();
                return;
            }

            if (Manifest == null)
            {
                return;
            }

            bool saveSuccess = false;
            var wantSave = MessageBoxResult.Yes;
            while (wantSave == MessageBoxResult.Yes && !saveSuccess)
            {
                wantSave = MessageBox.Show("Do you want to save the manifest before quitting?", "Save manifest?",
                                           MessageBoxButton.YesNoCancel,
                                           MessageBoxImage.Question, MessageBoxResult.Yes);
                if (wantSave == MessageBoxResult.Yes)
                {
                    saveSuccess = DoSaveManifest();
                }
            }

            args.Cancel = (wantSave == MessageBoxResult.Cancel);
        }
    }
}
