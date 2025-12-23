using System.ComponentModel.DataAnnotations;

namespace WALMS.API.Common
{
    public class GreaterThanZeroAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is int intValue && intValue <= 0)
            {
                return new ValidationResult($"{validationContext.DisplayName} must be greater than zero.");
            }
            return ValidationResult.Success;
        }
    }
}
