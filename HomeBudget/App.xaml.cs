using System.Windows;
using HomeBudget.DataAccess;
using HomeBudget.Services;
using HomeBudget.ViewModels;

namespace HomeBudget;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var authService = new AuthService();
        var dataStore = new InMemoryDataStore(authService);

        IWorkspaceRepository workspaceRepo = new InMemoryWorkspaceRepository(dataStore);
        ITransactionRepository transactionRepo = new InMemoryTransactionRepository(dataStore);
        ICategoryRepository categoryRepo = new InMemoryCategoryRepository(dataStore);
        IBudgetLimitRepository limitRepo = new InMemoryBudgetLimitRepository(dataStore);

        var workspaceService = new WorkspaceService(workspaceRepo, authService);
        var budgetService = new BudgetService(transactionRepo, categoryRepo, limitRepo);
        var backupService = new BackupService(dataStore);

        var mainViewModel = new MainViewModel(workspaceService, budgetService, backupService);
        var mainWindow = new MainWindow(mainViewModel);

        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
