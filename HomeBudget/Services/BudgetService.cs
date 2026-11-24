using HomeBudget.DataAccess;
using HomeBudget.Models;

namespace HomeBudget.Services;

public class LimitExceededEventArgs : EventArgs
{
    public string CategoryName { get; }
    public decimal MonthlyLimit { get; }
    public decimal CurrentSpent { get; }

    public LimitExceededEventArgs(string categoryName, decimal monthlyLimit, decimal currentSpent)
    {
        CategoryName = categoryName;
        MonthlyLimit = monthlyLimit;
        CurrentSpent = currentSpent;
    }
}

public class BudgetService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IBudgetLimitRepository _limitRepo;

    public event EventHandler<LimitExceededEventArgs>? LimitExceeded;

    public BudgetService(
        ITransactionRepository transactionRepo,
        ICategoryRepository categoryRepo,
        IBudgetLimitRepository limitRepo)
    {
        _transactionRepo = transactionRepo;
        _categoryRepo = categoryRepo;
        _limitRepo = limitRepo;
    }

    public IEnumerable<Category> GetCategories() => _categoryRepo.GetAll();

    public IEnumerable<Transaction> GetMonthlyTransactions(Guid workspaceId, int year, int month) =>
        _transactionRepo.GetByWorkspace(workspaceId)
            .Where(t => t.Date.Year == year && t.Date.Month == month)
            .OrderByDescending(t => t.Date)
            .ToList();

    public IEnumerable<BudgetLimit> GetLimits(Guid workspaceId) =>
        _limitRepo.GetByWorkspace(workspaceId);

    public Transaction AddTransaction(
        Guid workspaceId,
        Category category,
        string title,
        decimal amount,
        TransactionType type,
        DateTime date,
        bool isRecurring = false,
        int? billingDay = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Podaj nazwę / opis transakcji.");

        if (amount <= 0)
            throw new ArgumentException("Kwota musi być większa od zera.");

        if (isRecurring && (billingDay is null || billingDay < 1 || billingDay > 31))
            throw new ArgumentException("Podaj dzień obciążenia z zakresu 1–31.");

        var transaction = new Transaction
        {
            WorkspaceId = workspaceId,
            CategoryId = category.Id,
            CategoryName = category.Name,
            Title = title.Trim(),
            Amount = Math.Round(amount, 2),
            Type = type,
            Date = date,
            IsRecurring = isRecurring,
            BillingDay = isRecurring ? billingDay : null,
            IsPaid = true
        };

        _transactionRepo.Add(transaction);

        if (type == TransactionType.Expense)
        {
            CheckBudgetLimit(workspaceId, category, date.Year, date.Month);
        }

        return transaction;
    }

    public void DeleteTransaction(Guid transactionId) => _transactionRepo.Delete(transactionId);

    public void SetCategoryLimit(Guid workspaceId, Category category, decimal monthlyLimit, int year, int month)
    {
        if (monthlyLimit <= 0)
            throw new ArgumentException("Limit miesięczny musi być większy od zera.");

        _limitRepo.AddOrUpdate(new BudgetLimit
        {
            WorkspaceId = workspaceId,
            CategoryId = category.Id,
            CategoryName = category.Name,
            MonthlyLimit = Math.Round(monthlyLimit, 2)
        });

        CheckBudgetLimit(workspaceId, category, year, month);
    }

    public List<string> GetUpcomingRecurringReminders(Guid workspaceId, int daysAhead = 5)
    {
        var today = DateTime.Today;
        var recurring = _transactionRepo.GetByWorkspace(workspaceId)
            .Where(t => t.IsRecurring && t.BillingDay.HasValue && !t.IsPaid)
            .ToList();

        var reminders = new List<string>();
        foreach (var item in recurring)
        {
            int diff = item.BillingDay!.Value - today.Day;
            if (diff >= 0 && diff <= daysAhead)
            {
                reminders.Add($"{item.Title} ({item.Amount:N2} zł) – termin: {item.BillingDay:00}.{today.Month:00}");
            }
        }
        return reminders;
    }

    private void CheckBudgetLimit(Guid workspaceId, Category category, int year, int month)
    {
        var limit = _limitRepo.GetByWorkspaceAndCategory(workspaceId, category.Id);
        if (limit is null) return;

        var spent = GetMonthlyTransactions(workspaceId, year, month)
            .Where(t => t.Type == TransactionType.Expense && t.CategoryId == category.Id)
            .Sum(t => t.Amount);

        if (spent > limit.MonthlyLimit)
        {
            LimitExceeded?.Invoke(this, new LimitExceededEventArgs(category.Name, limit.MonthlyLimit, spent));
        }
    }
}
