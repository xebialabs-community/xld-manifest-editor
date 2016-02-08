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
using Microsoft.Practices.Prism.ViewModel;
using System.ComponentModel;

namespace XebiaLabs.Deployit.UI.ViewModels
{
	public class MapStringOfStringItemViewModel : NotificationObject, IDataErrorInfo
	{
		private string _value;
		private string _key;
		public string Key
		{
			get{return _key;}
			set
			{
				if (_key == value)
					return;
				_key = value;
				RaisePropertyChanged(() => Key);
			}
		}
		public string Value
		{
			get{return _value;}
			set
			{
				if (_value == value)
					return;
				_value = value;
				RaisePropertyChanged(() => Value);
			}
		}


		#region IDataErrorInfo Members

		public string Error
		{
			get { return this["Key"]; }
		}

		public string this[string columnName]
		{
			get
			{
			    return (columnName == "Key" && string.IsNullOrWhiteSpace(_key))
                    ? Properties.Resources.PROPERTY_REQUIRED
                    : null;
			}
		}

		#endregion
	}
}
