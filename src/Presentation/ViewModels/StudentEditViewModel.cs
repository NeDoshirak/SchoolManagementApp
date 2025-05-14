using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Presentation.ViewModels
{
    public class ClassDisplay
    {
        public int ClassId { get; set; }
        public string DisplayName { get; set; }
    }

    public partial class StudentEditViewModel : ObservableObject
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClassRepository _classRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;
        private readonly Student _originalStudent;
        private readonly Window _window;

        [ObservableProperty]
        private string _fullName;

        [ObservableProperty]
        private ObservableCollection<ClassDisplay> _classes;

        [ObservableProperty]
        private ClassDisplay _selectedClass;

        [ObservableProperty]
        private string _photoPath;

        public StudentEditViewModel(Student student, Window window)
        {
            _studentRepository = App.Services.GetService<IStudentRepository>();
            _classRepository = App.Services.GetService<IClassRepository>();
            _dataChangeNotifier = App.Services.GetService<IDataChangeNotifier>();
            _originalStudent = student;
            _window = window;

            LoadClasses();

            if (student != null)
            {
                FullName = student.FullName;
                PhotoPath = student.PhotoPath;
                SelectedClass = Classes.FirstOrDefault(c => c.ClassId == student.ClassId);
            }
        }

        private void LoadClasses()
        {
            Classes = new ObservableCollection<ClassDisplay>(
                _classRepository.GetAll().Select(c => new ClassDisplay
                {
                    ClassId = c.ClassId,
                    DisplayName = $"{c.Number}{c.Letter}"
                }));
        }

        [RelayCommand]
        private void SelectPhoto()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var sourceFile = dialog.FileName;
                if (!File.Exists(sourceFile))
                {
                    MessageBox.Show("Выбранный файл не существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var destinationDir = Path.Combine(baseDir, "Images", "Students");
                Directory.CreateDirectory(destinationDir);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(sourceFile); // Уникальное имя файла
                var destinationPath = Path.Combine(destinationDir, fileName);

                File.Copy(sourceFile, destinationPath, true);

                // Используем полный путь относительно приложения
                PhotoPath = Path.Combine("Images", "Students", fileName).Replace("\\", "/");

                MessageBox.Show($"Файл скопирован в: {destinationPath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(FullName) || SelectedClass == null || string.IsNullOrWhiteSpace(PhotoPath))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var student = new Student
                {
                    FullName = FullName,
                    ClassId = SelectedClass.ClassId,
                    PhotoPath = PhotoPath
                };

                if (_originalStudent == null)
                {
                    _studentRepository.Create(student);
                }
                else
                {
                    student.StudentId = _originalStudent.StudentId;
                    _studentRepository.Update(student);
                }

                _dataChangeNotifier.NotifyStudentChanged();
                _window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _window.Close();
        }
    }
}