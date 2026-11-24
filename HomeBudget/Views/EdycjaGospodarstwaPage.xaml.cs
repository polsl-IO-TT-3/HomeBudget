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

            // Jeśli gospodarstwo nie ma hasła, ukrywamy pole potwierdzenia obecnym hasłem
            StareHasloPanel.Visibility = _gospodarstwo.CzyChronione ? Visibility.Visible : Visibility.Collapsed;

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
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
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

            // Każda zmiana wymaga zatwierdzenia obecnym hasłem (jeśli gospodarstwo jest chronione)
            if (_gospodarstwo.CzyChronione && StareHasloBox.Password != _gospodarstwo.Haslo)
            {
                ErrorText.Text = "Wpisz poprawne obecne hasło, aby zatwierdzić zmiany.";
                StareHasloBox.Clear();
                StareHasloBox.Focus();
                return;
            }

            // Jeśli sekcja zmiany hasła została rozwinięta i wpisano nowe hasło
            if (ZmianaHaslaExpander.IsExpanded && !string.IsNullOrEmpty(NoweHasloBox.Password))
            {
                _gospodarstwo.Haslo = HasloHelper.Hash(NoweHasloBox.Password);
            }

            _gospodarstwo.Nazwa = nowaNazwa;
            BazaDanych.Zapisz();

            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}