
using AlphaLogistics.API.Common;
using System.Net;
using System.Net.Mail;
using WALMS.API.Common;

public static class EmailService
{
	private static readonly SmtpOptions _smtpOptions;  
	private static readonly bool enableEmail;  
    static EmailService()
	{
		var config = new ConfigurationBuilder()
		   .SetBasePath(AppContext.BaseDirectory) // Set the base path for appsettings.json
		   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
		   .Build();

		var smtpSettings = config.GetSection("SmtpSettings");
       // var enableEmail = config.GetValue<bool>("EnableEmail");

        _smtpOptions = new SmtpOptions
        {
            Host = smtpSettings["Host"],
            Port = int.Parse(smtpSettings["Port"]),
            Username = smtpSettings["UserName"],
            Password = smtpSettings["AppPassword"],
            FromEmail = smtpSettings["FromEmail"],
            EnableEmail = smtpSettings.GetValue<bool>("EnableEmail"),
            EnableSSL = smtpSettings.GetValue<bool>("EnableSSL"),
        };
    }
	public static async Task SendEmailAsync(string? email, string? subject, string? message, string? cc = null, string? bcc = null)
	{
		using (var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port))
		{
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password);
			client.EnableSsl = true;

			var mailMessage = new MailMessage
			{
				From = new MailAddress(_smtpOptions.FromEmail),
				Subject = subject,
				Body = message,
				IsBodyHtml = true
			};

			//string appPassword = Environment.GetEnvironmentVariable("SMTP_APP_PASSWORD");

			if (!string.IsNullOrEmpty(email))
			{
				mailMessage.To.Add(email);
			}

			// Add CC if provided
			if (!string.IsNullOrEmpty(cc))
			{
				foreach (var ccEmail in cc.Split(';'))
				{
					mailMessage.CC.Add(ccEmail.Trim());
				}
			}

			// Add BCC if provided
			if (!string.IsNullOrEmpty(bcc))
			{
				foreach (var bccEmail in bcc.Split(';'))
				{
					mailMessage.Bcc.Add(bccEmail.Trim());
				}
			}

			if (!string.IsNullOrEmpty(email) && _smtpOptions.EnableEmail && message!=null && subject!=null)
			{
				await client.SendMailAsync(mailMessage);
			}
        }
	}
}
