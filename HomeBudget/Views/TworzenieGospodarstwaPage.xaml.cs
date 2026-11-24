using System.Windows;
using System.Windows.Controls;

namespace HomeBudget
{
    public partial class TworzenieGospodarstwaPage : Page
    {
        public TworzenieGospodarstwaPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var okno = Window.GetWindow(this);
                if (okno != null) okno.Title = "Tworzenie gospodarstwa";
                NazwaBox.Focus();
            };
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

            if (string.IsNullOrEmpty(nazwa))
            {
                ErrorText.Text = "Podaj nazwę gospodarstwa.";
                return;
            }
            if (BazaDanych.CzyNazwaIstnieje(nazwa))
            {
                ErrorText.Text = "Gospodarstwo o takiej nazwie już istnieje.";
                return;
            }

            if (string.IsNullOrEmpty(haslo))
            {
                ErrorText.Text = "Hasło nie może być puste.";
                return;
            }

            BazaDanych.Gospodarstwa.Add(new Gospodarstwo
            {
                Nazwa = nazwa,
                HasloHash = HasloHelper.Hash(haslo)
            });

            BazaDanych.Zapisz();

            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
