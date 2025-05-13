
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
    public partial class ScheduleViewModel : ObservableObject
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IClassRepository _classRepository;
        private readonly IQuarterRepository _quarterRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;

        [ObservableProperty]
        private ObservableCollection<Schedule> _schedules;

        [ObservableProperty]
        private Schedule _selectedSchedule;

        [ObservableProperty]
        private ObservableCollection<Class> _classes;

        [ObservableProperty]
        private ObservableCollection<Quarter> _quarters;

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects;

        [ObservableProperty]
        private Class _selectedClass;

        [ObservableProperty]
        private Quarter _selectedQuarter;

        public ObservableCollection<string> DaysOfWeek { get; } = new ObservableCollection<string>
        {
            "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"
        };

        public ObservableCollection<int> LessonNumbers { get; } = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7 };

        public bool HasSelectedSchedule => SelectedSchedule != null;

        public ScheduleViewModel(
            IScheduleRepository scheduleRepository,
            IClassRepository classRepository,
            IQuarterRepository quarterRepository,
            ISubjectRepository subjectRepository,
            IDataChangeNotifier dataChangeNotifier)
        {
            _scheduleRepository = scheduleRepository;
            _classRepository = classRepository;
            _quarterRepository = quarterRepository;
            _subjectRepository = subjectRepository;
            _dataChangeNotifier = dataChangeNotifier;

            LoadSchedules();
            LoadClasses();
            LoadQuarters();
            LoadSubjects();
            SubscribeToChanges();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedSchedule))
                {
                    OnPropertyChanged(nameof(HasSelectedSchedule));
                }
                if (e.PropertyName == nameof(SelectedClass) || e.PropertyName == nameof(SelectedQuarter))
                {
                    LoadSchedules();
                }
            };
        }

        private void LoadSchedules()
        {
            var allSchedules = _scheduleRepository.GetAll();
            if (SelectedClass != null && SelectedQuarter != null)
            {
                Schedules = new ObservableCollection<Schedule>(
                    allSchedules.Where(s => s.ClassId == SelectedClass.ClassId && s.QuarterId == SelectedQuarter.QuarterId));
            }
            else
            {
                Schedules = new ObservableCollection<Schedule>(allSchedules);
            }
        }

        private void LoadClasses()
        {
            Classes = new ObservableCollection<Class>(_classRepository.GetAll());
            if (Classes.Any())
            {
                SelectedClass = Classes.First();
            }
        }

        private void LoadQuarters()
        {
            Quarters = new ObservableCollection<Quarter>(_quarterRepository.GetAll());
            if (Quarters.Any())
            {
                SelectedQuarter = Quarters.First();
            }
        }

        private void LoadSubjects()
        {
            Subjects = new ObservableCollection<Subject>(_subjectRepository.GetAll());
        }

        private void SubscribeToChanges()
        {
            _dataChangeNotifier.ScheduleChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var prevSelectedId = SelectedSchedule?.ScheduleId;
                    LoadSchedules();
                    if (prevSelectedId.HasValue)
                    {
                        SelectedSchedule = Schedules.FirstOrDefault(s => s.ScheduleId == prevSelectedId);
                    }
                });
            };
        }

        [RelayCommand]
        private void AddSchedule()
        {
            var window = new Window
            {
                Title = "Добавление урока в расписание",
                Width = 400,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var classComboBox = new ComboBox
            {
                ItemsSource = Classes,
                SelectedItem = SelectedClass,
                DisplayMemberPath = "FullName",
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var quarterComboBox = new ComboBox
            {
                ItemsSource = Quarters,
                SelectedItem = SelectedQuarter,
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
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
            var subjectComboBox = new ComboBox
            {
                ItemsSource = Subjects,
                DisplayMemberPath = "SubjectName",
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var dayOfWeekComboBox = new ComboBox
            {
                ItemsSource = DaysOfWeek,
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var lessonNumberComboBox = new ComboBox
            {
                ItemsSource = LessonNumbers,
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };

            stackPanel.Children.Add(new TextBlock { Text = "Класс:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(classComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Четверть:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(quarterComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Предмет:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(subjectComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "День недели:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(dayOfWeekComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Номер урока:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(lessonNumberComboBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (classComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите класс.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (quarterComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите четверть.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (subjectComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите предмет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (dayOfWeekComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите день недели.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (lessonNumberComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите номер урока.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var schedule = new Schedule
                    {
                        ClassId = ((Class)classComboBox.SelectedItem).ClassId,
                        QuarterId = ((Quarter)quarterComboBox.SelectedItem).QuarterId,
                        SubjectId = ((Subject)subjectComboBox.SelectedItem).SubjectId,
                        DayOfWeek = (string)dayOfWeekComboBox.SelectedItem,
                        LessonNumber = (int)lessonNumberComboBox.SelectedItem
                    };
                    _scheduleRepository.Create(schedule);
                    _dataChangeNotifier.NotifyScheduleChanged();
                    MessageBox.Show("Урок успешно добавлен в расписание.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void EditSchedule(Schedule schedule)
        {
            if (schedule == null) return;

            var window = new Window
            {
                Title = "Редактирование урока в расписании",
                Width = 400,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var classComboBox = new ComboBox
            {
                ItemsSource = Classes,
                DisplayMemberPath = "FullName",
                SelectedItem = Classes.FirstOrDefault(c => c.ClassId == schedule.ClassId),
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var quarterComboBox = new ComboBox
            {
                ItemsSource = Quarters,
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
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
            quarterComboBox.SelectedItem = Quarters.FirstOrDefault(q => q.QuarterId == schedule.QuarterId);
            var subjectComboBox = new ComboBox
            {
                ItemsSource = Subjects,
                DisplayMemberPath = "SubjectName",
                SelectedItem = Subjects.FirstOrDefault(s => s.SubjectId == schedule.SubjectId),
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var dayOfWeekComboBox = new ComboBox
            {
                ItemsSource = DaysOfWeek,
                SelectedItem = schedule.DayOfWeek,
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var lessonNumberComboBox = new ComboBox
            {
                ItemsSource = LessonNumbers,
                SelectedItem = schedule.LessonNumber,
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0)
            };

            stackPanel.Children.Add(new TextBlock { Text = "Класс:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(classComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Четверть:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(quarterComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Предмет:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(subjectComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "День недели:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(dayOfWeekComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Номер урока:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(lessonNumberComboBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (classComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите класс.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (quarterComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите четверть.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (subjectComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите предмет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (dayOfWeekComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите день недели.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (lessonNumberComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите номер урока.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    schedule.ClassId = ((Class)classComboBox.SelectedItem).ClassId;
                    schedule.QuarterId = ((Quarter)quarterComboBox.SelectedItem).QuarterId;
                    schedule.SubjectId = ((Subject)subjectComboBox.SelectedItem).SubjectId;
                    schedule.DayOfWeek = (string)dayOfWeekComboBox.SelectedItem;
                    schedule.LessonNumber = (int)lessonNumberComboBox.SelectedItem;
                    _scheduleRepository.Update(schedule);
                    _dataChangeNotifier.NotifyScheduleChanged();
                    MessageBox.Show("Урок успешно обновлён в расписании.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void DeleteSchedule(Schedule schedule)
        {
            if (schedule == null) return;
            if (MessageBox.Show($"Вы уверены, что хотите удалить урок {schedule.Subject.SubjectName} в {schedule.DayOfWeek} (урок {schedule.LessonNumber}) для класса {schedule.Class.Number}{schedule.Class.Letter}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _scheduleRepository.Delete(schedule.ScheduleId);
                    _dataChangeNotifier.NotifyScheduleChanged();
                    SelectedSchedule = null;
                    MessageBox.Show("Урок успешно удалён из расписания.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}