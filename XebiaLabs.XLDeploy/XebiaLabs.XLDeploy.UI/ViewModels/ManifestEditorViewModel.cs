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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.ViewModel;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.UI.ViewModels
{
    public class ManifestEditorViewModel : NotificationObject
    {
        public DeployitManifest Manifest { get; private set; }
        internal Dictionary<string, Descriptor> Descriptors { get; private set; }
        internal Dictionary<string, Descriptor> AllDescriptors { get; private set; }
        public List<Descriptor> AvailableDescriptors { get; private set; }

        internal IDeployitServer Server { get; private set; }

        public List<ManifestItemViewModel> TreeRoots { get; private set; }

        public ManifestItemViewModel RootItem
        {
            get { return TreeRoots[0]; }
        }

        private NotificationObject _currentItemEditor;

        public NotificationObject CurrentItemEditor
        {
            get { return _currentItemEditor; }
            set
            {
                if (_currentItemEditor == value)
                    return;
                _currentItemEditor = value;
                RaisePropertyChanged(() => CurrentItemEditor);
            }
        }

        private TreeViewItemViewModel _node;

        public TreeViewItemViewModel SelectedTreeItem
        {
            get { return _node; }
            set
            {
                if (value == null || _node == value)
                {
                    return;
                }

                _node = value;
                var prop = _node as IEditorViewModelProvider;

                if (prop != null)
                {
                    CurrentItemEditor = prop.ItemEditor;
                }
                RaisePropertyChanged(() => SelectedTreeItem);
                RaisePropertyChanged(() => IsATreeItemSelected);
            }
        }

        public bool IsATreeItemSelected
        {
            get { return SelectedTreeItem != null; }
        }

        public string ManifestAppName
        {
            get { return Manifest.ApplicationName; }
        }

        public string ManifestVersion
        {
            get { return Manifest.Version; }
        }


        //? <summary>
        //? Initializes a new instance of the ManifestEditorViewModel class.
        //? </summary>
        //? <param name="manifest"></param>
        //? <param name="server"></param>
        public ManifestEditorViewModel(DeployitManifest manifest, IDeployitServer server)
        {
            Manifest = manifest;
            Manifest.ApplicationNameChanged += (_, __) => RaisePropertyChanged(() => ManifestAppName);
            Manifest.VersionChanged += (_, __) => RaisePropertyChanged(() => ManifestVersion);
            Server = server;
            var descriptorsList = server.MetadataService.GetDescriptors();
            AllDescriptors = descriptorsList.ToDictionary(_ => _.Type);
            Descriptors = descriptorsList
                .Where(_ => _.Interfaces.Contains("udm.Deployable") && !_.IsVirtual)
                .ToDictionary(_ => _.Type);

            AvailableDescriptors = (
                from d in Descriptors.Values
                where !d.IsVirtual
                orderby d.Type
                select d
                ).ToList();

            TreeRoots = new List<ManifestItemViewModel> {new ManifestItemViewModel(this, manifest)};
        }

        public IEnumerable<string> SaveChangesToManifest()
        {
            return TreeRoots[0].SaveManifestTree();
        }

        public Descriptor GetDescriptor(string type, bool includeEmbeddeds = false)
        {
            return Descriptors.ContainsKey(type)
                ? Descriptors[type]
                : !includeEmbeddeds
                    ? null
                    : AllDescriptors.Values.FirstOrDefault(d =>
                        d.Type == type
                        && d.Interfaces.Contains("udm.EmbeddedDeployable")
                        && !d.IsVirtual);
        }
    }
}
