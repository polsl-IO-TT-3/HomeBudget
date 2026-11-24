using System.Windows;
using System.Windows.Controls;
using HomeBudget.Models;

namespace HomeBudget
{
    public partial class DashboardPage : Page
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(Gospodarstwo gospodarstwo)
        {
            InitializeComponent();
            _viewModel = new DashboardViewModel(gospodarstwo, () =>
            {
                NavigationService?.Navigate(new ListaGospodarstwPage());
            });
            DataContext = _viewModel;

            Loaded += (s, e) =>
            {
                var okno = Window.GetWindow(this);
                if (okno != null) okno.Title = $"Budżet domowy — {gospodarstwo.Nazwa}";
            };
        }

        private void CategoryVisibilityCheckBox_Click(object sender, RoutedEventArgs e)
        {
            BazaDanych.Zapisz();
            _viewModel.RefreshDashboard();
        }
    }
}