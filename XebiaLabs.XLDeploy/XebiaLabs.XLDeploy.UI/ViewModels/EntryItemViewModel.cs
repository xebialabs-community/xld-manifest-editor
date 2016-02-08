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
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.UI.ViewModels
{
    public class EntryItemViewModel : TreeViewItemViewModel, IEditorViewModelProvider
	{
		private readonly ManifestEditorViewModel _editor;
        private readonly EntryEditorViewModel _entryEditor;
        private readonly Descriptor _descriptor;
        private readonly IEntryItemCollection _collection;

		public Entry Entry { get; private set; }

		public NotificationObject ItemEditor
		{
			get { return _entryEditor; }
		}

		public EntryItemViewModel(Entry entry, TreeViewItemViewModel parent, ManifestEditorViewModel editor, Descriptor descriptor)
			: base(parent)
		{
			if (entry == null)
				throw new ArgumentNullException("entry", "entry is null.");
			if (parent == null)
				throw new ArgumentNullException("parent", "parent is null.");
			if (editor == null)
				throw new ArgumentNullException("editor", "editor is null.");
			if (descriptor == null)
				throw new ArgumentNullException("descriptor", "descriptor is null.");


			_descriptor = descriptor;
			Entry = entry;
			_editor = editor;
			_entryEditor = new EntryEditorViewModel(entry, editor);
		    TreeItemLabel = entry.Name;
		    _entryEditor.PropertyChanged += (_, args) => { if (args.PropertyName == "Name") TreeItemLabel = entry.Name; };
			_collection = parent as IEntryItemCollection;
			if (_collection != null)
			{
				MenuItems = new List<MenuItemViewModel> { new MenuItemViewModel("Remove CI", new DelegateCommand(DoRemove), null) };
			}
			IsExpanded = true;
		}

		protected override void LoadChildren()
		{
		    (from property in _descriptor.Properties
                 where property.AsContainment
                 select new PropertyItemViewModel(property, this, _editor)
            )
            .ToList()
            .ForEach(Children.Add);
		}

        private void DoRemove()
        {
            var youSure = MessageBox.Show("Do you really want to remove [" + TreeItemLabel + "] from the manifest?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (youSure != MessageBoxResult.Yes) { return; }

            Parent.Children.Remove(this);
			_editor.SelectedTreeItem = Parent;
			_collection.RemoveEntryItem(this);
		}


	    public override bool? HasChildren
	    {
	        get { return true; }
	    }

	    public bool HasMenuItems
		{
			get { return MenuItems != null && MenuItems.Count > 0; }
		}

		public IEnumerable<string> SaveEntry()
		{
		    return _entryEditor.SaveEntry()
                .Concat(
                    from pivm in Children.OfType<PropertyItemViewModel>()
                    from errMsg in pivm.SaveEmbEntries()
                    select errMsg
                );
		}
	}
}
