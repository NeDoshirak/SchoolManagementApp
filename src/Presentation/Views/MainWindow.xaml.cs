using Microsoft.Extensions.DependencyInjection;
using Presentation.Configuration;
using Presentation.DependencyInjection;
using Presentation.ViewModels;
using System;
using System.Windows;

namespace Presentation.Views
{
    public partial class MainWindow : Window
    {
        private static MainWindow _instance;
        private static readonly object _lock = new object();
        private IServiceProvider _serviceProvider;

        public static MainWindow Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MainWindow();
                    }
                    return _instance;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _instance = this; // Устанавливаем Singleton при создании
            Loaded += (s, e) => Initialize(); // Синхронная инициализация при загрузке окна
        }


        private void Initialize()
        {
            try
            {
                // Чтение строки подключения
                var connectionString = ConfigurationReader.GetConnectionString();

                // Настройка DI
                _serviceProvider = ServiceConfiguration.ConfigureServices(connectionString);

                // Установка DataContext
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _instance = null; // Сброс Singleton при закрытии
        }
    }
}