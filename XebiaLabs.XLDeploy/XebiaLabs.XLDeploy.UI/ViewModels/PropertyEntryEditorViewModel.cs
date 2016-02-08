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
using System.Collections.Generic;
using Microsoft.Practices.Prism.ViewModel;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.UI.ViewModels
{
	public abstract class PropertyEntryEditorViewModel : NotificationObject
	{
		protected ManifestEditorViewModel ManifestEditor { get; private set; }

		public DescriptorProperty PropertyDescriptor { get; private set; }
		public Entry Entry { get; private set; }

		protected PropertyEntryEditorViewModel(ManifestEditorViewModel manifestEditor, DescriptorProperty propertyDescriptor, Entry entry)
		{
			ManifestEditor = manifestEditor;
			PropertyDescriptor = propertyDescriptor;
			Entry = entry;
		}

		protected EntryProperty GetEntryProperty(bool createIfN = false)
		{
			var result = Entry.GetProperty(PropertyName);
			if (result == null && createIfN)
			{
				result = new EntryProperty(PropertyName);
				Entry.Properties.Add(result);
			}
			return result;
		}

		protected void RemoveEntryProperty()
		{
			Entry.RemoveProperty(PropertyName);
		}

		public abstract IEnumerable<string> SaveDataToEntryProperty();

	    protected IEnumerable<string> RequiredMsg
	    {
	        get { return new[] {PropertyName + " is required but has no value"}; }
	    }

        protected string PropertyName { get { return PropertyDescriptor.Name;  } }
	}
}
