using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Windows;

namespace SchoolManagementApp.Presentation.ViewModels
{
    public partial class StudentEditViewModel : ObservableObject
    {
        [ObservableProperty]
        private Student student;

        private readonly IStudentRepository _studentRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;

        public StudentEditViewModel()
        {
            // Конструктор по умолчанию для XAML
            student = new Student();
            _studentRepository = null; // Будет инициализировано в коде-behind
            _dataChangeNotifier = null; // Будет инициализировано в коде-behind
        }

        public StudentEditViewModel(Student student, IStudentRepository studentRepository, IDataChangeNotifier dataChangeNotifier)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));
            Student = new Student
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                PhotoPath = student.PhotoPath,
                ClassId = student.ClassId
            };
            _studentRepository = studentRepository;
            _dataChangeNotifier = dataChangeNotifier;
        }

        [RelayCommand]
        private void Save()
        {
            if (Student != null && _studentRepository != null)
            {
                _studentRepository.Update(Student);
                _dataChangeNotifier.NotifyStudentChanged();
                Window.GetWindow(App.Current.MainWindow)?.Close(); // Закрываем редактирование
            }
        }
    }
}