using Application.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Presentation.ViewModels;
using Presentation.Views;
using SchoolDB.Application.Interfaces;

namespace Presentation.DependencyInjection
{
    public static class ServiceConfiguration
    {
        public static ServiceProvider ConfigureServices(string connectionString)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDataChangeNotifier, DataChangeNotifier>();

            services.AddSingleton<IClassRepository>(sp => new ClassRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<IStudentRepository>(sp => new StudentRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<ITeacherRepository>(sp => new TeacherRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<ISubjectRepository>(sp => new SubjectRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<IGradeRepository>(sp => new GradeRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<IQuarterRepository>(sp => new QuarterRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<IScheduleRepository>(sp => new ScheduleRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));
            services.AddSingleton<ITeacherSubjectRepository>(sp => new TeacherSubjectRepository(connectionString, sp.GetRequiredService<IDataChangeNotifier>()));


            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<StudentViewModel>();
            services.AddSingleton<StudentWindow>();
            services.AddSingleton<ClassViewModel>();
            services.AddSingleton<StudentAddViewModel>();
            services.AddSingleton<TeacherViewModel>();
            services.AddSingleton<SubjectViewModel>();
            services.AddSingleton<QuarterViewModel>();
            services.AddSingleton<GradeViewModel>();
            services.AddSingleton<ScheduleViewModel>();


            return services.BuildServiceProvider();
        }
    }
}