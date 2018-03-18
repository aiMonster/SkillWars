using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Attributes
{
    public class ChangePasswordAttribute : ValidationAttribute
    {
        private readonly string _passwordProperty;
        public ChangePasswordAttribute(string password)
        {
            _passwordProperty = password;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_passwordProperty);
            if (property == null)
            {
                return new ValidationResult(string.Format("Unknown property: {0}", _passwordProperty));
            }
            object password = property.GetValue(validationContext.ObjectInstance, null);
            if ((string)password == (string)value)
            {
                return new ValidationResult("New password cannot be the same as old");
            }
            return null;
        }
    }
}
