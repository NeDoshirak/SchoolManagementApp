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
    /// Логика взаимодействия для QuarterWindow.xaml
    /// </summary>
    public partial class QuarterWindow : Window
    {
        private static QuarterWindow _instance;
        private static readonly object _lock = new object();

        public static QuarterWindow Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new QuarterWindow();
                    }
                    return _instance;
                }
            }
        }

        private QuarterWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<QuarterViewModel>();
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
