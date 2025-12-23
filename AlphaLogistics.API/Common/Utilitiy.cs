using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
using System;
using System.Reflection;
using System.Security.Cryptography;

namespace Logistics
{
    public static class Utilitiy
    {

        public static TimeSpan ToTimeSpan(this TimeOnly time)
        {
            return new TimeSpan(time.Hour, time.Minute, time.Second);
        }    

        public static string ToTwoDecimal(this decimal? value)
        {
            if (!value.HasValue)
                return string.Empty;
            return string.Format("{0:#,0.00}", value.Value);
        }
        public static string ToTwoDecimal(this decimal value)
        {
            return string.Format("{0:#,0.00}", value);
        }
        public static string ToNoDecimal(this decimal? value)
        {
            if (!value.HasValue)
                return string.Empty;
            return string.Format("{0:#,0}", decimal.Round(value.Value, 0, MidpointRounding.ToEven));
        }
        public static string ToNoDecimal(this decimal value)
        {
            return string.Format("{0:#,0}", decimal.Round(value, 0, MidpointRounding.ToEven));
        }
        public static string ToMoney(this decimal? value, string? currencySymbol = null)
        {
            if (!value.HasValue)
                return string.Empty;
            return string.Format("{1}{0:#,0.00}", value.Value, GetMoneySymbol(currencySymbol));
        }
        public static string ToMoney(this decimal value, string? currencySymbol = null)
        {
            return string.Format("{1}{0:#,0.00}", value, GetMoneySymbol(currencySymbol));
        }
        public static string ToMoneyNoDecimal(this decimal? value, string? currencySymbol = null)
        {
            if (!value.HasValue)
                return string.Empty;
            return string.Format("{1}{0:#,0}", decimal.Round(value.Value, 0, MidpointRounding.ToEven), GetMoneySymbol(currencySymbol));
        }
        public static string ToMoneyNoDecimal(this decimal value, string? currencySymbol = null)
        {
            return string.Format("{1}{0:#,0}", decimal.Round(value, 0, MidpointRounding.ToEven), GetMoneySymbol(currencySymbol));
        }
        public static string ToMoney4(this decimal? value, string? currencySymbol = null)
        {
            if (!value.HasValue)
                return string.Empty;
            return string.Format("{1}{0:#,0.0000}", value.Value, GetMoneySymbol(currencySymbol));
        }
        public static string ToMoney4(this decimal value, string? currencySymbol = null)
        {
            return string.Format("{1}{0:#,0.0000}", value, GetMoneySymbol(currencySymbol));
        }

        public static string ToMoneyUnicode(this decimal? value, string? currencySymbol = null)
        {
            if (!value.HasValue)
                return string.Empty;
            return string.Format("{1}{0:#,0.00}", value.Value, GetMoneySymbolUnicode(currencySymbol));
        }
        public static string ToMoneyUnicode(this decimal value, string? currencySymbol = null)
        {
            return string.Format("{1}{0:#,0.00}", value, GetMoneySymbolUnicode(currencySymbol));
        }
        public static string ToMoneyUnicodeNoDecimal(this decimal? value, string? currencySymbol = null)
        {
            if (!value.HasValue)
                return string.Empty;
            return string.Format("{1}{0:#,0}", decimal.Round(value.Value, 0, MidpointRounding.ToEven), GetMoneySymbolUnicode(currencySymbol));
        }
        public static string ToMoneyUnicodeNoDecimal(this decimal value, string? currencySymbol = null)
        {
            return string.Format("{1}{0:#,0}", decimal.Round(value, 0, MidpointRounding.ToEven), GetMoneySymbolUnicode(currencySymbol));
        }
        public static string GetMoneySymbol(string? currencySymbol = null)
        {
            return string.Format("{0}{1}", !string.IsNullOrWhiteSpace(currencySymbol) ? currencySymbol : "₹", "&nbsp;");
        }
        public static string GetMoneySymbolUnicode(string? currencySymbol = null)
        {
            return string.Format("{0}{1}", !string.IsNullOrWhiteSpace(currencySymbol) ? currencySymbol : "&#8377;", "&#x0020;");
        }
        public static string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }
		public static DateTime GetDeliveryDateWithTimestampAsync(DateTime? originalDate, string postalCode)
		{
			// Check if originalDate has a value
			if (!originalDate.HasValue)
			{
				throw new ArgumentNullException(nameof(originalDate), "Original date cannot be null.");
			}
	
			// Check for India
			if (IsIndianPostalCode(postalCode))
			{
				return originalDate.Value.Date.AddHours(10); // For India, set the time to 10:00 AM
			}

			// Check for UK
			if (IsUKPostalCode(postalCode))
			{
				return originalDate.Value.Date.AddHours(8); // For UK, set the time to 8:00 AM
			}

			// Default for foreign or other countries
			return originalDate.Value.Date.AddHours(9); // Default timestamp
		}
		public static bool IsIndianPostalCode(string postalCode)
		{
			// Example rule: Indian postal codes are numeric and 6 digits long (e.g., 110001)
			return postalCode.Length == 6 && int.TryParse(postalCode, out int _);
		}

