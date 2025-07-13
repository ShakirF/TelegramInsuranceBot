using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<User> Users { get; }
        IRepository<UserState> UserStates { get; }
        IRepository<Document> Documents { get; }
        IRepository<ExtractedField> ExtractedFields { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
