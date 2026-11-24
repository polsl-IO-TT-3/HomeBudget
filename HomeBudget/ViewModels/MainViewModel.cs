using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using HomeBudget.Models;
using HomeBudget.Services;

namespace HomeBudget.ViewModels;

public enum ActiveScreen
{
    WorkspaceSelection,
    CreateWorkspace,
    Dashboard
}

public class MainViewModel : ViewModelBase
{
    private static readonly string[] PolishMonths =
    {
        "STYCZEŃ", "LUTY", "MARZEC", "KWIECIEŃ", "MAJ", "CZERWIEC",
        "LIPIEC", "SIERPIEŃ", "WRZESIEŃ", "PAŹDZIERNIK", "LISTOPAD", "GRUDZIEŃ"
    };

    private readonly WorkspaceService _workspaceService;
    private readonly BudgetService _budgetService;
    private readonly BackupService _backupService;

    private ActiveScreen _currentScreen = ActiveScreen.WorkspaceSelection;
    private Workspace? _loggedInWorkspace;
    private string _newWorkspaceName = string.Empty;
    private string _createErrorMessage = string.Empty;

    // Okres analizy (Rok + Miesiąc)
    private int _selectedYear = DateTime.Now.Year;
    private int _selectedMonth = DateTime.Now.Month;

    // Formularz transakcji
    private Category? _selectedCategory;
    private string _newTitle = string.Empty;
    private string _newAmountText = string.Empty;
    private bool _isExpense = true;
    private bool _isRecurring;
    private string _billingDayText = "10";
    private Transaction? _selectedTransaction;

    // Formularz limitu
    private Category? _selectedLimitCategory;
    private string _newLimitAmountText = string.Empty;

    private string _statusMessage = "Tryb offline – dane zapisywane lokalnie.";
    private string _reminderBanner = string.Empty;

