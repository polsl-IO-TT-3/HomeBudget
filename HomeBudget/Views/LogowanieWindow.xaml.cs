using System.Windows;

namespace HomeBudget
{
    public partial class LogowanieWindow : Window
    {
        private readonly Gospodarstwo _gospodarstwo;

        public LogowanieWindow(Gospodarstwo gospodarstwo, string tytulOkna = "Logowanie", string tekstPrzycisku = "Loguj")
        {
            InitializeComponent();
            _gospodarstwo = gospodarstwo;
            Title = tytulOkna;
            WorkspaceTitleText.Text = gospodarstwo.Nazwa;
            ConfirmButton.Content = tekstPrzycisku;
            HasloBox.Focus();
        }

        private void Loguj_Click(object sender, RoutedEventArgs e)
        {
            if (_gospodarstwo.SprawdzHaslo(HasloBox.Password))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorText.Text = "Nieprawidłowe hasło.";
            }
        }
    }
}
