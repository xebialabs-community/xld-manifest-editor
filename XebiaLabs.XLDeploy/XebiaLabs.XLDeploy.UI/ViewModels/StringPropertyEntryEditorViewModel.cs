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
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;
using System.ComponentModel;
using XebiaLabs.Deployit.UI.Properties;

namespace XebiaLabs.Deployit.UI.ViewModels
{
	public class StringPropertyEntryEditorViewModel : PropertyEntryEditorViewModel, IDataErrorInfo
	{
		public StringPropertyEntryEditorViewModel(ManifestEditorViewModel manifestEditor, DescriptorProperty propertyDescriptor, Entry entry)
			: base(manifestEditor, propertyDescriptor, entry)
		{

			var property = GetEntryProperty();

			if (property != null)
			{
				Value = property.GetStringValue() ?? string.Empty;
			}
			else
			{
				if (propertyDescriptor != null && propertyDescriptor.DefaultValue != null)
				{
					Value = propertyDescriptor.DefaultValue;
				}
			}
		}

		public string Value { get; set; }

        public override IEnumerable<string> SaveDataToEntryProperty()
		{
			if (PropertyDescriptor.Required && string.IsNullOrWhiteSpace(Value))
			{
			    return RequiredMsg;
			}

			if (string.IsNullOrWhiteSpace(Value))
			{
				RemoveEntryProperty();
				return new string[0];
			}

			var propertyEntry = GetEntryProperty(true);

			propertyEntry.SetStringValue(Value.Trim());
			return new string[0];
		}

		#region IDataErrorInfo Members

		public string Error
		{
			get { return this["Value"]; }
		}

		public string this[string columnName]
		{
			get
			{
				if (columnName == "Value" && PropertyDescriptor.Required && string.IsNullOrWhiteSpace("Value"))
				{
					return Resources.PROPERTY_REQUIRED;
				}

				return null;
			}
		}

		#endregion
	}
}
