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
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.UI.ViewModels
{
    public class ManifestItemViewModel : TreeViewItemViewModel, IEditorViewModelProvider, IEntryItemCollection
    {
        private readonly DeployitManifest _manifest;
        private readonly ManifestEditorViewModel _editor;

        public NotificationObject ItemEditor { get; private set; }

        public ManifestItemViewModel(ManifestEditorViewModel editor, DeployitManifest manifest)
            : base(null)
        {
            if (manifest == null)
                throw new ArgumentNullException("manifest", "manifest is null.");
            if (editor == null)
                throw new ArgumentNullException("editor", "editor is null.");
            _manifest = manifest;
            TreeItemLabel = manifest.ApplicationName;
            _manifest.ApplicationNameChanged += (_, __) => OnApplicationNameChanged();
            _manifest.VersionChanged += (_, __) => OnApplicationVersionChanged();
            _editor = editor;
            IsExpanded = true;
            ItemEditor = new ManifestEditorInfoViewModel(manifest);
            BuildMenuItems();
        }

        public override bool? HasChildren
        {
            get { return true; }
        }

        private string GetTypePrefix(string ciType)
        {
            var pos = ciType.IndexOf('.');
            return pos < 0 ? string.Empty : ciType.Substring(0, pos);
        }

        private void BuildMenuItems()
        {
            Func<Descriptor, string> typeWithoutPrefix = d => d.Type.Substring(d.Type.IndexOf('.') + 1);
            Func<Descriptor, MenuItemViewModel> buildMenuItemFromDescriptor =
                d => new MenuItemViewModel(typeWithoutPrefix(d), new DelegateCommand(() => DoAddEntry(d)), null);

            var q = from d in _editor.Descriptors.Values
                    orderby d.Type
                    let prefixType = GetTypePrefix(d.Type)
                    group d by prefixType
                    into gp
                    select new MenuItemViewModel(gp.Key, null, gp.Select(buildMenuItemFromDescriptor).ToList());

            MenuItems = new List<MenuItemViewModel>
                {
                    new MenuItemViewModel(Properties.Resources.EDITOR_NEW_CI, null, q.ToList())
                };
        }

        private void DoAddEntry(Descriptor descriptor)
        {
            if (descriptor == null)
                return;

            var entry = new Entry {Type = descriptor.Type};

            entry.Name = string.Format("New {0}", entry.Type);
            _manifest.Entries.Add(entry);

            var item = new EntryItemViewModel(entry, this, _editor, descriptor) {IsSelected = true};
            Children.Add(item);
            _editor.CurrentItemEditor = item.ItemEditor;

        }

        public IEnumerable<string> SaveManifestTree()
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(ApplicationName))
            {
                result.Add("Application name must not be empty");
            }
            if (string.IsNullOrWhiteSpace(Version))
            {
                result.Add("Application version must not be empty");
            }
            var childErrors = from child in Children.Cast<EntryItemViewModel>()
                              from errMsg in child.SaveEntry()
                              select errMsg;
            result.AddRange(childErrors);
            return result;
        }

        protected override void LoadChildren()
        {
            base.LoadChildren();

            foreach (var entry in _manifest.Entries)
            {
                var item = new EntryItemViewModel(entry, this, _editor,
                                                  _editor.AvailableDescriptors.First(_ => _.Type == entry.Type));
                Children.Add(item);
            }
        }

        public string ApplicationName
        {
            get { return _manifest.ApplicationName; }
        }

        private void OnApplicationNameChanged()
        {
            TreeItemLabel = ItemTitle;
            RaisePropertyChanged(() => ApplicationName);
            RaisePropertyChanged(() => ItemTitle);
            RaisePropertyChanged(() => TreeItemLabel);
        }

        public string Version
        {
            get { return _manifest.Version; }
        }

        private void OnApplicationVersionChanged()
        {
            RaisePropertyChanged(() => Version);
            RaisePropertyChanged(() => ItemVersion);
        }

        public string ItemTitle
        {
            get { return string.IsNullOrWhiteSpace(ApplicationName) ? "Untitled application" : ApplicationName.Trim(); }
        }

        public string ItemVersion
        {
            get { return string.IsNullOrWhiteSpace(Version) ? "No version" : Version.Trim(); }
        }

        #region IEntryItemCollection Members

        public void RemoveEntryItem(EntryItemViewModel item)
        {
            _editor.Manifest.Entries.Remove(item.Entry);
        }

        #endregion
    }
}
