using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System;
using System.IO;

namespace Presentation.ViewModels
{
    public partial class TeacherViewModel : ObservableObject
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IClassRepository _classRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;

        [ObservableProperty]
        private ObservableCollection<Teacher> _teachers;

        [ObservableProperty]
        private Teacher _selectedTeacher;

        [ObservableProperty]
        private string _selectedTeacherFullName;

        [ObservableProperty]
        private bool _selectedTeacherIsActive;

        [ObservableProperty]
        private string _selectedTeacherPhotoPath;

        [ObservableProperty]
        private string _selectedTeacherClassName;

        [ObservableProperty]
        private string _selectedTeacherCabinetNumber;

        public bool HasSelectedTeacher => SelectedTeacher != null;

        public TeacherViewModel(ITeacherRepository teacherRepository, IClassRepository classRepository, IDataChangeNotifier dataChangeNotifier)
        {
            _teacherRepository = teacherRepository;
            _classRepository = classRepository;
            _dataChangeNotifier = dataChangeNotifier;

            LoadTeachers();
            SubscribeToChanges();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedTeacher))
                {
                    UpdateTeacherDetails();
                    OnPropertyChanged(nameof(HasSelectedTeacher));
                }
            };
        }

        private void LoadTeachers()
        {
            Teachers = new ObservableCollection<Teacher>(_teacherRepository.GetAll());
        }

        private void UpdateTeacherDetails()
        {
            if (SelectedTeacher == null)
            {
                SelectedTeacherFullName = null;
                SelectedTeacherIsActive = false;
                SelectedTeacherPhotoPath = null;
                SelectedTeacherClassName = null;
                SelectedTeacherCabinetNumber = null;
            }
            else
            {
                SelectedTeacherFullName = SelectedTeacher.FullName;
                SelectedTeacherIsActive = SelectedTeacher.IsActive;
                SelectedTeacherPhotoPath = SelectedTeacher.PhotoPath;
                SelectedTeacherClassName = SelectedTeacher.Class != null ? SelectedTeacher.Class.FullName : "Не назначен";
                SelectedTeacherCabinetNumber = SelectedTeacher.CabinetNumber?.ToString() ?? "Не указан";
            }
        }

        private void SubscribeToChanges()
        {
            _dataChangeNotifier.TeacherChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadTeachers();
                    if (SelectedTeacher != null)
                    {
                        SelectedTeacher = Teachers.FirstOrDefault(t => t.TeacherId == SelectedTeacher.TeacherId);
                    }
                });
            };

            _dataChangeNotifier.ClassChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadTeachers();
                    if (SelectedTeacher != null)
                    {
                        SelectedTeacher = Teachers.FirstOrDefault(t => t.TeacherId == SelectedTeacher.TeacherId);
                    }
                });
            };

            _dataChangeNotifier.TeacherSubjectChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadTeachers();
                    if (SelectedTeacher != null)
                    {
                        SelectedTeacher = Teachers.FirstOrDefault(t => t.TeacherId == SelectedTeacher.TeacherId);
                    }
                });
            };
        }

        [RelayCommand]
        private void AddTeacher()
        {
            var window = new Window
            {
                Title = "Добавление учителя",
                Width = 300,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var fullNameBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            var isActiveCheck = new CheckBox { Content = "Активен", IsChecked = true, Margin = new Thickness(0, 0, 0, 10) };
            var photoPathBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            var cabinetBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            var classComboBox = new ComboBox
            {
                ItemsSource = _classRepository.GetAll().ToList(),
                DisplayMemberPath = "FullName",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var selectPhotoButton = new Button { Content = "Выбрать фото", Margin = new Thickness(0, 0, 0, 10), Command = new RelayCommand(() => SelectPhoto(photoPathBox)) };

            stackPanel.Children.Add(new TextBlock { Text = "Имя:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(fullNameBox);
            stackPanel.Children.Add(isActiveCheck);
            stackPanel.Children.Add(new TextBlock { Text = "Путь к фото:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(photoPathBox);
            stackPanel.Children.Add(selectPhotoButton);
            stackPanel.Children.Add(new TextBlock { Text = "Номер кабинета:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(cabinetBox);
            stackPanel.Children.Add(new TextBlock { Text = "Класс:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(classComboBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(fullNameBox.Text))
                {
                    MessageBox.Show("Имя учителя не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(cabinetBox.Text) && !int.TryParse(cabinetBox.Text, out int cabinet))
                {
                    MessageBox.Show("Номер кабинета должен быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var newTeacher = new Teacher
                    {
                        FullName = fullNameBox.Text,
                        IsActive = isActiveCheck.IsChecked ?? false,
                        PhotoPath = string.IsNullOrWhiteSpace(photoPathBox.Text) ? null : photoPathBox.Text,
                        CabinetNumber = string.IsNullOrWhiteSpace(cabinetBox.Text) ? null : (int?)int.Parse(cabinetBox.Text),
                        ClassId = classComboBox.SelectedItem != null ? ((Class)classComboBox.SelectedItem).ClassId : (int?)null
                    };
                    _teacherRepository.Create(newTeacher);

                    if (newTeacher.ClassId.HasValue)
                    {
                        var classEntity = _classRepository.GetById(newTeacher.ClassId.Value);
                        if (classEntity.TeacherId.HasValue)
                        {
                            MessageBox.Show("Этот класс уже имеет классного руководителя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        classEntity.TeacherId = newTeacher.TeacherId;
                        _classRepository.Update(classEntity);
                        _dataChangeNotifier.NotifyClassChanged();
                    }

                    _dataChangeNotifier.NotifyTeacherChanged();
                    MessageBox.Show($"Учитель {newTeacher.FullName} успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void EditTeacher(Teacher teacher)
        {
            if (teacher == null) return;

            var window = new Window
            {
                Title = "Редактирование учителя",
                Width = 300,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var fullNameBox = new TextBox { Text = teacher.FullName, Margin = new Thickness(0, 0, 0, 10) };
            var isActiveCheck = new CheckBox { Content = "Активен", IsChecked = teacher.IsActive, Margin = new Thickness(0, 0, 0, 10) };
            var photoPathBox = new TextBox { Text = teacher.PhotoPath ?? "", Margin = new Thickness(0, 0, 0, 10) };
            var cabinetBox = new TextBox { Text = teacher.CabinetNumber?.ToString() ?? "", Margin = new Thickness(0, 0, 0, 10) };
            var classComboBox = new ComboBox
            {
                ItemsSource = _classRepository.GetAll().ToList(),
                DisplayMemberPath = "FullName",
                Margin = new Thickness(0, 0, 0, 10)
            };
            if (teacher.ClassId.HasValue)
            {
                classComboBox.SelectedItem = classComboBox.Items.Cast<Class>().FirstOrDefault(c => c.ClassId == teacher.ClassId.Value);
            }
            var selectPhotoButton = new Button { Content = "Выбрать фото", Margin = new Thickness(0, 0, 0, 10), Command = new RelayCommand(() => SelectPhoto(photoPathBox)) };

            stackPanel.Children.Add(new TextBlock { Text = "Имя:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(fullNameBox);
            stackPanel.Children.Add(isActiveCheck);
            stackPanel.Children.Add(new TextBlock { Text = "Путь к фото:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(photoPathBox);
            stackPanel.Children.Add(selectPhotoButton);
            stackPanel.Children.Add(new TextBlock { Text = "Номер кабинета:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(cabinetBox);
            stackPanel.Children.Add(new TextBlock { Text = "Класс:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(classComboBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(fullNameBox.Text))
                {
                    MessageBox.Show("Имя учителя не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(cabinetBox.Text) && !int.TryParse(cabinetBox.Text, out int cabinet))
                {
                    MessageBox.Show("Номер кабинета должен быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var oldClassId = teacher.ClassId;
                    teacher.FullName = fullNameBox.Text;
                    teacher.IsActive = isActiveCheck.IsChecked ?? false;
                    teacher.PhotoPath = string.IsNullOrWhiteSpace(photoPathBox.Text) ? null : photoPathBox.Text;
                    teacher.CabinetNumber = string.IsNullOrWhiteSpace(cabinetBox.Text) ? null : (int?)int.Parse(cabinetBox.Text);
                    teacher.ClassId = classComboBox.SelectedItem != null ? ((Class)classComboBox.SelectedItem).ClassId : (int?)null;

                    if (oldClassId.HasValue && oldClassId != teacher.ClassId)
                    {
                        var oldClass = _classRepository.GetById(oldClassId.Value);
                        oldClass.TeacherId = null;
                        _classRepository.Update(oldClass);
                        _dataChangeNotifier.NotifyClassChanged();
                    }

                    if (teacher.ClassId.HasValue)
                    {
                        var newClass = _classRepository.GetById(teacher.ClassId.Value);
                        if (newClass.TeacherId.HasValue && newClass.TeacherId != teacher.TeacherId)
                        {
                            MessageBox.Show("Этот класс уже имеет классного руководителя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        newClass.TeacherId = teacher.TeacherId;
                        _classRepository.Update(newClass);
                        _dataChangeNotifier.NotifyClassChanged();
                    }

                    _teacherRepository.Update(teacher);
                    _dataChangeNotifier.NotifyTeacherChanged();
                    MessageBox.Show($"Учитель {teacher.FullName} успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void DeleteTeacher(Teacher teacher)
        {
            if (teacher == null) return;
            if (MessageBox.Show($"Вы уверены, что хотите удалить учителя {teacher.FullName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    if (teacher.ClassId.HasValue)
                    {
                        var classEntity = _classRepository.GetById(teacher.ClassId.Value);
                        classEntity.TeacherId = null;
                        _classRepository.Update(classEntity);
                        _dataChangeNotifier.NotifyClassChanged();
                    }
                    _teacherRepository.Delete(teacher.TeacherId);
                    _dataChangeNotifier.NotifyTeacherChanged();
                    SelectedTeacher = null;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SelectPhoto(TextBox photoPathBox)
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
                var destinationDir = Path.Combine(baseDir, "Images", "Teachers");
                Directory.CreateDirectory(destinationDir);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(sourceFile);
                var destinationPath = Path.Combine(destinationDir, fileName);

                File.Copy(sourceFile, destinationPath, true);

                photoPathBox.Text = Path.Combine("Images", "Teachers", fileName).Replace("\\", "/");

                MessageBox.Show($"Файл скопирован в: {destinationPath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}