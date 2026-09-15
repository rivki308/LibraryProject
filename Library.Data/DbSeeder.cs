using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Security;
using Microsoft.EntityFrameworkCore;
using LibraryEntity = Library.Core.Entities.Library;

namespace Library.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var fiction = new Category { Name = "ספרות יפה" };
        var scienceFiction = new Category { Name = "מדע בדיוני" };
        var history = new Category { Name = "היסטוריה" };

        await context.Categories.AddRangeAsync(new[] { fiction, scienceFiction, history }, cancellationToken);

        var mainLibrary = new LibraryEntity { Name = "הספרייה המרכזית" };
        await context.Libraries.AddAsync(mainLibrary, cancellationToken);

        var books = new[]
        {
            new Book { Title = "אלף לילה ולילה", Author = "לא ידוע", ISBN = "9780000000001", PublishYear = 1850, Category = fiction, IsAvailable = BookStatus.Available },
            new Book { Title = "מסע בין כוכבים", Author = "דמי כהן", ISBN = "9780000000002", PublishYear = 2005, Category = scienceFiction, IsAvailable = BookStatus.Available },
            new Book { Title = "תולדות העם", Author = "דנה לוי", ISBN = "9780000000003", PublishYear = 1998, Category = history, IsAvailable = BookStatus.Available },
            new Book { Title = "עולם חדש ומופלא", Author = "יואב שמש", ISBN = "9780000000004", PublishYear = 2012, Category = scienceFiction, IsAvailable = BookStatus.Available },
            new Book { Title = "ימי הביניים", Author = "רותם אברהם", ISBN = "9780000000005", PublishYear = 1975, Category = history, IsAvailable = BookStatus.Available }
        };

        await context.Books.AddRangeAsync(books, cancellationToken);

        var adminMember = new Member
        {
            Name = "מנהל המערכת",
            Email = "admin@library.demo",
            PasswordHash = PasswordHasher.Hash("Admin123!"),
            IsAdmin = true
        };

        var regularMember = new Member
        {
            Name = "מנוי לדוגמה",
            Email = "member@library.demo",
            PasswordHash = PasswordHasher.Hash("Member123!"),
            IsAdmin = false
        };

        await context.Members.AddRangeAsync(new[] { adminMember, regularMember }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
