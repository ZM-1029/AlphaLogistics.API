
using System.Text.RegularExpressions;
namespace WALMS.API.Common
{

	public static class TemplateHelper
	{
		public static string? ProcessTemplate<T>(string? template, T model)
		{
			if (template == null) return null;
			template = ReplacePlaceholders(template, model);

			return template;
		}

		static string ReplacePlaceholders<T>(string template, T model)
		{
			if (string.IsNullOrWhiteSpace(template) || model == null)
				return template;

			var properties = typeof(T).GetProperties();
			foreach (var property in properties)
			{
				/*var placeholder = $"{{{{{property.Name.Trim()}}}}}";
				var value = property.GetValue(model)?.ToString() ?? string.Empty;
				template = template.Replace(placeholder, value);*/
				var value = property.GetValue(model)?.ToString() ?? string.Empty;
				string placeholderPattern = $@"\{{\{{\s*{property.Name.Trim()}\s*\}}\}}"; // Regex to match {{ PropertyName }} with spaces
				template = Regex.Replace(template, placeholderPattern, property.GetValue(model)?.ToString() ?? string.Empty, RegexOptions.IgnoreCase);

			}

			return template;
		}

		private static string AddCustomPlaceholders(string template)
		{
			template = template.Replace("{{CurrentDate}}", DateTime.Now.ToString("yyyy-MM-dd"));

			return template;
		}

		private static void ValidateTemplate(string template)
		{
			var unresolvedPlaceholders = Regex.Matches(template, "{{(.*?)}}");
			if (unresolvedPlaceholders.Count > 0)
			{
				throw new InvalidOperationException($"Unresolved placeholders: {string.Join(", ", unresolvedPlaceholders)}");
			}
		}
	}

}
