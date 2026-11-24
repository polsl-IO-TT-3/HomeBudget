using HomeBudget.Models;

namespace HomeBudget.DataAccess;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly InMemoryDataStore _store;

    public InMemoryTransactionRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public IEnumerable<Transaction> GetByWorkspace(Guid workspaceId) =>
        _store.Transactions
            .Where(t => t.WorkspaceId == workspaceId)
            .OrderByDescending(t => t.Date)
            .ToList();

    public Transaction? GetById(Guid id) => _store.Transactions.FirstOrDefault(t => t.Id == id);

    public void Add(Transaction transaction) => _store.Transactions.Add(transaction);

    public void Update(Transaction transaction)
    {
        var existing = GetById(transaction.Id);
        if (existing is null) return;

        existing.CategoryId = transaction.CategoryId;
        existing.CategoryName = transaction.CategoryName;
        existing.Title = transaction.Title;
        existing.Amount = transaction.Amount;
        existing.Type = transaction.Type;
        existing.Date = transaction.Date;
        existing.IsRecurring = transaction.IsRecurring;
        existing.BillingDay = transaction.BillingDay;
        existing.IsPaid = transaction.IsPaid;
    }

    public void Delete(Guid id)
    {
        var existing = GetById(id);
        if (existing is not null)
            _store.Transactions.Remove(existing);
    }
}
