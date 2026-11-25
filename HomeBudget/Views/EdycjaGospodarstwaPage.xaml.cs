using System.Windows;
using System.Windows.Controls;

namespace HomeBudget
{
    public partial class EdycjaGospodarstwaPage : Page
    {
        private readonly Gospodarstwo _gospodarstwo;

        public EdycjaGospodarstwaPage(Gospodarstwo gospodarstwo)
        {
            InitializeComponent();
            _gospodarstwo = gospodarstwo;
            NowaNazwaBox.Text = gospodarstwo.Nazwa;

            Loaded += (s, e) =>
            {
                var okno = Window.GetWindow(this);
                if (okno != null) okno.Title = "Edycja gospodarstwa";
                NowaNazwaBox.SelectAll();
                NowaNazwaBox.Focus();
            };
        }

        private void Cofnij_Click(object sender, RoutedEventArgs e)
        {
            WrocDoListy();
        }

        private void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = string.Empty;

            string nowaNazwa = NowaNazwaBox.Text.Trim();
            if (string.IsNullOrEmpty(nowaNazwa))
            {
                ErrorText.Text = "Nazwa gospodarstwa nie może być pusta.";
                return;
            }

            if (BazaDanych.CzyNazwaIstnieje(nowaNazwa, _gospodarstwo))
            {
                ErrorText.Text = "Gospodarstwo o takiej nazwie już istnieje.";
                return;
            }

            if (!_gospodarstwo.SprawdzHaslo(StareHasloBox.Password))
            {
                ErrorText.Text = "Nieprawidłowe obecne hasło.";
                StareHasloBox.Clear();
                StareHasloBox.Focus();
                return;
            }

            _gospodarstwo.Nazwa = nowaNazwa;

            if (!string.IsNullOrEmpty(NoweHasloBox.Password))
            {
                _gospodarstwo.UstawHaslo(NoweHasloBox.Password);
            }

            BazaDanych.Zapisz();
            WrocDoListy();
        }

        private void WrocDoListy()
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(new ListaGospodarstwPage());
                while (NavigationService.CanGoBack)
                {
                    NavigationService.RemoveBackEntry();
                }
            }
        }
    }
}