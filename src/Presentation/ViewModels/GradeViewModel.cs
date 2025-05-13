using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Presentation.ViewModels
{
    public partial class GradeViewModel : ObservableObject
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IQuarterRepository _quarterRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;

        [ObservableProperty]
        private ObservableCollection<Grade> _grades;

        [ObservableProperty]
        private Grade _selectedGrade;

        [ObservableProperty]
        private ObservableCollection<Teacher> _teachers;

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects;

        public bool HasSelectedGrade => SelectedGrade != null;

        public GradeViewModel(
            IGradeRepository gradeRepository,
            IStudentRepository studentRepository,
            ISubjectRepository subjectRepository,
            ITeacherRepository teacherRepository,
            IQuarterRepository quarterRepository,
            IDataChangeNotifier dataChangeNotifier)
        {
            _gradeRepository = gradeRepository;
            _studentRepository = studentRepository;
            _subjectRepository = subjectRepository;
            _teacherRepository = teacherRepository;
            _quarterRepository = quarterRepository;
            _dataChangeNotifier = dataChangeNotifier;

            LoadGrades();
            LoadTeachers();
            SubscribeToChanges();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedGrade))
                {
                    OnPropertyChanged(nameof(HasSelectedGrade));
                }
            };
        }

        private void LoadGrades()
        {
            Grades = new ObservableCollection<Grade>(_gradeRepository.GetAll());
        }

        private void LoadTeachers()
        {
            Teachers = new ObservableCollection<Teacher>(_teacherRepository.GetAll());
        }

        private void SubscribeToChanges()
        {
            _dataChangeNotifier.GradeChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var prevSelectedId = SelectedGrade?.GradeId;
                    LoadGrades();
                    if (prevSelectedId.HasValue)
                    {
                        SelectedGrade = Grades.FirstOrDefault(g => g.GradeId == prevSelectedId);
                    }
                });
            };
        }

        [RelayCommand]
        private void AddGrade()
        {
            var window = new Window
            {
                Title = "Добавление оценки",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var teacherComboBox = new ComboBox
            {
                ItemsSource = Teachers,
                DisplayMemberPath = "FullName",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var subjectComboBox = new ComboBox
            {
                DisplayMemberPath = "SubjectName",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var studentComboBox = new ComboBox
            {
                ItemsSource = _studentRepository.GetAll(),
                DisplayMemberPath = "FullName",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var quarterComboBox = new ComboBox
            {
                ItemsSource = _quarterRepository.GetAll(),
                Margin = new Thickness(0, 0, 0, 10)
            };
            quarterComboBox.ItemTemplate = new DataTemplate
            {
                DataType = typeof(Quarter),
                VisualTree = new FrameworkElementFactory(typeof(TextBlock))
            };
            quarterComboBox.ItemTemplate.VisualTree.SetValue(TextBlock.TextProperty, new MultiBinding
            {
                Converter = new Presentation.Converters.QuarterDisplayConverter(),
                Bindings =
                {
                    new Binding("QuarterNumber"),
                    new Binding("AcademicYear")
                }
            });
            var gradeComboBox = new ComboBox
            {
                ItemsSource = new[] { 1, 2, 3, 4, 5 },
                Margin = new Thickness(0, 0, 0, 10)
            };

            stackPanel.Children.Add(new TextBlock { Text = "Учитель:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(teacherComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Предмет:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(subjectComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Ученик:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(studentComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Четверть:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(quarterComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Оценка:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(gradeComboBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            teacherComboBox.SelectionChanged += (s, e) =>
            {
                if (teacherComboBox.SelectedItem is Teacher selectedTeacher)
                {
                    subjectComboBox.ItemsSource = _subjectRepository.GetAll()
                        .Where(sub => sub.TeacherSubjects.Any(ts => ts.TeacherId == selectedTeacher.TeacherId))
                        .ToList();
                }
                else
                {
                    subjectComboBox.ItemsSource = null;
                }
            };

            okButton.Click += (s, e) =>
            {
                if (teacherComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите учителя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (subjectComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите предмет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (studentComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите ученика.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (quarterComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите четверть.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (gradeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите оценку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var grade = new Grade
                    {
                        StudentId = ((Student)studentComboBox.SelectedItem).StudentId,
                        SubjectId = ((Subject)subjectComboBox.SelectedItem).SubjectId,
                        QuarterId = ((Quarter)quarterComboBox.SelectedItem).QuarterId,
                        GradeValue = (int)gradeComboBox.SelectedItem
                    };
                    _gradeRepository.Create(grade);
                    _dataChangeNotifier.NotifyGradeChanged();
                    MessageBox.Show($"Оценка {grade.GradeValue} успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void EditGrade(Grade grade)
        {
            if (grade == null) return;

            var window = new Window
            {
                Title = "Редактирование оценки",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var teacherComboBox = new ComboBox
            {
                ItemsSource = Teachers,
                DisplayMemberPath = "FullName",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var subjectComboBox = new ComboBox
            {
                DisplayMemberPath = "SubjectName",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var studentComboBox = new ComboBox
            {
                ItemsSource = _studentRepository.GetAll(),
                DisplayMemberPath = "FullName",
                Margin = new Thickness(0, 0, 0, 10)
            };
            var quarterComboBox = new ComboBox
            {
                ItemsSource = _quarterRepository.GetAll(),
                Margin = new Thickness(0, 0, 0, 10)
            };
            quarterComboBox.ItemTemplate = new DataTemplate
            {
                DataType = typeof(Quarter),
                VisualTree = new FrameworkElementFactory(typeof(TextBlock))
            };
            quarterComboBox.ItemTemplate.VisualTree.SetValue(TextBlock.TextProperty, new MultiBinding
            {
                Converter = new Presentation.Converters.QuarterDisplayConverter(),
                Bindings =
                {
                    new Binding("QuarterNumber"),
                    new Binding("AcademicYear")
                }
            });
            var gradeComboBox = new ComboBox
            {
                ItemsSource = new[] { 1, 2, 3, 4, 5 },
                SelectedItem = grade.GradeValue,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Предварительно выбираем текущие значения с проверкой на null
            var teacher = Teachers.FirstOrDefault(t => t.TeacherSubjects?.Any(ts => ts.SubjectId == grade.SubjectId) == true);
            if (teacher != null)
            {
                teacherComboBox.SelectedItem = teacher;
            }
            else
            {
                if (Teachers.Any())
                {
                    teacherComboBox.SelectedIndex = 0;
                    MessageBox.Show($"Учитель для предмета {grade.Subject.SubjectName} не найден. Выбран первый доступный учитель.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Список учителей пуст. Пожалуйста, добавьте учителей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    window.Close();
                    return;
                }
            }

            subjectComboBox.ItemsSource = _subjectRepository.GetAll()
                .Where(sub => sub.TeacherSubjects?.Any(ts => ts.TeacherId == (teacherComboBox.SelectedItem as Teacher)?.TeacherId) == true)
                .ToList();
            subjectComboBox.SelectedItem = subjectComboBox.Items.Cast<Subject>().FirstOrDefault(s => s.SubjectId == grade.SubjectId);
            studentComboBox.SelectedItem = studentComboBox.Items.Cast<Student>().FirstOrDefault(s => s.StudentId == grade.StudentId);
            quarterComboBox.SelectedItem = quarterComboBox.Items.Cast<Quarter>().FirstOrDefault(q => q.QuarterId == grade.QuarterId);

            stackPanel.Children.Add(new TextBlock { Text = "Учитель:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(teacherComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Предмет:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(subjectComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Ученик:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(studentComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Четверть:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(quarterComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Оценка:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(gradeComboBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            teacherComboBox.SelectionChanged += (s, e) =>
            {
                if (teacherComboBox.SelectedItem is Teacher selectedTeacher)
                {
                    subjectComboBox.ItemsSource = _subjectRepository.GetAll()
                        .Where(sub => sub.TeacherSubjects?.Any(ts => ts.TeacherId == selectedTeacher.TeacherId) == true)
                        .ToList();
                }
                else
                {
                    subjectComboBox.ItemsSource = null;
                }
            };

            okButton.Click += (s, e) =>
            {
                if (teacherComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите учителя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (subjectComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите предмет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (studentComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите ученика.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (quarterComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите четверть.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (gradeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите оценку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    grade.StudentId = ((Student)studentComboBox.SelectedItem).StudentId;
                    grade.SubjectId = ((Subject)subjectComboBox.SelectedItem).SubjectId;
                    grade.QuarterId = ((Quarter)quarterComboBox.SelectedItem).QuarterId;
                    grade.GradeValue = (int)gradeComboBox.SelectedItem;
                    _gradeRepository.Update(grade);
                    _dataChangeNotifier.NotifyGradeChanged();
                    MessageBox.Show($"Оценка {grade.GradeValue} успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void DeleteGrade(Grade grade)
        {
            if (grade == null) return;
            if (MessageBox.Show($"Вы уверены, что хотите удалить оценку {grade.GradeValue} для {grade.Student.FullName} по предмету {grade.Subject.SubjectName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _gradeRepository.Delete(grade.GradeId);
                    _dataChangeNotifier.NotifyGradeChanged();
                    SelectedGrade = null;
                    MessageBox.Show("Оценка успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}