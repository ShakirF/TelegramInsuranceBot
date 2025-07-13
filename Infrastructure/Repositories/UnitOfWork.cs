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

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new Repository<User>(context);
            UserStates = new Repository<UserState>(context);
            Documents = new Repository<Document>(context);
            ExtractedFields = new Repository<ExtractedField>(context);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
