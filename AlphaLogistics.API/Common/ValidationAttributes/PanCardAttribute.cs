using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WALMS.API.Common.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class PanCardAttribute : RegularExpressionAttribute, IClientModelValidator
    {
        public PanCardAttribute() : base(@"[A-Z]{5}[0-9]{4}[A-Z]{1}")
        {
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
                ErrorMessage = ErrorMessage;
            else ErrorMessage = "Invalid PAN Number";
        }
        public void AddValidation(ClientModelValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(ErrorMessage))
                ErrorMessage = "Invalid PAN Number";
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-regex", ErrorMessage);
            MergeAttribute(context.Attributes, "data-val-regex-pattern", Pattern);
            MergeAttribute(context.Attributes, "oninput", "changeToUpperCase(this);");
        }

        private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }
            attributes.Add(key, value);
            return true;
        }
    }
}