		public static bool IsUKPostalCode(string postalCode)
		{
			// UK postal codes can follow several patterns like:
			// A9 9AA, A99 9AA, AA9 9AA, AA99 9AA, A9A 9AA, AA9A 9AA, and others
			var ukPattern = @"^[A-Z]{1,2}[0-9R][0-9A-Z]? [0-9][ABD-HJLNP-UW-Z]{2}$";
			return System.Text.RegularExpressions.Regex.IsMatch(postalCode, ukPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
		}		

        public static string RandomTicketNumberGenerator()
        {
			var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
			var randomNumber = new Random().Next(0, 99999);
			var ticketNumber = $"TK-{timestamp.Substring(timestamp.Length - 3)}{randomNumber:D3}";
			return ticketNumber;
		}	

		public static  string GenerateOTP()
		{
			Random random = new Random();
			return random.Next(100000, 999999).ToString(); // Generate a random 6-digit number
		}

		public static string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];  // Prepare a buffer to hold the random bytes.
			using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomNumber);  // Fill the buffer with cryptographically strong random bytes.
				return Convert.ToBase64String(randomNumber);  // Convert the bytes to a Base64 string and return.
			}
		}

        public static string GenerateConsignmentNumber(string? state=null)
        {         
            string prefix = string.IsNullOrWhiteSpace(state)
                ? ((char)new Random().Next('A', 'Z' + 1)).ToString()
                : state.Substring(0, 2).ToUpper();
           
            int randomNumber = new Random().Next(10000, 99999);
      
            var consignmentNo = $"{prefix}{randomNumber}";

            return consignmentNo;
        }
        public static string GenerateSerialNumber(string orderno)//string productCode,string? customer = null
        {
            string prefix1 = string.IsNullOrWhiteSpace(orderno)
                ? ((char)new Random().Next('A', 'Z' + 1)).ToString()
                : orderno.Substring(0, 1).ToUpper();

            string prefix2 = orderno.Substring(0, 1).ToUpper();
            int randomNumber = new Random().Next(100000, 999999);

            var consignmentNo = $"{prefix1}{prefix2}-{randomNumber}";

            return consignmentNo;
        }

        public static string GenerateCode(int digits)
        {
            int maxExclusive = (int)Math.Pow(10, digits);
            int value = RandomNumberGenerator.GetInt32(0, maxExclusive);
            return $"EM-{value.ToString($"D{digits}")}";
        }
        public static string NormalizeWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return string.Empty;

            // Convert to lowercase
            word = word.ToLowerInvariant();

            // Remove punctuation (like ', " etc.)
            word = new string(word.Where(c => !char.IsPunctuation(c)).ToArray());

            // Remove extra spaces
            word = word.Trim();

            return word;
        }
        public static bool IsSimilarToAny(string? inputWord, string? existingWord)
        {
            if (string.IsNullOrWhiteSpace(inputWord) || string.IsNullOrWhiteSpace(existingWord))
                return false;

            inputWord = NormalizeWord(inputWord);
            existingWord = NormalizeWord(existingWord);

            // Direct match
            if (inputWord == existingWord)
                return true;

            // Plurals
            if (inputWord == existingWord + "s" || inputWord == existingWord + "es")
                return true;

            // Possessives
            if (inputWord == existingWord + "'s")
                return true;

            // Substring
            if (inputWord.Contains(existingWord, StringComparison.OrdinalIgnoreCase))
                return true;

            //  Split inputWord and check pieces
            var inputParts = inputWord.Split(new[] { ' ', '&', ',', '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in inputParts)
            {
                if (part.Contains(existingWord, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /*public static DateTime ConvertUtcToUkTime(DateTime utcDateTime)
        {
            // UK time zone (handles GMT and BST automatically)
            TimeZoneInfo ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, ukTimeZone);
        }*/

        public static DateTime ConvertUtcToUkTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            TimeZoneInfo ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, ukTimeZone);
        }

        public static DateTime ConvertUkTimeToUtc(DateTime ukDateTime)
        {
            // Always treat the input as "Unspecified" kind, since it's a local time entered by user
            ukDateTime = DateTime.SpecifyKind(ukDateTime, DateTimeKind.Unspecified);

            TimeZoneInfo ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(ukDateTime, ukTimeZone);
        }    
        public static string GenerateApiKey()
        {
            return Guid.NewGuid().ToString("N"); // Remove hyphens
        }       
    }
}
   