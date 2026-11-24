using HomeBudget.Models;

namespace HomeBudget.DataAccess;

public class InMemoryWorkspaceRepository : IWorkspaceRepository
{
    private readonly InMemoryDataStore _store;

    public InMemoryWorkspaceRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public IEnumerable<Workspace> GetAll() => _store.Workspaces.ToList();

    public Workspace? GetById(Guid id) => _store.Workspaces.FirstOrDefault(w => w.Id == id);

    public void Add(Workspace workspace) => _store.Workspaces.Add(workspace);
}
