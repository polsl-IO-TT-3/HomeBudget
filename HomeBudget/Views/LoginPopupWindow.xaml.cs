using System.Windows;

namespace HomeBudget.Views;

public partial class LoginPopupWindow : Window
{
    private readonly Func<string, bool> _verifyPassword;

    public LoginPopupWindow(string workspaceName, Func<string, bool> verifyPassword)
    {
        InitializeComponent();
        _verifyPassword = verifyPassword;
        WorkspaceTitleText.Text = workspaceName;
        Loaded += (_, _) => PasswordInput.Focus();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (_verifyPassword(PasswordInput.Password))
        {
            DialogResult = true;
            Close();
            return;
        }

        ErrorText.Text = "Nieprawidłowe hasło. Spróbuj ponownie.";
        PasswordInput.SelectAll();
        PasswordInput.Focus();
    }
}