    public ObservableCollection<Workspace> Workspaces { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Transaction> MonthlyTransactions { get; } = new();
    public ObservableCollection<Transaction> MonthlyIncomes { get; } = new();
    public ObservableCollection<Transaction> RecurringExpenses { get; } = new();
    public ObservableCollection<CategorySummaryItem> CategorySummaries { get; } = new();

    public ActiveScreen CurrentScreen
    {
        get => _currentScreen;
        private set
        {
            if (SetProperty(ref _currentScreen, value))
            {
                OnPropertyChanged(nameof(IsSelectionVisible));
                OnPropertyChanged(nameof(IsCreateVisible));
                OnPropertyChanged(nameof(IsDashboardVisible));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    public bool IsSelectionVisible => CurrentScreen == ActiveScreen.WorkspaceSelection;
    public bool IsCreateVisible => CurrentScreen == ActiveScreen.CreateWorkspace;
    public bool IsDashboardVisible => CurrentScreen == ActiveScreen.Dashboard;

    public string WindowTitle => CurrentScreen switch
    {
        ActiveScreen.CreateWorkspace => "Tworzenie gospodarstwa",
        ActiveScreen.Dashboard => $"Budżet domowy — {LoggedInWorkspace?.Name}",
        _ => "Budżet domowy"
    };

    public Workspace? LoggedInWorkspace
    {
        get => _loggedInWorkspace;
        private set => SetProperty(ref _loggedInWorkspace, value);
    }

    public string NewWorkspaceName
    {
        get => _newWorkspaceName;
        set
        {
            if (SetProperty(ref _newWorkspaceName, value))
                CreateErrorMessage = string.Empty;
        }
    }

    public string CreateErrorMessage
    {
        get => _createErrorMessage;
        set => SetProperty(ref _createErrorMessage, value);
    }

    // Okres analizy
    public string SelectedMonthTitle => $"{PolishMonths[_selectedMonth - 1]} {_selectedYear}";
    public string PeriodRangeText
    {
        get
        {
            var first = new DateTime(_selectedYear, _selectedMonth, 1);
            var last = first.AddMonths(1).AddDays(-1);
            return $"{first:dd.MM.yyyy} – {last:dd.MM.yyyy}";
        }
    }

    // KPI Dashboardu
    public decimal TotalIncome => MonthlyTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
    public decimal TotalExpenses => MonthlyTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
    public decimal RemainingBudget => TotalIncome - TotalExpenses;
    public decimal RecurringTotal => RecurringExpenses.Sum(t => t.Amount);
    public double SpentRatio => TotalIncome > 0 ? (double)(TotalExpenses / TotalIncome) : (TotalExpenses > 0 ? 1.0 : 0.0);

    public string TotalIncomeText => $"{TotalIncome:N2} zł";
    public string TotalExpensesText => $"{TotalExpenses:N2} zł";
    public string RemainingBudgetText => $"{RemainingBudget:N2} zł";
    public string RecurringTotalText => $"{RecurringTotal:N2} zł";

    // Formularz transakcji
    public Category? SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
    public string NewTitle { get => _newTitle; set => SetProperty(ref _newTitle, value); }
    public string NewAmountText { get => _newAmountText; set => SetProperty(ref _newAmountText, value); }
    public bool IsExpense { get => _isExpense; set => SetProperty(ref _isExpense, value); }
    public bool IsRecurring { get => _isRecurring; set => SetProperty(ref _isRecurring, value); }
    public string BillingDayText { get => _billingDayText; set => SetProperty(ref _billingDayText, value); }
    public Transaction? SelectedTransaction { get => _selectedTransaction; set => SetProperty(ref _selectedTransaction, value); }

    // Formularz limitu
    public Category? SelectedLimitCategory { get => _selectedLimitCategory; set => SetProperty(ref _selectedLimitCategory, value); }
    public string NewLimitAmountText { get => _newLimitAmountText; set => SetProperty(ref _newLimitAmountText, value); }

    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public string ReminderBanner { get => _reminderBanner; set => SetProperty(ref _reminderBanner, value); }
    public bool HasReminders => !string.IsNullOrWhiteSpace(ReminderBanner);

    // Komendy
    public ICommand ShowCreateWorkspaceCommand { get; }
    public ICommand CancelCreateWorkspaceCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand AddTransactionCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ICommand SetLimitCommand { get; }
    public ICommand ExportBackupCommand { get; }

    public MainViewModel(
        WorkspaceService workspaceService,
        BudgetService budgetService,
        BackupService backupService)
    {
        _workspaceService = workspaceService;
        _budgetService = budgetService;
        _backupService = backupService;

        _budgetService.LimitExceeded += OnBudgetLimitExceeded;

        ShowCreateWorkspaceCommand = new RelayCommand(_ => OpenCreateView());
        CancelCreateWorkspaceCommand = new RelayCommand(_ => ReturnToSelection());
        LogoutCommand = new RelayCommand(_ => Logout());
        PreviousMonthCommand = new RelayCommand(_ => ChangeMonth(-1));
        NextMonthCommand = new RelayCommand(_ => ChangeMonth(1));
        AddTransactionCommand = new RelayCommand(_ => AddTransaction());
        DeleteTransactionCommand = new RelayCommand(_ => DeleteSelectedTransaction(), _ => SelectedTransaction is not null);
        SetLimitCommand = new RelayCommand(_ => SetCategoryLimit());
        ExportBackupCommand = new RelayCommand(_ => ExportBackup());

        LoadCategories();
        RefreshWorkspaces();
    }

    private void LoadCategories()
    {
        Categories.Clear();
        foreach (var cat in _budgetService.GetCategories())
            Categories.Add(cat);

        SelectedCategory = Categories.FirstOrDefault(c => c.DefaultType == TransactionType.Expense) ?? Categories.FirstOrDefault();
        SelectedLimitCategory = SelectedCategory;
    }

    public void RefreshWorkspaces()
    {
        Workspaces.Clear();
        foreach (var ws in _workspaceService.GetAllWorkspaces())
            Workspaces.Add(ws);
    }

    public bool TryCreateWorkspace(string password)
    {
        try
        {
            _workspaceService.CreateWorkspace(NewWorkspaceName, password);
            RefreshWorkspaces();
            ReturnToSelection();
            return true;
        }
        catch (Exception ex)
        {
            CreateErrorMessage = ex.Message;
            return false;
        }
    }

    public bool VerifyWorkspacePassword(Workspace workspace, string? password) =>
        _workspaceService.Authenticate(workspace, password);

    public void CompleteLogin(Workspace workspace)
    {
        LoggedInWorkspace = workspace;
        CurrentScreen = ActiveScreen.Dashboard;
        RefreshDashboard();
    }

    private void ChangeMonth(int delta)
    {
        var current = new DateTime(_selectedYear, _selectedMonth, 1).AddMonths(delta);
        _selectedYear = current.Year;
        _selectedMonth = current.Month;
        OnPropertyChanged(nameof(SelectedMonthTitle));
        OnPropertyChanged(nameof(PeriodRangeText));
        RefreshDashboard();
    }

    public void RefreshDashboard()
    {
        if (LoggedInWorkspace is null) return;

        var transactions = _budgetService.GetMonthlyTransactions(LoggedInWorkspace.Id, _selectedYear, _selectedMonth).ToList();
        var limits = _budgetService.GetLimits(LoggedInWorkspace.Id).ToList();

        MonthlyTransactions.Clear();
        MonthlyIncomes.Clear();
        RecurringExpenses.Clear();

        foreach (var t in transactions)
        {
            MonthlyTransactions.Add(t);
            if (t.Type == TransactionType.Income)
                MonthlyIncomes.Add(t);
            if (t.Type == TransactionType.Expense && t.IsRecurring)
                RecurringExpenses.Add(t);
        }

        // Podsumowanie kategorii (Planowane limity vs Rzeczywiste wydatki + % udział)
        CategorySummaries.Clear();
        decimal totalExpenses = TotalExpenses;
        var expenseCategories = Categories.Where(c => c.DefaultType == TransactionType.Expense).ToList();

        foreach (var cat in expenseCategories)
        {
            decimal spent = transactions.Where(t => t.Type == TransactionType.Expense && t.CategoryId == cat.Id).Sum(t => t.Amount);
            decimal limit = limits.FirstOrDefault(l => l.CategoryId == cat.Id)?.MonthlyLimit ?? 0m;
            double share = totalExpenses > 0 ? (double)(spent / totalExpenses) * 100.0 : 0.0;

            CategorySummaries.Add(new CategorySummaryItem
            {
                CategoryName = cat.Name,
                PlannedLimit = limit,
                ActualSpent = spent,
                SharePercent = share
            });
        }

        // Przypomnienia o nadchodzących opłatach cyklicznych
        var reminders = _budgetService.GetUpcomingRecurringReminders(LoggedInWorkspace.Id);
        ReminderBanner = reminders.Count > 0
            ? "Nadchodzące opłaty cykliczne: " + string.Join(" | ", reminders)
            : string.Empty;
        OnPropertyChanged(nameof(HasReminders));

        NotifyKpiChanged();
    }

    private void NotifyKpiChanged()
    {
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(RemainingBudget));
        OnPropertyChanged(nameof(RecurringTotal));
        OnPropertyChanged(nameof(SpentRatio));
        OnPropertyChanged(nameof(TotalIncomeText));
        OnPropertyChanged(nameof(TotalExpensesText));
        OnPropertyChanged(nameof(RemainingBudgetText));
        OnPropertyChanged(nameof(RecurringTotalText));
    }

    private void AddTransaction()
    {
        if (LoggedInWorkspace is null || SelectedCategory is null) return;

        var normalized = NewAmountText.Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            MessageBox.Show("Podaj poprawną kwotę (np. 125.50).", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int? billingDay = null;
        if (IsRecurring)
        {
            if (!int.TryParse(BillingDayText, out var day))
            {
                MessageBox.Show("Podaj poprawny dzień miesiąca (1–31).", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            billingDay = day;
        }

        try
        {
            var type = IsExpense ? TransactionType.Expense : TransactionType.Income;
            var date = new DateTime(_selectedYear, _selectedMonth, Math.Min(DateTime.Now.Day, DateTime.DaysInMonth(_selectedYear, _selectedMonth)));

            _budgetService.AddTransaction(LoggedInWorkspace.Id, SelectedCategory, NewTitle, amount, type, date, IsRecurring, billingDay);

            NewTitle = string.Empty;
            NewAmountText = string.Empty;
            IsRecurring = false;

            RefreshDashboard();
            StatusMessage = "Dodano transakcję i przeliczono budżet.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void DeleteSelectedTransaction()
    {
        if (SelectedTransaction is null) return;

        _budgetService.DeleteTransaction(SelectedTransaction.Id);
        RefreshDashboard();
        StatusMessage = "Usunięto transakcję i przeliczono budżet.";
    }

    private void SetCategoryLimit()
    {
        if (LoggedInWorkspace is null || SelectedLimitCategory is null) return;

        var normalized = NewLimitAmountText.Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var limitAmount))
        {
            MessageBox.Show("Podaj poprawną kwotę limitu.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _budgetService.SetCategoryLimit(LoggedInWorkspace.Id, SelectedLimitCategory, limitAmount, _selectedYear, _selectedMonth);
            NewLimitAmountText = string.Empty;
            RefreshDashboard();
            StatusMessage = $"Zapisano limit dla kategorii {SelectedLimitCategory.Name}: {limitAmount:N2} zł.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ExportBackup()
    {
        try
        {
            var path = _backupService.ExportBackup();
            StatusMessage = $"Zapisano eksport JSON: {path}";
            MessageBox.Show($"Dane wyeksportowano do pliku:\n{path}", "Eksport zakończony", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd eksportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void PerformAutomaticExitBackup()
    {
        try
        {
            _backupService.ExportBackup();
        }
        catch
        {
            // Nie blokuj zamykania aplikacji w prototypie
        }
    }

    private void OnBudgetLimitExceeded(object? sender, LimitExceededEventArgs e)
    {
        MessageBox.Show(
            $"Przekroczono miesięczny limit dla kategorii \"{e.CategoryName}\"!\n" +
            $"Limit: {e.MonthlyLimit:N2} zł\n" +
            $"Wydano: {e.CurrentSpent:N2} zł",
            "Alert przekroczenia limitu",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private void OpenCreateView()
    {
        NewWorkspaceName = string.Empty;
        CreateErrorMessage = string.Empty;
        CurrentScreen = ActiveScreen.CreateWorkspace;
    }

    private void ReturnToSelection()
    {
        NewWorkspaceName = string.Empty;
        CreateErrorMessage = string.Empty;
        CurrentScreen = ActiveScreen.WorkspaceSelection;
    }

    private void Logout()
    {
        LoggedInWorkspace = null;
        CurrentScreen = ActiveScreen.WorkspaceSelection;
    }
}
