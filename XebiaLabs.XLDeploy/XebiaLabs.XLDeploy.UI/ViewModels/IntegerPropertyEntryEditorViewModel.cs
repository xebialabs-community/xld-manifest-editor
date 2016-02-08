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
using System.ComponentModel;
using System.Globalization;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;
using XebiaLabs.Deployit.UI.Properties;

namespace XebiaLabs.Deployit.UI.ViewModels
{
	public class IntegerPropertyEntryEditorViewModel : PropertyEntryEditorViewModel, IDataErrorInfo
	{
		private string _stringValue;

		public IntegerPropertyEntryEditorViewModel(ManifestEditorViewModel manifestEditor, DescriptorProperty propertyDescriptor, Entry entry)
			: base(manifestEditor, propertyDescriptor, entry)
		{
			var property = GetEntryProperty();
		    if (property == null) { return; }

		    var intValue = property.GetIntValue();
		    if (intValue.HasValue)
		    {
		        _stringValue = intValue.Value.ToString(CultureInfo.InvariantCulture);
		    }
		}

		private bool IsDataValid(out int integer, out string error)
		{
			if (string.IsNullOrWhiteSpace(_stringValue))
			{
				integer = 0;
			    error = PropertyDescriptor.Required ? Resources.PROPERTY_REQUIRED : null;
			    return !PropertyDescriptor.Required;
			}

		    var parseSuccess = int.TryParse(_stringValue, out integer);
		    error = parseSuccess ? null : Resources.PROPERTY_INTREQUIRED;

            return parseSuccess;
		}


		public string Value
		{
			get
			{
                if (_stringValue != null) { return _stringValue; }

                var entryProperty = GetEntryProperty();
			    if (entryProperty == null) { return null; }

                var intValue = entryProperty.GetIntValue();
			    if (intValue == null) { return null; }

			    _stringValue = intValue.Value.ToString(CultureInfo.InvariantCulture);

			    return _stringValue;
			}
		    set
			{
				_stringValue = value;
			}
		}

		public override IEnumerable<string> SaveDataToEntryProperty()
		{
			if (string.IsNullOrWhiteSpace(_stringValue))
			{
			    if (PropertyDescriptor.Required) { return RequiredMsg; }

			    RemoveEntryProperty();
			    return new string[0];
			}

            int intValue;
		    string errorMessage;
		    if (IsDataValid(out intValue, out errorMessage))
		    {
		        var entry = GetEntryProperty(true);
		        entry.SetIntValue(intValue);
		        return new string[0];
		    }
		    return new[] {PropertyName + " must be an integer"};
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
	            if (columnName != "Value") { return null; }

	            int value;
	            string message;
	            IsDataValid(out value, out message);
	            return message;
	        }
	    }

	    #endregion
	}
}
