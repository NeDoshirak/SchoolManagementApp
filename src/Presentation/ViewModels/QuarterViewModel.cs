using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Presentation.ViewModels
{
    public partial class QuarterViewModel : ObservableObject
    {
        private readonly IQuarterRepository _quarterRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;

        [ObservableProperty]
        private ObservableCollection<Quarter> _quarters;

        [ObservableProperty]
        private Quarter _selectedQuarter;

        [ObservableProperty]
        private int _selectedQuarterNumber;

        [ObservableProperty]
        private string _selectedAcademicYear;

        public bool HasSelectedQuarter => SelectedQuarter != null;

        public QuarterViewModel(IQuarterRepository quarterRepository, IDataChangeNotifier dataChangeNotifier)
        {
            _quarterRepository = quarterRepository;
            _dataChangeNotifier = dataChangeNotifier;

            LoadQuarters();
            SubscribeToChanges();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedQuarter))
                {
                    UpdateQuarterDetails();
                    OnPropertyChanged(nameof(HasSelectedQuarter));
                }
            };
        }

        private void LoadQuarters()
        {
            Quarters = new ObservableCollection<Quarter>(_quarterRepository.GetAll());
        }

        private void UpdateQuarterDetails()
        {
            if (SelectedQuarter == null)
            {
                SelectedQuarterNumber = 0;
                SelectedAcademicYear = null;
            }
            else
            {
                SelectedQuarterNumber = SelectedQuarter.QuarterNumber;
                SelectedAcademicYear = SelectedQuarter.AcademicYear;
            }
        }

        private void SubscribeToChanges()
        {
            _dataChangeNotifier.QuarterChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var prevSelectedId = SelectedQuarter?.QuarterId;
                    LoadQuarters();
                    if (prevSelectedId.HasValue)
                    {
                        SelectedQuarter = Quarters.FirstOrDefault(q => q.QuarterId == prevSelectedId) ?? new Quarter();
                        UpdateQuarterDetails();
                    }
                });
            };
        }

        [RelayCommand]
        private void AddQuarter()
        {
            var window = new Window
            {
                Title = "Добавление четверти",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var numberComboBox = new ComboBox
            {
                ItemsSource = new[] { 1, 2, 3, 4 },
                Margin = new Thickness(0, 0, 0, 10)
            };
            var academicYearBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };

            stackPanel.Children.Add(new TextBlock { Text = "Номер четверти:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(numberComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Учебный год (например, 2024-2025):", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(academicYearBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (numberComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите номер четверти.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(academicYearBox.Text))
                {
                    MessageBox.Show("Учебный год не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(academicYearBox.Text, @"^\d{4}/\d{4}$"))
                {
                    MessageBox.Show("Учебный год должен быть в формате ГГГГ/ГГГГ (например, 2024/2025).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var quarter = new Quarter
                    {
                        QuarterNumber = (int)numberComboBox.SelectedItem,
                        AcademicYear = academicYearBox.Text
                    };
                    _quarterRepository.Create(quarter);
                    _dataChangeNotifier.NotifyQuarterChanged();
                    MessageBox.Show($"Четверть {quarter.QuarterNumber} ({quarter.AcademicYear}) успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void EditQuarter(Quarter quarter)
        {
            if (quarter == null) return;

            var window = new Window
            {
                Title = "Редактирование четверти",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var numberComboBox = new ComboBox
            {
                ItemsSource = new[] { 1, 2, 3, 4 },
                SelectedItem = quarter.QuarterNumber,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var academicYearBox = new TextBox
            {
                Text = quarter.AcademicYear,
                Margin = new Thickness(0, 0, 0, 10)
            };

            stackPanel.Children.Add(new TextBlock { Text = "Номер четверти:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(numberComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Учебный год (например, 2024/2025):", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(academicYearBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "ОК", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            okButton.Click += (s, e) =>
            {
                if (numberComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите номер четверти.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(academicYearBox.Text))
                {
                    MessageBox.Show("Учебный год не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(academicYearBox.Text, @"^\d{4}-\d{4}$"))
                {
                    MessageBox.Show("Учебный год должен быть в формате ГГГГ/ГГГГ (например, 2024-2025).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    quarter.QuarterNumber = (int)numberComboBox.SelectedItem;
                    quarter.AcademicYear = academicYearBox.Text;
                    _quarterRepository.Update(quarter);
                    _dataChangeNotifier.NotifyQuarterChanged();
                    MessageBox.Show($"Четверть {quarter.QuarterNumber} ({quarter.AcademicYear}) успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void DeleteQuarter(Quarter quarter)
        {
            if (quarter == null) return;
            if (MessageBox.Show($"Вы уверены, что хотите удалить четверть {quarter.QuarterNumber} ({quarter.AcademicYear})?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _quarterRepository.Delete(quarter.QuarterId);
                    _dataChangeNotifier.NotifyQuarterChanged();
                    SelectedQuarter = null;
                    MessageBox.Show($"Четверть {quarter.QuarterNumber} ({quarter.AcademicYear}) успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}