using Domain.Entities;
using Domain.Interfaces;
using Persistence.DbContext;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IRepository<User> Users { get; }
        public IRepository<UserState> UserStates { get; }
        public IRepository<Document> Documents { get; }
        public IRepository<ExtractedField> ExtractedFields { get; }
        public IRepository<Policy> Policies { get; }
        public IRepository<PolicyEvent> PolicyEvents { get; }
        public IRepository<AuditLog> AuditLogs { get; }
        public IRepository<Conversation> Conversations { get; }
        public IRepository<Error> Errors { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new Repository<User>(context);
            UserStates = new Repository<UserState>(context);
            Documents = new Repository<Document>(context);
            ExtractedFields = new Repository<ExtractedField>(context);
            Policies = new Repository<Policy>(context);
            PolicyEvents = new Repository<PolicyEvent>(context);
            AuditLogs = new Repository<AuditLog>(context);
            Conversations = new Repository<Conversation>(context);
            Errors = new Repository<Error>(context);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
