using HomeBudget.Models;

namespace HomeBudget.DataAccess;

public interface IWorkspaceRepository
{
    IEnumerable<Workspace> GetAll();
    Workspace? GetById(Guid id);
    void Add(Workspace workspace);
}
