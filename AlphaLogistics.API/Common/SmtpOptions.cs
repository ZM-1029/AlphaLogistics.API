namespace AlphaLogistics.API.Common
{
    public class SmtpOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSSL { get; set; }
        public string FromEmail { get; set; }
        public bool EnableEmail { get; set; }
    }
}
