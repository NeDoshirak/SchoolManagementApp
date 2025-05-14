using Microsoft.Extensions.DependencyInjection;
using Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presentation.Views
{
    /// <summary>
    /// Логика взаимодействия для ScheduleWindow.xaml
    /// </summary>
    public partial class ScheduleWindow : Window
    {
        private static ScheduleWindow _instance;
        private static readonly object _lock = new object();

        public static ScheduleWindow Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ScheduleWindow();
                    }
                    return _instance;
                }
            }
        }

        private ScheduleWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<ScheduleViewModel>();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _instance = null;
        }

        public new void Show()
        {
            if (_instance != null)
            {
                if (_instance.IsVisible)
                {
                    _instance.Activate();
                }
                else
                {
                    base.Show();
                }
            }
        }
    }
}
