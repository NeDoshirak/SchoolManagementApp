using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace Presentation.ViewModels
{
    public partial class SubjectViewModel : ObservableObject
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects;

        [ObservableProperty]
        private Subject _selectedSubject;

        [ObservableProperty]
        private string _selectedSubjectName;

        [ObservableProperty]
        private ObservableCollection<Teacher> _selectedSubjectTeachers;

        public bool HasSelectedSubject => SelectedSubject != null;

        public SubjectViewModel(ISubjectRepository subjectRepository, ITeacherRepository teacherRepository, IDataChangeNotifier dataChangeNotifier)
        {
            _subjectRepository = subjectRepository;
            _teacherRepository = teacherRepository;
            _dataChangeNotifier = dataChangeNotifier;

            LoadSubjects();
            SubscribeToChanges();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedSubject))
                {
                    UpdateSubjectDetails();
                    OnPropertyChanged(nameof(HasSelectedSubject));
                }
            };
        }

        private void LoadSubjects()
        {
            var subjectsList = _subjectRepository.GetAll().ToList();
            Subjects = new ObservableCollection<Subject>(subjectsList);
        }

        private void UpdateSubjectDetails()
        {
            if (SelectedSubject == null)
            {
                SelectedSubjectName = null;
                SelectedSubjectTeachers = null;
            }
            else
            {
                SelectedSubjectName = SelectedSubject.SubjectName ?? "Не указано";
                SelectedSubjectTeachers = new ObservableCollection<Teacher>(
                    SelectedSubject.TeacherSubjects?.Select(ts => ts.Teacher) ?? new List<Teacher>());
            }
        }

        private void SubscribeToChanges()
        {
            _dataChangeNotifier.SubjectChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var prevSelectedId = SelectedSubject?.SubjectId;
                    LoadSubjects();
                    if (prevSelectedId.HasValue)
                    {
                        SelectedSubject = Subjects.FirstOrDefault(s => s.SubjectId == prevSelectedId) ?? new Subject();
                        UpdateSubjectDetails();
                    }
                });
            };

            _dataChangeNotifier.TeacherChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedSubject != null)
                    {
                        UpdateSubjectDetails();
                    }
                });
            };

            _dataChangeNotifier.TeacherSubjectChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadSubjects();
                    if (SelectedSubject != null)
                    {
                        SelectedSubject = Subjects.FirstOrDefault(s => s.SubjectId == SelectedSubject.SubjectId);
                        UpdateSubjectDetails();
                    }
                });
            };
        }

        [RelayCommand]
        private void AddSubject()
        {
            var window = new Window
            {
                Title = "Добавление предмета",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var nameBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };

            stackPanel.Children.Add(new TextBlock { Text = "Название:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(nameBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    MessageBox.Show("Название предмета не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var subject = new Subject { SubjectName = nameBox.Text, TeacherSubjects = new List<TeacherSubject>() };
                    _subjectRepository.Create(subject);
                    _dataChangeNotifier.NotifySubjectChanged();
                    MessageBox.Show($"Предмет {subject.SubjectName} успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void EditSubject(Subject subject)
        {
            if (subject == null) return;

            var window = new Window
            {
                Title = "Редактирование предмета",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var nameBox = new TextBox { Text = subject.SubjectName ?? "", Margin = new Thickness(0, 0, 0, 10) };

            stackPanel.Children.Add(new TextBlock { Text = "Название:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(nameBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    MessageBox.Show("Название предмета не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    subject.SubjectName = nameBox.Text;
                    _subjectRepository.Update(subject);
                    _dataChangeNotifier.NotifySubjectChanged();
                    MessageBox.Show($"Предмет {subject.SubjectName} успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void DeleteSubject(Subject subject)
        {
            if (subject == null) return;
            if (MessageBox.Show($"Вы уверены, что хотите удалить предмет {subject.SubjectName ?? "Неизвестный"}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _subjectRepository.Delete(subject.SubjectId);
                    _dataChangeNotifier.NotifySubjectChanged();
                    SelectedSubject = null;
                    MessageBox.Show($"Предмет {subject.SubjectName ?? "Неизвестный"} успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void AddTeacher()
        {
            if (SelectedSubject == null) return;

            var window = new Window
            {
                Title = "Назначение преподавателя",
                Width = 300,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var teacherComboBox = new ComboBox
            {
                ItemsSource = _teacherRepository.GetAll().Where(t => !SelectedSubject.TeacherSubjects.Any(ts => ts.TeacherId == t.TeacherId)).ToList(),
                DisplayMemberPath = "FullName",
                Margin = new Thickness(0, 0, 0, 10)
            };

            stackPanel.Children.Add(new TextBlock { Text = "Выберите преподавателя:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(teacherComboBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (teacherComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите преподавателя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var teacher = (Teacher)teacherComboBox.SelectedItem;
                    if (teacher == null || string.IsNullOrEmpty(teacher.FullName))
                    {
                        MessageBox.Show("Выбранный преподаватель некорректен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var teacherName = teacher.FullName;
                    var subjectName = SelectedSubject.SubjectName ?? "Неизвестный";

                    Debug.WriteLine($"Перед добавлением: SubjectId={SelectedSubject.SubjectId}, SubjectName={subjectName}, TeacherId={teacher.TeacherId}, TeacherName={teacherName}");

                    SelectedSubject.TeacherSubjects = SelectedSubject.TeacherSubjects ?? new List<TeacherSubject>();
                    SelectedSubject.TeacherSubjects.Add(new TeacherSubject { TeacherId = teacher.TeacherId, Teacher = teacher });
                    _subjectRepository.Update(SelectedSubject);
                    _dataChangeNotifier.NotifyTeacherSubjectChanged();
                    _dataChangeNotifier.NotifySubjectChanged();

                    Debug.WriteLine($"После добавления: SubjectId={SelectedSubject?.SubjectId}, SubjectName={SelectedSubject?.SubjectName}, TeacherName={teacherName}");

                    MessageBox.Show($"Преподаватель {teacherName} добавлен к предмету {subjectName}.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void RemoveTeacher(Teacher teacher)
        {
            if (SelectedSubject == null || teacher == null) return;
            if (MessageBox.Show($"Вы уверены, что хотите убрать преподавателя {teacher.FullName} из предмета {SelectedSubject.SubjectName ?? "Неизвестный"}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var teacherSubject = SelectedSubject.TeacherSubjects?.FirstOrDefault(ts => ts.TeacherId == teacher.TeacherId);
                    if (teacherSubject != null)
                    {
                        SelectedSubject.TeacherSubjects.Remove(teacherSubject);
                        _subjectRepository.Update(SelectedSubject);
                        _dataChangeNotifier.NotifyTeacherSubjectChanged();
                        _dataChangeNotifier.NotifySubjectChanged();
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}