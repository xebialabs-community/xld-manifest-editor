/// Copyright (c) 2015, XebiaLabs B.V., All rights reserved.
///
///
/// The Manifest Editor for XL Deploy is licensed under the terms of the GPLv2
/// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most XebiaLabs Libraries.
/// There are special exceptions to the terms and conditions of the GPLv2 as it is applied to
/// this software, see the FLOSS License Exception
/// <https://github.com/jenkinsci/deployit-plugin/blob/master/LICENSE>.
///
/// This program is free software; you can redistribute it and/or modify it under the terms
/// of the GNU General Public License as published by the Free Software Foundation; version 2
/// of the License.
///
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
/// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
/// See the GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License along with this
/// program; if not, write to the Free Software Foundation, Inc., 51 Franklin St, Fifth
/// Floor, Boston, MA 02110-1301  USA
///
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Prism.ViewModel;
using XebiaLabs.Deployit.Client.Manifest;
using System.Windows;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.UI.ViewModels
{
    public class EntryEditorViewModel : NotificationObject, IDataErrorInfo
	{
		private readonly ManifestEditorViewModel _manifestEditor;
		public Descriptor Descriptor { get; private set; }
		internal Entry Entry { get; set; }

		public string Name
		{
			get { return Entry.Name; }
			set
			{
				if (value == Entry.Name)
				{
					return;
				}
				Entry.Name = value;
				RaisePropertyChanged(() => Name);
			}
		}

		public string Location
		{
            get { return IsArtifact ? Entry.Path : ""; }
			set
			{
			    if (!IsArtifact) { return; }
			    Entry.Path = value;
			    RaisePropertyChanged(() => Location);
			}
		}

		public Visibility LocationVisibility
		{
			get { return IsArtifact ? Visibility.Visible : Visibility.Hidden; }
		}

		public string Type
		{
			get { return Entry.Type; }
		}


		public bool IsEditable { get; set; }

		public List<PropertyEntryCategoryEditorViewModel> Categories { get; private set; }

        public PropertyEntryCategoryEditorViewModel CurrentViewedCategory { get; set; }


		private PropertyEntryEditorViewModel GetPropertyEditor(DescriptorProperty propertyDescriptor)
		{
			if (propertyDescriptor.Name == "placeholders")
			{
				return null;
			}

			if (propertyDescriptor.AsContainment)
			{
				return null;
			}
			if (propertyDescriptor.IsStringEnum)
			{
				return new EnumPropertyEntryEditorViewModel(_manifestEditor, propertyDescriptor, Entry);
			}
		    if (propertyDescriptor.IsString)
		    {
		        return new StringPropertyEntryEditorViewModel(_manifestEditor, propertyDescriptor, Entry);
		    }
		    if (propertyDescriptor.IsSetOrListOfString)
		    {
		        return new ListOrSetOfStringEditorViewModel(_manifestEditor, propertyDescriptor, Entry);
		    }
		    if (propertyDescriptor.IsBoolean)
		    {
		        return new BooleanPropertyEntryEditorViewModel(_manifestEditor, propertyDescriptor, Entry);
		    }
		    if (propertyDescriptor.IsMapStringString)
		    {
		        return new MapStringStringEditorViewModel(_manifestEditor, propertyDescriptor, Entry);
		    }
		    if (propertyDescriptor.IsInteger)
		    {
		        return new IntegerPropertyEntryEditorViewModel(_manifestEditor, propertyDescriptor, Entry);
		    }
		    if (propertyDescriptor.IsSetOfCi)
		    {
		        return new SetOfCIViewModel(_manifestEditor, propertyDescriptor, Entry);
		    }
		    if (propertyDescriptor.IsListOfCi)
		    {
		        return new ListOfCIViewModel(_manifestEditor, propertyDescriptor, Entry);
		    }
		    if (propertyDescriptor.IsCiReference)
		    {
		        return new CIReferenceViewModel(_manifestEditor, propertyDescriptor, Entry);
		    }

            throw new InvalidOperationException("Unhandled property type: " + propertyDescriptor.Kind);
		}

		/// <summary>
		/// Initializes a new instance of the EntryEditorViewModel class.
		/// </summary>
		public EntryEditorViewModel(Entry entry, ManifestEditorViewModel manifestEditor)
		{
			Entry = entry;
			_manifestEditor = manifestEditor;

		    Descriptor = _manifestEditor.GetDescriptor(entry.Type, true);
            if (Descriptor == null)
            {
                IsEditable = false;
				Categories = new List<PropertyEntryCategoryEditorViewModel>();
                return;
            }

			IsArtifact = Descriptor.Interfaces.Contains("udm.DeployableArtifact");

			var categoryQuery = from p in Descriptor.Properties
								where !p.Hidden
								orderby p.Category ascending
								let categoryName = p.Category ?? "Common"
								let propertyViewModel = GetPropertyEditor(p)
								where propertyViewModel != null
								group propertyViewModel by categoryName into category
								select new PropertyEntryCategoryEditorViewModel(category.Key, category);

			Categories = categoryQuery.ToList();
			CurrentViewedCategory = Categories[0];

			IsEditable = true;
		}

        public bool IsArtifact { get; private set; }

	    public IEnumerable<string> SaveEntry()
	    {
	        var name = Entry.Name;
	        if (string.IsNullOrWhiteSpace(name))
	        {
	            name = "untitled";
	            yield return "There is an untitled CI entry";
	        }
            if (IsArtifact && string.IsNullOrWhiteSpace(Location))
            {
                yield return string.Format("Entry [{0}] is an artifact but has no location set", name);
            }
	        var prefix = string.Format("In [{0}]: ", name);
	        foreach (var msg in (from catg in Categories
	                             from prop in catg.Properties
	                             from msg in (prop.SaveDataToEntryProperty() ?? new string[0])
	                             select prefix + msg))
	        {
	            yield return msg;
	        }
		}

        public string this[string columnName]
        {
            get
            {
                return (columnName == "Name" && string.IsNullOrWhiteSpace(Name))
                           ? "Name must not be empty"
                           : (columnName == "Location" && IsArtifact && string.IsNullOrWhiteSpace(Location))
                                 ? "Artifacts must have a Location"
                                 : null;
            }
        }

        public string Error { get { return this["Name"] ?? this["Location"]; } }
	}
}
