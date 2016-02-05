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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Prism.ViewModel;

namespace XebiaLabs.Deployit.UI.Validation
{
    public class ValidatableViewModel : NotificationObject, IDataErrorInfo
    {
        private readonly Dictionary<PropertyInfo, List<IValidationItem>> _validationAttributeDictionary;


        public ValidatableViewModel()
        {
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            _validationAttributeDictionary = new Dictionary<PropertyInfo, List<IValidationItem>>();

            foreach (var property in props)
            {
                var validationItems = new List<IValidationItem>();
                var attributes =
                    property.GetCustomAttributes(typeof(ValidationAttribute), true)
                        .Cast<ValidationAttribute>()
                        .ToList();

                var propertyName = property.Name;
                if (attributes.Count > 0)
                {
                    validationItems.AddRange(
                        attributes.Select(
                            attribute => new AttributeValidationItem(attribute, propertyName) as IValidationItem));
                }

                var methodName = string.Format("Validate{0}", propertyName);
                var validationMethod = GetType()
                    .GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                        new[] {property.PropertyType}, new ParameterModifier[] {});
                if (validationMethod != null)
                {
                    validationItems.Add(new MethodValidationItem(validationMethod));
                }
                if (validationItems.Count > 0)
                {
                    _validationAttributeDictionary.Add(property, validationItems);
                }
            }
        }

        protected virtual List<string> ValidateCore()
        {
            return new List<string>();
        }

        private List<string> Validate(ValidatableViewModel vm)
        {
            if (vm == null)
            {
                throw new ArgumentNullException("vm");
            }

            var result = ValidateCore();
            result.AddRange(from item in _validationAttributeDictionary
                let value = item.Key.GetValue(vm, null)
                from validationItem in item.Value
                select validationItem.Validate(this, value)
                into error
                where !string.IsNullOrEmpty(error)
                select error);
            return result;
        }

        private List<string> Validate(ValidatableViewModel vm, string propertyName)
        {
            if (vm == null)
            {
                throw new ArgumentNullException("vm");
            }

            var result = new List<string>();

            List<IValidationItem> validators;
            var propertyInfo = GetType().GetProperty(propertyName);
            if (_validationAttributeDictionary.TryGetValue(propertyInfo, out validators))
            {
                var value = propertyInfo.GetValue(vm, null);
                result.AddRange(
                    from validationItem in validators
                    let error = validationItem.Validate(this, value)
                    where !string.IsNullOrEmpty(error)
                    select error);
            }
            RaisePropertyChanged(() => IsValid);
            return result;
        }

        public string Error
        {
            get
            {
                var errors = Validate(this);
                return errors.Count == 0
                    ? string.Empty
                    : string.Join(", ", errors.ToArray());
            }
        }

        public string this[string columnName]
        {
            get
            {
                var errors = Validate(this, columnName);
                return errors.Count == 0
                    ? string.Empty
                    : string.Join(", ", errors.ToArray());
            }
        }

        public bool IsValid
        {
            get { return string.IsNullOrEmpty(Error); }
        }
    }
}
