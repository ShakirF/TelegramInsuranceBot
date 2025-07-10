using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.DbContext
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserState> UserStates => Set<UserState>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<ExtractedField> ExtractedFields => Set<ExtractedField>();
    }
}
