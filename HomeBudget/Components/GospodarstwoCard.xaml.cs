using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HomeBudget.Components
{
    public partial class GospodarstwoCard : UserControl
    {
        public GospodarstwoCard()
        {
            InitializeComponent();
        }

        private void Otworz_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Gospodarstwo wybraneGospodarstwo) return;

            if (!wybraneGospodarstwo.CzyChronione)
            {
                NavigationService.GetNavigationService(this)?.Navigate(new DashboardPage(wybraneGospodarstwo));
                return;
            }

            var oknoLogowania = new LogowanieWindow(wybraneGospodarstwo)
            {
                Owner = Window.GetWindow(this)
            };

            if (oknoLogowania.ShowDialog() == true)
            {
                NavigationService.GetNavigationService(this)?.Navigate(new DashboardPage(wybraneGospodarstwo));
            }
        }

        private void Edytuj_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Gospodarstwo wybraneGospodarstwo) return;

            NavigationService.GetNavigationService(this)?.Navigate(new EdycjaGospodarstwaPage(wybraneGospodarstwo));
        }

        private void Usun_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Gospodarstwo wybraneGospodarstwo) return;

            var oknoUsuwania = new UsunGospodarstwoWindow(wybraneGospodarstwo)
            {
                Owner = Window.GetWindow(this)
            };

            if (oknoUsuwania.ShowDialog() == true)
            {
                BazaDanych.Gospodarstwa.Remove(wybraneGospodarstwo);
                BazaDanych.Zapisz();
            }
        }
    }
}