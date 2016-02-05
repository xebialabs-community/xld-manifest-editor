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
using Microsoft.Practices.Prism.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using XebiaLabs.Deployit.Client.Manifest;
using XebiaLabs.Deployit.Client.UDM;

namespace XebiaLabs.Deployit.UI.ViewModels
{
    public class SetOfCIViewModel : PropertyEntryEditorViewModel
    {
        private string _ciToAdd;
        public const string NO_CIS = "(No CIs have been set)";

        public SetOfCIViewModel(ManifestEditorViewModel m, DescriptorProperty pd, Entry entry) : base(m, pd, entry)
        {
            var entryProperty = GetEntryProperty();
            var ciSet = (entryProperty == null ? new HashSet<string>() : entryProperty.GetSetOfCI());

            CiRefs = new ObservableCollection<string>(ciSet);
            if (CiRefs.Count == 0) { CiRefs.Add(NO_CIS); }
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
                            CiRefs.Add(NO_CIS);
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
                    CiRefs.Remove(NO_CIS);
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
            get { return !string.IsNullOrWhiteSpace(CiToAdd) && !CiRefs.Contains(CiToAdd.Trim()); }
        }


        public override IEnumerable<string> SaveDataToEntryProperty()
        {
            var entry = GetEntryProperty(true);
            var hasNoCIs = CiRefs.Contains(NO_CIS);
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
