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
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.UI.ViewModels
{
    public class ListOfCIViewModel : PropertyEntryEditorViewModel
    {
        private string _ciToAdd;
        private int _selectedCIIndex;

        public ListOfCIViewModel(ManifestEditorViewModel manifestEditor, DescriptorProperty propertyDescriptor, Entry entry)
            :base(manifestEditor, propertyDescriptor, entry)
        {
            var entryProperty = GetEntryProperty();
            var ciSet = (entryProperty == null ? new List<string>{ SetOfCIViewModel.NO_CIS } : entryProperty.GetListOfCI());

            CiRefs = new ObservableCollection<string>(ciSet);
            SelectedCIIndex = -1;
        }

        public ObservableCollection<string> CiRefs { get; private set; }

        public ICommand Remove
        {
            get
            {
                return new DelegateCommand<string>(ciRef =>
                {
                    CiRefs.Remove(ciRef);
                    if (CiRefs.Count == 0)
                    {
                        CiRefs.Add(SetOfCIViewModel.NO_CIS);
                    }
                });
            }
        }

        public ICommand Add
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    CiRefs.Add(CiToAdd.Trim());
                    CiRefs.Remove(SetOfCIViewModel.NO_CIS);
                    CiToAdd = "";
                });
            }
        }

        public string CiToAdd
        {
            get { return _ciToAdd; }
            set
            {
                _ciToAdd = value;
                RaisePropertyChanged("CiToAdd");
                RaisePropertyChanged("CanAdd");
            }
        }

        public bool CanAdd
        {
            get { return !string.IsNullOrWhiteSpace(CiToAdd); }
        }

        public int SelectedCIIndex
        {
            get { return _selectedCIIndex; }
            set
            {
                if (value == _selectedCIIndex) { return; }
                _selectedCIIndex = value;
                RaisePropertyChanged("SelectedCIIndex");
                RaisePropertyChanged("MoveUp");
                RaisePropertyChanged("MoveDown");
            }
        }

        public ICommand MoveUp
        {
            get { return new DelegateCommand(() => CiRefs.Move(SelectedCIIndex, SelectedCIIndex - 1), () => SelectedCIIndex > 0); }
        }

        public ICommand MoveDown
        {
            get { return new DelegateCommand(() => CiRefs.Move(SelectedCIIndex, SelectedCIIndex + 1), () => SelectedCIIndex > -1 && SelectedCIIndex < CiRefs.Count - 1); }
        }

        public override IEnumerable<string> SaveDataToEntryProperty()
        {
            var entry = GetEntryProperty(true);
            var hasNoCIs = CiRefs.Contains(SetOfCIViewModel.NO_CIS);
            if (hasNoCIs)
            {
                if (PropertyDescriptor.Required)
                {
                    return RequiredMsg;
                }
                RemoveEntryProperty();
            }
            else
            {
                entry.SetSetOfCi(CiRefs);
            }

            return new string[0];
        }
    }
}
