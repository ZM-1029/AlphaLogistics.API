using System.ComponentModel.DataAnnotations;

namespace WALMS.API.Common.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class AlphaNumericWithoutSpaceAttribute : RegularExpressionAttribute
    {
        public AlphaNumericWithoutSpaceAttribute() : base(@"^[A-Za-z0-9]+$")
        {
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
                ErrorMessage = ErrorMessage;
            else ErrorMessage = "Only alphanumeric [A-Z, a-z, 0-9] allowed";
        }
    }
}
