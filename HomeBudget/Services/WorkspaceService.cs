using HomeBudget.DataAccess;
using HomeBudget.Models;

namespace HomeBudget.Services;

public class WorkspaceService
{
    private readonly IWorkspaceRepository _repository;
    private readonly AuthService _authService;

    public WorkspaceService(IWorkspaceRepository repository, AuthService authService)
    {
        _repository = repository;
        _authService = authService;
    }

    public IEnumerable<Workspace> GetAllWorkspaces() => _repository.GetAll();

    public Workspace CreateWorkspace(string name, string? password)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Podaj nazwę gospodarstwa.");

        var trimmedName = name.Trim();
        if (_repository.GetAll().Any(w => w.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Gospodarstwo o takiej nazwie już istnieje.");

        var workspace = new Workspace
        {
            Name = trimmedName,
            PasswordHash = string.IsNullOrWhiteSpace(password)
                ? null
                : _authService.HashPassword(password)
        };

        _repository.Add(workspace);
        return workspace;
    }

    public bool Authenticate(Workspace workspace, string? password) =>
        _authService.VerifyPassword(workspace, password);
}
