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
using System.ComponentModel.DataAnnotations;

namespace XebiaLabs.Deployit.UI.Validation
{
    internal class AttributeValidationItem : IValidationItem
    {
        private readonly ValidationAttribute _attribute;
        private readonly string _propertyName;

        /// <summary>
        /// Initializes a new instance of the AttributeValidationItem class.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="propertyName"></param>
        public AttributeValidationItem(ValidationAttribute attribute, string propertyName)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "attribute is null.");
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException("propertyName is null or empty.", "propertyName");
            _attribute = attribute;
            _propertyName = propertyName;
        }

        public string Validate(object obj, object propertyValue)
        {
            return !_attribute.IsValid(propertyValue)
                ? _attribute.FormatErrorMessage(_propertyName)
                : null;
        }
    }
}
