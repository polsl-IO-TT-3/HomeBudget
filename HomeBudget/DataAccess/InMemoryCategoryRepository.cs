using HomeBudget.Models;

namespace HomeBudget.DataAccess;

public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly InMemoryDataStore _store;

    public InMemoryCategoryRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public IEnumerable<Category> GetAll() => _store.Categories.ToList();

    public Category? GetById(Guid id) => _store.Categories.FirstOrDefault(c => c.Id == id);

    public void Add(Category category) => _store.Categories.Add(category);
}
