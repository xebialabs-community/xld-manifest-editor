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
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.UI.ViewModels
{
	public class PropertyItemViewModel : TreeViewItemViewModel, IEntryItemCollection
	{
	    private readonly DescriptorProperty _descriptorProperty;
	    private readonly EntryProperty _entryProperty;
	    private readonly ManifestEditorViewModel _editor;

	    /// <summary>
	    /// Initializes a new instance of the PropertyItemViewModel class.
	    /// </summary>
	    public PropertyItemViewModel(DescriptorProperty descriptorProperty, EntryItemViewModel parent,
	        ManifestEditorViewModel editor)
	        : base(parent)
	    {
	        if (descriptorProperty == null)
	            throw new ArgumentNullException("descriptorProperty", "descriptorProperty is null.");
	        if (parent == null)
	            throw new ArgumentNullException("parent", "parent is null.");
	        if (editor == null)
	            throw new ArgumentNullException("editor", "editor is null.");

	        var referencedType = descriptorProperty.ReferencedType;
	        if (!descriptorProperty.AsContainment || string.IsNullOrWhiteSpace(referencedType))
	        {
	            throw new ArgumentException("Invalid descriptor property");
	        }

	        _descriptorProperty = descriptorProperty;
	        _editor = editor;
	        TreeItemLabel = descriptorProperty.Label;

	        var childDescriptors = new List<Descriptor>(
	            _editor.AllDescriptors.Values.Where(
	                d => !d.IsVirtual && (
	                    d.Type == referencedType
	                    || d.Supertypes.Contains(referencedType)
	                    || d.Interfaces.Contains(referencedType))));

	        _entryProperty = parent.Entry.GetPropertyAndCreateIfN(descriptorProperty.Name);

	        IsExpanded = true;

	        MenuItems = new List<MenuItemViewModel>
	        {
	            new MenuItemViewModel(Properties.Resources.EDITOR_NEW_CI, null, new List<MenuItemViewModel>(
	                from descriptor in childDescriptors
	                select new MenuItemViewModel(descriptor.Type, new DelegateCommand(() => DoAdd(descriptor)), null)
	                ))
	        };
	    }

	    public DescriptorProperty DescriptorProperty { get { return _descriptorProperty; } }

	    private void DoAdd(Descriptor descriptor)
	    {
	        var entry = new Entry {Type = descriptor.Type};
	        entry.Name = string.Format("New {0}", entry.Type);
	        var item = new EntryItemViewModel(entry, this, _editor, descriptor) {IsSelected = true};
	        Children.Add(item);
	        _editor.CurrentItemEditor = item.ItemEditor;
	    }

	    protected override void LoadChildren()
	    {
	        (from entry in _entryProperty.GetListOrSetOfEmbEntry()
	         select new EntryItemViewModel(entry, this, _editor, _editor.GetDescriptor(entry.Type, true))
	        ).ToList()
	         .ForEach(Children.Add);
	    }

	    public override bool? HasChildren
		{
			get {return true;}
		}

		public IEnumerable<string> SaveEmbEntries()
		{
			var items = Children.OfType<EntryItemViewModel>().ToList();
            var errors = items.SelectMany(eivm => eivm.SaveEntry()).ToList();
            if (errors.Count != 0)
            {
                return errors;
            }

			_entryProperty.SetListOrSetOfEmbCI(items.Select(_ => _.Entry));
			return new string[0];
		}
		#region IEntryItemCollection Members

		public void RemoveEntryItem(EntryItemViewModel item)
		{
			Children.Remove(item);
		}

		#endregion
	}
}
