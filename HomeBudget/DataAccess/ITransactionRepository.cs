using HomeBudget.Models;

namespace HomeBudget.DataAccess;

public interface ITransactionRepository
{
    IEnumerable<Transaction> GetByWorkspace(Guid workspaceId);
    Transaction? GetById(Guid id);
    void Add(Transaction transaction);
    void Update(Transaction transaction);
    void Delete(Guid id);
}
