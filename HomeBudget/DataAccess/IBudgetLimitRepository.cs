using HomeBudget.Models;

namespace HomeBudget.DataAccess;

public interface IBudgetLimitRepository
{
    IEnumerable<BudgetLimit> GetByWorkspace(Guid workspaceId);
    BudgetLimit? GetByWorkspaceAndCategory(Guid workspaceId, Guid categoryId);
    void AddOrUpdate(BudgetLimit limit);
}
