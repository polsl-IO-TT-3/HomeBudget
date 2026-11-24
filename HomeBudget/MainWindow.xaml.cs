using System.Windows;
using System.Windows.Controls;
using HomeBudget.Models;
using HomeBudget.ViewModels;
using HomeBudget.Views;

namespace HomeBudget;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void WorkspaceCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Workspace workspace })
            return;

        if (!workspace.IsProtected)
        {
            _viewModel.CompleteLogin(workspace);
            return;
        }

        var popup = new LoginPopupWindow(
            workspace.Name,
            enteredPassword => _viewModel.VerifyWorkspacePassword(workspace, enteredPassword))
        {
            Owner = this
        };

        if (popup.ShowDialog() == true)
        {
            _viewModel.CompleteLogin(workspace);
        }
    }

    private void CreateWorkspaceButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.TryCreateWorkspace(NewWorkspacePasswordInput.Password))
        {
            NewWorkspacePasswordInput.Clear();
        }
    }

    private void CancelCreateButton_Click(object sender, RoutedEventArgs e)
    {
        NewWorkspacePasswordInput.Clear();
    }
}
