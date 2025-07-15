using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<User> Users { get; }
        IRepository<UserState> UserStates { get; }
        IRepository<Document> Documents { get; }
        IRepository<ExtractedField> ExtractedFields { get; }
        IRepository<Policy> Policies { get; }
        IRepository<PolicyEvent> PolicyEvents { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<Conversation> Conversations { get; }
        IRepository<Error> Errors { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
