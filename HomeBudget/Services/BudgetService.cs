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
    private readonly HashSet<(Guid WorkspaceId, int Year, int Month)> _propagatedMonths = new();

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

    public Category AddCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Podaj nazwę nowej kategorii.");

        var trimmed = name.Trim();
        if (_categoryRepo.GetAll().Any(c => c.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Kategoria o takiej nazwie już istnieje.");

        var category = new Category
        {
            Name = trimmed,
            IsVisibleInSummary = true,
            ShowAsSeparateCard = false
        };

        _categoryRepo.Add(category);
        return category;
    }

    public IEnumerable<Transaction> GetMonthlyTransactions(Guid workspaceId, int year, int month)
    {
        EnsureRecurringTransactionsForMonth(workspaceId, year, month);

        return _transactionRepo.GetByWorkspace(workspaceId)
            .Where(t => t.Date.Year == year && t.Date.Month == month)
            .OrderByDescending(t => t.Date)
            .ToList();
    }

    /// <summary>
    /// Automatycznie przenosi opłaty stałe (cykliczne) z wcześniejszych miesięcy na kolejne miesiące.
    /// </summary>
    private void EnsureRecurringTransactionsForMonth(Guid workspaceId, int year, int month)
    {
        var key = (workspaceId, year, month);
        if (_propagatedMonths.Contains(key))
            return;

        var targetMonthStart = new DateTime(year, month, 1);
        var allWorkspaceTransactions = _transactionRepo.GetByWorkspace(workspaceId).ToList();

        // Szablony opłat cyklicznych z miesięcy wcześniejszych niż wybrany
        var earlierRecurring = allWorkspaceTransactions
            .Where(t => t.IsRecurring && new DateTime(t.Date.Year, t.Date.Month, 1) < targetMonthStart)
            .GroupBy(t => new
            {
                t.CategoryId,
                Title = t.Title.ToLowerInvariant(),
                t.Type,
                t.BillingDay
            })
            .Select(g => g.OrderByDescending(t => t.Date).First())
            .ToList();

        var existingInTargetMonth = allWorkspaceTransactions
            .Where(t => t.Date.Year == year && t.Date.Month == month && t.IsRecurring)
            .ToList();

        foreach (var template in earlierRecurring)
        {
            bool alreadyExists = existingInTargetMonth.Any(t =>
                t.CategoryId == template.CategoryId &&
                t.Title.Equals(template.Title, StringComparison.OrdinalIgnoreCase) &&
                t.Type == template.Type &&
                t.BillingDay == template.BillingDay);

            if (alreadyExists)
                continue;

            int day = Math.Min(template.BillingDay ?? template.Date.Day, DateTime.DaysInMonth(year, month));
            var carriedOver = new Transaction
            {
                WorkspaceId = workspaceId,
                CategoryId = template.CategoryId,
                CategoryName = template.CategoryName,
                Title = template.Title,
                Amount = template.Amount,
                Type = template.Type,
                Date = new DateTime(year, month, day),
                IsRecurring = true,
                BillingDay = template.BillingDay ?? day,
                IsPaid = false // W nowym miesiącu opłata domyślnie czeka na opłacenie
            };

            _transactionRepo.Add(carriedOver);
            existingInTargetMonth.Add(carriedOver);
        }

        _propagatedMonths.Add(key);
    }

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

        // Jeśli dodano nową opłatę cykliczną, wyczyść cache dla późniejszych miesięcy, aby mogła na nie przejść
        if (isRecurring)
        {
            var addedMonthStart = new DateTime(date.Year, date.Month, 1);
            _propagatedMonths.RemoveWhere(k =>
                k.WorkspaceId == workspaceId && new DateTime(k.Year, k.Month, 1) > addedMonthStart);
        }

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
            .Where(t => t.IsRecurring && t.BillingDay.HasValue && !t.IsPaid && t.Date.Year == today.Year && t.Date.Month == today.Month)
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
