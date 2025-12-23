using System.Text;
using System.Text.Json;

namespace WALMS.API.Common
{
    public static class SMSServices
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly IConfiguration _config;

        static SMSServices()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        //public static async Task<bool> SendAsync(string phone, string message)
        //{
        //    var bodyObj = new
        //    {
        //        to = phone,
        //        from = "ZOUMA",
        //        msg = message
        //    };

        //    string json = JsonSerializer.Serialize(bodyObj);

        //    var request = new HttpRequestMessage(
        //        HttpMethod.Post,
        //        $"{_config["VoodooSms:BaseUrl"]}/sendsms");

        //    request.Headers.Add("Authorization", $"Bearer {_config["VoodooSms:ApiKey"]}");
        //    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        //    var response = await _httpClient.SendAsync(request);
        //    return response.IsSuccessStatusCode;
        //}
        public static async Task<bool> SendAsync(string phone, string message)
        {
            bool isActive = bool.TryParse(_config["VoodooSms:IsActive"], out bool activeValue) && activeValue;

            if (!isActive)
            {
                Console.WriteLine("SMS service is disabled in configuration.");
                return false; // return false to indicate SMS did NOT send
            }

            /* var bodyObj = new
             {
                 to = phone,
                 from = "ZOUMA",
                 msg = message
             };*/

            var bodyObj = new
            {
                to = phone,
                from = "ZMAPPK",
                msg = message
            };

            string json = JsonSerializer.Serialize(bodyObj);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_config["VoodooSms:BaseUrl"]}/sendsms");

            request.Headers.Add("Authorization", $"Bearer {_config["VoodooSms:ApiKey"]}");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
