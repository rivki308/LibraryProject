using Library.Core.Entities;
using Microsoft.EntityFrameworkCore;
using LibraryEntity = Library.Core.Entities.Library;

namespace Library.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; } = default!;
    public DbSet<LibraryEntity> Libraries { get; set; } = default!;
    public DbSet<Member> Members { get; set; } = default!;
    public DbSet<Loan> Loans { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
