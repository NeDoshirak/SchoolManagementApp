using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Presentation.ViewModels
{
    public partial class StudentAddViewModel : ObservableObject
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClassRepository _classRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;
        private readonly Window _window;
        private readonly int _classId;

        [ObservableProperty]
        private ObservableCollection<Student> _availableStudents;

        [ObservableProperty]
        private Student _selectedStudent;

        public bool IsStudentSelected => SelectedStudent != null;

        public StudentAddViewModel(int classId, Window window)
        {
            _studentRepository = App.Services.GetService<IStudentRepository>();
            _classRepository = App.Services.GetService<IClassRepository>();
            _dataChangeNotifier = App.Services.GetService<IDataChangeNotifier>();
            _window = window;
            _classId = classId;

            LoadAvailableStudents();
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedStudent))
                {
                    OnPropertyChanged(nameof(IsStudentSelected));
                }
            };
        }

        private void LoadAvailableStudents()
        {
            var allStudents = _studentRepository.GetAll().ToList();
            var classStudents = _classRepository.GetById(_classId)?.Students ?? new List<Student>();
            AvailableStudents = new ObservableCollection<Student>(allStudents.Except(classStudents, new StudentComparer()));
        }

        [RelayCommand]
        private void Add()
        {
            if (SelectedStudent == null) return;

            try
            {
                SelectedStudent.ClassId = _classId;
                _studentRepository.Update(SelectedStudent);
                _dataChangeNotifier.NotifyStudentChanged();
                _window.Close();
                MessageBox.Show($"{SelectedStudent.FullName} успешно добавлен в класс.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _window.Close();
        }
    }

    // Класс для сравнения студентов (по ID)
    public class StudentComparer : IEqualityComparer<Student>
    {
        public bool Equals(Student x, Student y)
        {
            if (x == null || y == null) return false;
            return x.StudentId == y.StudentId;
        }

        public int GetHashCode(Student obj)
        {
            return obj.StudentId.GetHashCode();
        }
    }
}