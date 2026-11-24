using HomeBudget.Models;

namespace HomeBudget.DataAccess;

public class InMemoryBudgetLimitRepository : IBudgetLimitRepository
{
    private readonly InMemoryDataStore _store;

    public InMemoryBudgetLimitRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public IEnumerable<BudgetLimit> GetByWorkspace(Guid workspaceId) =>
        _store.BudgetLimits.Where(l => l.WorkspaceId == workspaceId).ToList();

    public BudgetLimit? GetByWorkspaceAndCategory(Guid workspaceId, Guid categoryId) =>
        _store.BudgetLimits.FirstOrDefault(l => l.WorkspaceId == workspaceId && l.CategoryId == categoryId);

    public void AddOrUpdate(BudgetLimit limit)
    {
        var existing = GetByWorkspaceAndCategory(limit.WorkspaceId, limit.CategoryId);
        if (existing is not null)
            existing.MonthlyLimit = limit.MonthlyLimit;
        else
            _store.BudgetLimits.Add(limit);
    }
}
