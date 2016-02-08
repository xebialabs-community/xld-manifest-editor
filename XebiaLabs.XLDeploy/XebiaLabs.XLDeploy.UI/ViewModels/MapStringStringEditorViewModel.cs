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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.UI.ViewModels
{
	public class MapStringStringEditorViewModel : PropertyEntryEditorViewModel, IDataErrorInfo
	{
		public ObservableCollection<MapStringOfStringItemViewModel> Items { get; private set; }

		public MapStringStringEditorViewModel(ManifestEditorViewModel manifestEditor, DescriptorProperty propertyDescriptor, Entry entry)
			: base(manifestEditor, propertyDescriptor, entry)
		{
			Items = new ObservableCollection<MapStringOfStringItemViewModel>();
		}

        public override IEnumerable<string> SaveDataToEntryProperty()
		{
            if (PropertyDescriptor.Required && Items.Count == 0)
            {
                return RequiredMsg;
            }

			if (Items.Count == 0)
			{
				RemoveEntryProperty();
			    return new string[0];
			}

            var errors = new List<string>();
            var emptyKeysCount = Items.Count(vm => string.IsNullOrWhiteSpace(vm.Key));
            if (emptyKeysCount > 0) { errors.Add(PropertyName + " has " + emptyKeysCount + " empty keys"); }

            var keyCounts = from item in Items
                            select new {key = item.Key, count = Items.Count(vm => vm.Key == item.Key)};

            var duplicateKeys = keyCounts.Where(keyCount => keyCount.count != 1).ToList();
            duplicateKeys.ForEach(keyCount => errors.Add(
                    string.Format("{0} contains key {1} {2} times", PropertyName, keyCount.key, keyCount.count)));

            if (emptyKeysCount == 0 && duplicateKeys.Count == 0)
            {
                try
                {
                    var dict = Items.ToDictionary(_ => _.Key, _ => _.Value);
                    GetEntryProperty(true).SetMapOfStringValue(dict);
                }
                catch (Exception e)
                {
                    errors.Add(string.Format("While setting MAP_OF_STRING for {0}: {1}", PropertyName, e.Message));
                }
            }
            return errors;
		}

		#region IDataErrorInfo Members

		public string Error
		{
			get { return this["Items"]; }
		}

	    public string this[string columnName]
	    {
	        get
	        {
	            return columnName == "Items" && PropertyDescriptor.Required && Items.Count == 0
	                       ? Properties.Resources.PROPERTY_REQUIRED
	                       : null;
	        }
	    }

	    #endregion
	}
}
