using System.Windows;
using System.Windows.Controls;

namespace HomeBudget
{
    public partial class TworzenieGospodarstwaPage : Page
    {
        public TworzenieGospodarstwaPage()
        {
            InitializeComponent();
        }

        private void Cofnij_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void Stworz_Click(object sender, RoutedEventArgs e)
        {
            string nazwa = NazwaBox.Text.Trim();
            string haslo = HasloBox.Password;

            if (string.IsNullOrEmpty(nazwa) || string.IsNullOrEmpty(haslo))
            {
                MessageBox.Show("Wypełnij nazwę i hasło!", "Uwaga");
                return;
            }

            BazaDanych.Gospodarstwa.Add(new Gospodarstwo
            {
                Nazwa = nazwa,
                Haslo = haslo
            });

            BazaDanych.Zapisz();

            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
