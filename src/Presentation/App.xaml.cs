using System.Configuration;
using System.Data;
using System.Windows;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            var connectionString = Configuration.ConfigurationReader.GetConnectionString();
            Services = DependencyInjection.ServiceConfiguration.ConfigureServices(connectionString);


        }
    }

}
