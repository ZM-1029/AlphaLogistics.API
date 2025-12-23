using System.ComponentModel.DataAnnotations;

namespace WALMS.API.Common.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class NumberOnlyAttribute : RegularExpressionAttribute
    {
        public NumberOnlyAttribute() : base(@"^[0-9]+$")
        {
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
                ErrorMessage = ErrorMessage;
            else ErrorMessage = "Only numbers allowed";
        }
    }
}
