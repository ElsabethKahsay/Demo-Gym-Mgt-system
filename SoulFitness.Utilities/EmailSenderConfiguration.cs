using Microsoft.Extensions.Configuration;
using System.IO;

namespace SoulFitness.Utilities
{
    public class EmailSenderConfiguration
    {
        private readonly string connectionString = string.Empty;
        private readonly string emailSender = string.Empty;
        private readonly string smtpHost = string.Empty;
        private readonly string password = string.Empty;
        private readonly string ccEmails = string.Empty;
        private readonly string siteKey = string.Empty;
        private readonly string secreteKey = string.Empty;

        public EmailSenderConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();

            connectionString = root.GetSection("ConnectionStrings").GetSection("QSMSConnection").Value;
            emailSender = root.GetSection("EmailSettings").GetSection("EmailSender").Value;
            smtpHost = root.GetSection("EmailSettings").GetSection("SMTPHost").Value;
            password = root.GetSection("EmailSettings").GetSection("Password").Value;
            ccEmails = root.GetSection("EmailSettings").GetSection("CC").Value;

            siteKey = root.GetSection("GoogleRecaptcha").GetSection("SiteKey").Value;
            secreteKey = root.GetSection("GoogleRecaptcha").GetSection("SecreteKey").Value;
        }
        public string ConnectionString
        {
            get => connectionString;
        }

        public string EmailSender
        {
            get => emailSender;
        }

        public string SMTPHost
        {
            get => smtpHost;
        }

        public string Password
        {
            get => password;
        }

        public string CCEmails
        {
            get => ccEmails;
        }

        public string SiteKey
        {
            get => siteKey;
        }

        public string SecretKey
        {
            get => secreteKey;
        }
    }
}
