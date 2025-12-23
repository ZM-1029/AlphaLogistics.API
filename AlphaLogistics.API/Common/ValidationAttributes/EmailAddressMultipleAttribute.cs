using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WALMS.API.Common.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class EmailAddressMultipleAttribute : DataTypeAttribute, IClientModelValidator
    {
        #region privates
        private readonly EmailAddressAttribute _emailAddressAttribute = new EmailAddressAttribute();
        private readonly string? _sparator;
        #endregion

        #region ctor
        public EmailAddressMultipleAttribute() : base(DataType.EmailAddress)
        {
        }
        public EmailAddressMultipleAttribute(string? Separator) : base(DataType.EmailAddress)
        {
            _sparator = Separator;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Checks if the value is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object? value)
        {
            var emailAddr = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(emailAddr)) return true;
            if (!string.IsNullOrEmpty(_sparator))
            {
                //lets test for mulitple email addresses
                var emailsAddress = emailAddr.Split(new[] { _sparator }, StringSplitOptions.RemoveEmptyEntries);
                return emailsAddress.All(t => _emailAddressAttribute.IsValid(t));
            }
            //lets test for mulitple email addresses
            var emails = emailAddr.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return emails.All(t => _emailAddressAttribute.IsValid(t));
        }
        #endregion

        public void AddValidation(ClientModelValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(ErrorMessage))
                ErrorMessage = "Invalid email addresses";
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-emailm", ErrorMessage);
            MergeAttribute(context.Attributes, "data-rule-emailm", "true");
            MergeAttribute(context.Attributes, "data-msg-emailm", ErrorMessage);
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
