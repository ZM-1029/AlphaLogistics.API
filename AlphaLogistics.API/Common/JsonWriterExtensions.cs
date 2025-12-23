using System.Text.Json;

namespace WALMS.API.Common
{
    public static class JsonWriterExtensions
    {
        private static readonly AsyncLocal<string?> _currentProperty = new();

        public static string? CurrentPropertyName(this Utf8JsonWriter writer) => _currentProperty.Value;

        public static void SetCurrentProperty(string? name) => _currentProperty.Value = name;
    }

}
