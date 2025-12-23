using System.Text.RegularExpressions;

namespace WALMS.API.Common
{
    public class ImageToIFormFIle
    {
        public IFormFile ConvertBase64ToIFormFile(string base64String, string fileName)
        {
            fileName = fileName+"." + GetFileExtensionFromBase64(base64String);
            
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            byte[] bytes = Convert.FromBase64String(base64String);
            MemoryStream stream = new MemoryStream(bytes);

          

            IFormFile file = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png" 
            };

            return file;
        }
        public static string GetFileExtensionFromBase64(string base64String)
        {
            // Regular expression to extract the MIME type
            var match = Regex.Match(base64String, @"data:(?<type>image\/[a-zA-Z]+);base64,");

            if (match.Success)
            {
                string mimeType = match.Groups["type"].Value;
                return MimeToExtension(mimeType);
            }

            return string.Empty; // Return empty if no valid MIME type is found
        }

        private static string MimeToExtension(string mimeType)
        {
            var mimeMapping = new Dictionary<string, string>
        {
            { "image/jpeg", "jpg" },
            { "image/png", "png" },
            { "image/gif", "gif" },
            { "image/bmp", "bmp" },
            { "image/webp", "webp" },
            { "image/svg+xml", "svg" }
        };

            return mimeMapping.ContainsKey(mimeType) ? mimeMapping[mimeType] : "unknown";
        }
    }
}
