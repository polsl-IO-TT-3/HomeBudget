using HomeBudget.Models;

namespace HomeBudget.DataAccess;

public interface ICategoryRepository
{
    IEnumerable<Category> GetAll();
    Category? GetById(Guid id);
    void Add(Category category);
}
