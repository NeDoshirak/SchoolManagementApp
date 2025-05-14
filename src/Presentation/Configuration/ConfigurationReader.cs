using Microsoft.Extensions.Configuration;
using System.IO;

namespace Presentation.Configuration
{
    public static class ConfigurationReader
    {
        public static string GetConnectionString()
        {
            using var stream = new FileStream("Configuration/appsettings.json", FileMode.Open, FileAccess.Read, FileShare.Read);
            var configuration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            return configuration.GetConnectionString("DefaultConnection");
        }
    }
}