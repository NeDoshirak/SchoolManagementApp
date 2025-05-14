using Microsoft.Extensions.DependencyInjection;
using Presentation.ViewModels;
using System;
using System.Windows;

namespace Presentation.Views
{
    public partial class ClassWindow : Window
    {
        private static ClassWindow _instance;
        private static readonly object _lock = new object();

        public static ClassWindow Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ClassWindow();
                    }
                    return _instance;
                }
            }
        }

        private ClassWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<ClassViewModel>();
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