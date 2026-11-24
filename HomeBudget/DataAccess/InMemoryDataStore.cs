using HomeBudget.Models;
using HomeBudget.Services;

namespace HomeBudget.DataAccess;

/// <summary>
/// Współdzielona baza in-memory dla Prototypu #1.
/// </summary>
public sealed class InMemoryDataStore
{
    public List<Workspace> Workspaces { get; } = new();
    public List<Category> Categories { get; } = new();
    public List<Transaction> Transactions { get; } = new();
    public List<BudgetLimit> BudgetLimits { get; } = new();

    public InMemoryDataStore(AuthService authService)
    {
        Seed(authService);
    }

    private void Seed(AuthService authService)
    {
        var catSalary = new Category { Name = "Wynagrodzenie", IsVisibleInSummary = false, ShowAsSeparateCard = false };
        var catFood = new Category { Name = "Jedzenie", IsVisibleInSummary = true, ShowAsSeparateCard = false };
        var catHousing = new Category { Name = "Mieszkanie", IsVisibleInSummary = true, ShowAsSeparateCard = false };
        var catSubs = new Category { Name = "Opłaty stałe", IsVisibleInSummary = true, ShowAsSeparateCard = false };
        var catTransport = new Category { Name = "Transport", IsVisibleInSummary = true, ShowAsSeparateCard = false };
        var catHealth = new Category { Name = "Zdrowie i higiena", IsVisibleInSummary = true, ShowAsSeparateCard = false };
        var catRelax = new Category { Name = "Relaks", IsVisibleInSummary = true, ShowAsSeparateCard = false };

        Categories.AddRange(new[] { catSalary, catFood, catHousing, catSubs, catTransport, catHealth, catRelax });

        var ws1 = new Workspace { Name = "Gospodarstwo 1", PasswordHash = authService.HashPassword("1234") };
        var ws2 = new Workspace { Name = "Gospodarstwo 2", PasswordHash = authService.HashPassword("1234") };
        Workspaces.AddRange(new[] { ws1, ws2 });

        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        // Dane demonstracyjne dla Gospodarstwa 1
        Transactions.AddRange(new[]
        {
            new Transaction { WorkspaceId = ws1.Id, CategoryId = catSalary.Id, CategoryName = catSalary.Name, Title = "Wypłata 1", Amount = 4500.00m, Type = TransactionType.Income, Date = monthStart.AddDays(4) },
            new Transaction { WorkspaceId = ws1.Id, CategoryId = catSalary.Id, CategoryName = catSalary.Name, Title = "Wypłata 2 / Stypendium", Amount = 3000.00m, Type = TransactionType.Income, Date = monthStart.AddDays(5) },

            new Transaction { WorkspaceId = ws1.Id, CategoryId = catSubs.Id, CategoryName = catSubs.Name, Title = "Netflix", Amount = 49.00m, Type = TransactionType.Expense, Date = monthStart.AddDays(2), IsRecurring = true, BillingDay = 3, IsPaid = true },
            new Transaction { WorkspaceId = ws1.Id, CategoryId = catSubs.Id, CategoryName = catSubs.Name, Title = "Spotify", Amount = 23.99m, Type = TransactionType.Expense, Date = monthStart.AddDays(4), IsRecurring = true, BillingDay = 5, IsPaid = true },
            new Transaction { WorkspaceId = ws1.Id, CategoryId = catHousing.Id, CategoryName = catHousing.Name, Title = "Czynsz", Amount = 1850.00m, Type = TransactionType.Expense, Date = monthStart.AddDays(7), IsRecurring = true, BillingDay = 10, IsPaid = true },
            new Transaction { WorkspaceId = ws1.Id, CategoryId = catSubs.Id, CategoryName = catSubs.Name, Title = "Internet", Amount = 70.00m, Type = TransactionType.Expense, Date = monthStart.AddDays(9), IsRecurring = true, BillingDay = Math.Min(28, now.Day + 2), IsPaid = false },

            new Transaction { WorkspaceId = ws1.Id, CategoryId = catFood.Id, CategoryName = catFood.Name, Title = "Zakupy spożywcze", Amount = 486.00m, Type = TransactionType.Expense, Date = monthStart.AddDays(3) },
            new Transaction { WorkspaceId = ws1.Id, CategoryId = catTransport.Id, CategoryName = catTransport.Name, Title = "Bilet miesięczny", Amount = 120.00m, Type = TransactionType.Expense, Date = monthStart.AddDays(6) },
            new Transaction { WorkspaceId = ws1.Id, CategoryId = catHealth.Id, CategoryName = catHealth.Name, Title = "Apteka", Amount = 85.50m, Type = TransactionType.Expense, Date = monthStart.AddDays(8) },
            new Transaction { WorkspaceId = ws1.Id, CategoryId = catRelax.Id, CategoryName = catRelax.Name, Title = "Kino i kawa", Amount = 95.00m, Type = TransactionType.Expense, Date = monthStart.AddDays(11) }
        });

        BudgetLimits.AddRange(new[]
        {
            new BudgetLimit { WorkspaceId = ws1.Id, CategoryId = catFood.Id, CategoryName = catFood.Name, MonthlyLimit = 1200.00m },
            new BudgetLimit { WorkspaceId = ws1.Id, CategoryId = catHousing.Id, CategoryName = catHousing.Name, MonthlyLimit = 2000.00m },
            new BudgetLimit { WorkspaceId = ws1.Id, CategoryId = catTransport.Id, CategoryName = catTransport.Name, MonthlyLimit = 300.00m },
            new BudgetLimit { WorkspaceId = ws1.Id, CategoryId = catRelax.Id, CategoryName = catRelax.Name, MonthlyLimit = 250.00m }
        });

        // Izolowane dane dla Gospodarstwa 2
        Transactions.AddRange(new[]
        {
            new Transaction { WorkspaceId = ws2.Id, CategoryId = catSalary.Id, CategoryName = catSalary.Name, Title = "Budżet wspólny", Amount = 9200.00m, Type = TransactionType.Income, Date = monthStart.AddDays(1) },
            new Transaction { WorkspaceId = ws2.Id, CategoryId = catHousing.Id, CategoryName = catHousing.Name, Title = "Kredyt i rachunki", Amount = 3200.00m, Type = TransactionType.Expense, Date = monthStart.AddDays(2), IsRecurring = true, BillingDay = 5, IsPaid = true }
        });
    }
}
