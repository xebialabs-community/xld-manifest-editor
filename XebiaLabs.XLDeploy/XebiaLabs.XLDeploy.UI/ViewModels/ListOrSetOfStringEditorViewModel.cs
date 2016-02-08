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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;
using XebiaLabs.Deployit.UI.Properties;

namespace XebiaLabs.Deployit.UI.ViewModels
{
	public class ListOrSetOfStringEditorViewModel : PropertyEntryEditorViewModel, IDataErrorInfo
	{
		public ListOrSetOfStringEditorViewModel(ManifestEditorViewModel manifestEditor, DescriptorProperty propertyDescriptor, Entry entry)
			: base(manifestEditor, propertyDescriptor, entry)
		{
			var property = GetEntryProperty();
		    if (property == null) return;

		    var items = property.GetListofStringValue();
		    if (items.Count > 0)
		    {
		        Value = string.Join(Environment.NewLine, items);
		    }
		}


		public string Value { get; set; }


        public override IEnumerable<string> SaveDataToEntryProperty()
		{
			var lines = GetLinesFromValue();
			if (lines.Count == 0 && PropertyDescriptor.Required)
			{
			    return RequiredMsg;
			}

			if (lines.Count == 0)
			{
				RemoveEntryProperty();
				return new string[0];
			}

			if (PropertyDescriptor.IsSetOfString && ValidateLines(lines) != null)
			{
				return new[] {PropertyName + " must have unique values"};
			}

			var propertyEntry = GetEntryProperty(true);

			propertyEntry.SetListOfStringValue(lines);

            return new string[0];
		}


		private List<string> GetLinesFromValue()
		{
			if (string.IsNullOrWhiteSpace(Value))
			{
				return new List<string>();
			}

		    return (from line in Value.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
		            where !string.IsNullOrWhiteSpace(line)
		            select line.Trim())
                   .ToList();
		}

		private string ValidateLines(List<string> lines)
		{
			if (PropertyDescriptor.IsSetOrListOfString)
			{
				var count = lines.Count;
				var distinctCount = lines.Distinct().Count();

				if (count != distinctCount)
				{
					return Resources.PROPERTY_SETOFSTRING_UNIQUEVALUES;
				}
			}
			return null;
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
			    return (columnName == "Value" && PropertyDescriptor.Required && GetLinesFromValue().Count == 0)
                    ? Resources.PROPERTY_REQUIRED
                    : null;
			}
		}

		#endregion
	}
}
