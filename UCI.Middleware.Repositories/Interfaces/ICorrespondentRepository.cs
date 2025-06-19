using UCI.Middleware.Entities.Entities.Ivass;

namespace UCI.Middleware.Repositories.Interfaces;

public interface ICorrespondentRepository : IRepository<Correspondent>
{
    Task<Correspondent?> GetByCodeAsync(string code);
    Task<Correspondent?> GetByUciCodeAsync(string uciCode);
    Task<IEnumerable<Correspondent>> GetByConventionalNameAsync(string name);
    Task<IEnumerable<Correspondent>> GetActiveCorrespondentsAsync();
    Task<IEnumerable<Correspondent>> GetNotificationEnabledAsync();
}