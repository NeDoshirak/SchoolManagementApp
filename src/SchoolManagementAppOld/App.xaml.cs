using Application.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using SchoolDB.Application.Interfaces;
using SchoolManagementApp.Presentation.ViewModels;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace SchoolManagementApp
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Чтение строки подключения из appsettings.json через поток
            string connectionString;
            string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            try
            {
                if (File.Exists(appSettingsPath))
                {
                    using (var stream = new FileStream(appSettingsPath, FileMode.Open, FileAccess.Read))
                    using (var reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        using JsonDocument doc = JsonDocument.Parse(json);
                        JsonElement root = doc.RootElement;
                        JsonElement connectionStrings = root.GetProperty("ConnectionStrings");
                        connectionString = connectionStrings.GetProperty("DefaultConnection").GetString() ?? "Host=localhost;Port=5432;Database=school;Username=postgres;Password=your_password";
                    }
                }
                else
                {
                    // Fallback, если файл не найден
                    connectionString = "Host=localhost;Port=5432;Database=school;Username=postgres;Password=your_password";
                }
            }
            catch (Exception ex)
            {
                // Если произошла ошибка при чтении файла, используем строку по умолчанию
                connectionString = "Host=localhost;Port=5432;Database=school;Username=postgres;Password=your_password";
                MessageBox.Show($"Ошибка чтения appsettings.json: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Настройка DI
            var services = new ServiceCollection();

            // Регистрация сервисов
            services.AddSingleton<IDataChangeNotifier, DataChangeNotifier>();
            services.AddSingleton<IClassRepository>(sp => new ClassRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<IStudentRepository>(sp => new StudentRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<ITeacherRepository>(sp => new TeacherRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<ISubjectRepository>(sp => new SubjectRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<IGradeRepository>(sp => new GradeRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<IQuarterRepository>(sp => new QuarterRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<IScheduleRepository>(sp => new ScheduleRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<IClassTeacherRepository>(sp => new ClassTeacherRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<ITeacherSubjectRepository>(sp => new TeacherSubjectRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));

            // Регистрация ViewModel-ов
            services.AddTransient<Presentation.ViewModels.MainViewModel>();
            services.AddTransient<Presentation.ViewModels.StudentViewModel>();
            services.AddTransient<Presentation.ViewModels.StudentEditViewModel>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = new SchoolManagementApp.MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<Presentation.ViewModels.MainViewModel>()
            };
            mainWindow.Show();
        }

        public static void ShutdownApplication()
        {
            try
            {
                foreach (Window window in Current.Windows)
                {
                    if (window != null)
                    {
                        window.Close();
                    }
                }

                Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при закрытии приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}