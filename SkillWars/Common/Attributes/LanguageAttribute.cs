using Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Attributes
{
    public class LanguageAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null)
            {
               Languages language = (Languages)value;
                if (Enum.IsDefined(typeof(Languages), language))
                    return true;
                else
                    this.ErrorMessage = "Such language is not found";
            }
            return false;
        }
    }
}
