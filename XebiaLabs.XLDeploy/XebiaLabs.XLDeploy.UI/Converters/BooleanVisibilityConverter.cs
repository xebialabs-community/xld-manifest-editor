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
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace XebiaLabs.Deployit.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility), ParameterType = typeof(string))]
    public class BooleanVisibilityConverter : IValueConverter
    {
        public Visibility NotVisibleVisibility { get; set; }

        public BooleanVisibilityConverter()
        {
            NotVisibleVisibility = Visibility.Collapsed;
        }
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;

            if (string.Compare(parameter as string, "not", true, CultureInfo.InvariantCulture) == 0)
            {
                boolValue = !boolValue;
            }
            return boolValue ? Visibility.Visible : NotVisibleVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new ValueUnavailableException();
        }

        #endregion
    }

}
