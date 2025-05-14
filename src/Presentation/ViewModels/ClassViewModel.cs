using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Presentation.Views.Popups;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Presentation.ViewModels
{
    public partial class ClassViewModel : ObservableObject
    {
        private readonly IClassRepository _classRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;

        [ObservableProperty]
        private ObservableCollection<Class> _classes;

        [ObservableProperty]
        private Class _selectedClass;

        [ObservableProperty]
        private ObservableCollection<Student> _selectedClassStudents;

        [ObservableProperty]
        private int _selectedClassNumber;

        [ObservableProperty]
        private string _selectedClassLetter;

        [ObservableProperty]
        private string _selectedClassTeacherName;

        public bool HasSelectedClass => SelectedClass != null;

        public ClassViewModel(IClassRepository classRepository, IStudentRepository studentRepository, ITeacherRepository teacherRepository, IDataChangeNotifier dataChangeNotifier)
        {
            _classRepository = classRepository;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _dataChangeNotifier = dataChangeNotifier;

            LoadClasses();
            SubscribeToChanges();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedClass))
                {
                    UpdateStudents();
                    UpdateClassDetails();
                    OnPropertyChanged(nameof(HasSelectedClass));
                }
            };
        }

        private void LoadClasses()
        {
            Classes = new ObservableCollection<Class>(_classRepository.GetAll());
        }

        private void UpdateStudents()
        {
            if (SelectedClass == null)
            {
                SelectedClassStudents = null;
                return;
            }

            SelectedClassStudents = new ObservableCollection<Student>(
                _studentRepository.GetAll().Where(s => s.ClassId == SelectedClass.ClassId));
        }

        private void UpdateClassDetails()
        {
            if (SelectedClass == null)
            {
                SelectedClassNumber = 0;
                SelectedClassLetter = null;
                SelectedClassTeacherName = null;
            }
            else
            {
                SelectedClassNumber = SelectedClass.Number;
                SelectedClassLetter = SelectedClass.Letter;
                SelectedClassTeacherName = SelectedClass.Teacher != null ? SelectedClass.Teacher.FullName : "Не назначен";
            }
        }

        private void SubscribeToChanges()
        {
            _dataChangeNotifier.StudentChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateStudents();
                });
            };

            _dataChangeNotifier.ClassChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadClasses();
                    if (SelectedClass != null)
                    {
                        SelectedClass = Classes.FirstOrDefault(c => c.ClassId == SelectedClass.ClassId);
                    }
                });
            };

            _dataChangeNotifier.TeacherChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadClasses();
                    if (SelectedClass != null)
                    {
                        SelectedClass = Classes.FirstOrDefault(c => c.ClassId == SelectedClass.ClassId);
                    }
                });
            };
        }

        [RelayCommand]
        private void AddClass()
        {
            var window = new Window
            {
                Title = "Добавление класса",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var numberBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            var letterBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            var teacherComboBox = new ComboBox
            {
                ItemsSource = _teacherRepository.GetAll().Where(t => !t.ClassId.HasValue || t.ClassId == 0).ToList(),
                DisplayMemberPath = "FullName",
                Margin = new Thickness(0, 0, 0, 10)
            };

            stackPanel.Children.Add(new TextBlock { Text = "Номер:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(numberBox);
            stackPanel.Children.Add(new TextBlock { Text = "Буква:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(letterBox);
            stackPanel.Children.Add(new TextBlock { Text = "Классный руководитель:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(teacherComboBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(numberBox.Text) || !int.TryParse(numberBox.Text, out int number) || number <= 0)
                {
                    MessageBox.Show("Номер класса должен быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(letterBox.Text) || letterBox.Text.Length != 1)
                {
                    MessageBox.Show("Буква класса должна быть одним символом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var newClass = new Class
                    {
                        Number = number,
                        Letter = letterBox.Text,
                        TeacherId = teacherComboBox.SelectedItem != null ? ((Teacher)teacherComboBox.SelectedItem).TeacherId : (int?)null
                    };
                    _classRepository.Create(newClass);

                    if (newClass.TeacherId.HasValue)
                    {
                        var teacher = _teacherRepository.GetById(newClass.TeacherId.Value);
                        teacher.ClassId = newClass.ClassId;
                        _teacherRepository.Update(teacher);
                    }

                    _dataChangeNotifier.NotifyClassChanged();
                    MessageBox.Show($"Класс {newClass.FullName} успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    window.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            cancelButton.Click += (s, e) => window.Close();
            window.Content = stackPanel;
            window.ShowDialog();
        }

        [RelayCommand]
        private void EditClass(Class classEntity)
        {
            if (classEntity == null) return;

            var window = new Window
            {
                Title = "Редактирование класса",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var numberBox = new TextBox { Text = classEntity.Number.ToString(), Margin = new Thickness(0, 0, 0, 10) };
            var letterBox = new TextBox { Text = classEntity.Letter, Margin = new Thickness(0, 0, 0, 10) };
            var teacherComboBox = new ComboBox
            {
                ItemsSource = _teacherRepository.GetAll().Where(t => !t.ClassId.HasValue || t.ClassId == 0 || t.ClassId == classEntity.ClassId).ToList(),
                DisplayMemberPath = "FullName",
                Margin = new Thickness(0, 0, 0, 10)
            };
            if (classEntity.TeacherId.HasValue)
            {
                teacherComboBox.SelectedItem = teacherComboBox.Items.Cast<Teacher>().FirstOrDefault(t => t.TeacherId == classEntity.TeacherId.Value);
            }

            stackPanel.Children.Add(new TextBlock { Text = "Номер:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(numberBox);
            stackPanel.Children.Add(new TextBlock { Text = "Буква:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(letterBox);
            stackPanel.Children.Add(new TextBlock { Text = "Классный руководитель:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(teacherComboBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(numberBox.Text) || !int.TryParse(numberBox.Text, out int number) || number <= 0)
                {
                    MessageBox.Show("Номер класса должен быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(letterBox.Text) || letterBox.Text.Length != 1)
                {
                    MessageBox.Show("Буква класса должна быть одним символом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var oldTeacherId = classEntity.TeacherId;
                    classEntity.Number = number;
                    classEntity.Letter = letterBox.Text;
                    classEntity.TeacherId = teacherComboBox.SelectedItem != null ? ((Teacher)teacherComboBox.SelectedItem).TeacherId : (int?)null;

                    if (oldTeacherId.HasValue && oldTeacherId != classEntity.TeacherId)
                    {
                        var oldTeacher = _teacherRepository.GetById(oldTeacherId.Value);
                        oldTeacher.ClassId = null;
                        _teacherRepository.Update(oldTeacher);
                    }

                    if (classEntity.TeacherId.HasValue)
                    {
                        var newTeacher = _teacherRepository.GetById(classEntity.TeacherId.Value);
                        newTeacher.ClassId = classEntity.ClassId;
                        _teacherRepository.Update(newTeacher);
                    }

                    _classRepository.Update(classEntity);
                    _dataChangeNotifier.NotifyClassChanged();
                    MessageBox.Show($"Класс {classEntity.FullName} успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    window.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            cancelButton.Click += (s, e) => window.Close();
            window.Content = stackPanel;
            window.ShowDialog();
        }

        [RelayCommand]
        private void DeleteClass(Class classEntity)
        {
            if (classEntity == null) return;
            if (MessageBox.Show($"Вы уверены, что хотите удалить класс {classEntity.FullName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    if (classEntity.TeacherId.HasValue)
                    {
                        var teacher = _teacherRepository.GetById(classEntity.TeacherId.Value);
                        teacher.ClassId = null;
                        _teacherRepository.Update(teacher);
                    }
                    _classRepository.Delete(classEntity.ClassId);
                    _dataChangeNotifier.NotifyClassChanged();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void AddStudentToClass()
        {
            if (SelectedClass == null) return;
            var addPopup = new StudentAddPopup(SelectedClass.ClassId);
            addPopup.ShowDialog();
        }
    }
}