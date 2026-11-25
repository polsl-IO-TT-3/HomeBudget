using System.Windows;

namespace HomeBudget
{
    public partial class UsunGospodarstwoWindow : Window
    {
        private readonly Gospodarstwo _gospodarstwo;

        public UsunGospodarstwoWindow(Gospodarstwo gospodarstwo)
        {
            InitializeComponent();
            _gospodarstwo = gospodarstwo;
            OpisText.Text = $"Czy na pewno chcesz trwale usunąć „{gospodarstwo.Nazwa}”?";

            HasloBox.Focus();
        }

        private void Usun_Click(object sender, RoutedEventArgs e)
        {
            if (_gospodarstwo.SprawdzHaslo(HasloBox.Password))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorText.Text = "Nieprawidłowe hasło. Gospodarstwo nie zostało usunięte.";
                HasloBox.Clear();
                HasloBox.Focus();
            }
        }
    }
}