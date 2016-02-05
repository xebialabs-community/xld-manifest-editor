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
using System.ComponentModel;
using Microsoft.Practices.Prism.ViewModel;
using XebiaLabs.Deployit.Client.Manifest;

namespace XebiaLabs.Deployit.UI.ViewModels
{
    public class ManifestEditorInfoViewModel : NotificationObject, IDataErrorInfo
    {
        private readonly DeployitManifest _manifest;

        public string ApplicationName
        {
            get { return _manifest.ApplicationName; }
            set
            {
                if (value == _manifest.ApplicationName)
                {
                    return;
                }
                _manifest.ApplicationName = value;
                RaisePropertyChanged(() => ApplicationName);
            }
        }

        public string Version
        {
            get { return _manifest.Version; }
            set
            {
                if (value == _manifest.Version)
                {
                    return;
                }
                _manifest.Version = value;
                RaisePropertyChanged(() => Version);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ManifestEditorViewModel class.
        /// </summary>
        /// <param name="manifest"></param>
        public ManifestEditorInfoViewModel(DeployitManifest manifest)
        {
            _manifest = manifest;
            _manifest.ApplicationNameChanged += (_, __) => RaisePropertyChanged(() => ApplicationName);
            _manifest.VersionChanged += (_, __) => RaisePropertyChanged(() => Version);
        }

        public string this[string columnName]
        {
            get
            {
                return (columnName == "ApplicationName" && string.IsNullOrWhiteSpace(ApplicationName))
                           ? "Application name is required"
                           : (columnName == "Version" && string.IsNullOrWhiteSpace(Version))
                                 ? "Application version is required"
                                 : null;
            }
        }

        public string Error { get { return this["ApplicationName"] ?? this["Version"]; } }
    }
}
