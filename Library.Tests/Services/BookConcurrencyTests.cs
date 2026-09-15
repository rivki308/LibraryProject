using Library.Core.Entities;
using Library.Core.Enums;
using Library.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Library.Tests.Services;

public class BookConcurrencyTests
{
    [Fact]
    public async Task TwoContexts_UpdateSameBook_SecondContextThrowsDbUpdateConcurrencyException()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        int bookId;
        await using (var seedContext = new AppDbContext(options))
        {
            var category = new Category { Name = "General" };
            var book = new Book
            {
                Title = "Concurrency Book",
                Author = "Author",
                ISBN = "1111111111",
                Category = category,
                IsAvailable = BookStatus.Available,
                RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }
            };

            seedContext.Books.Add(book);
            await seedContext.SaveChangesAsync();
            bookId = book.Id;
        }

        await using var context1 = new AppDbContext(options);
        await using var context2 = new AppDbContext(options);

        var book1 = await context1.Books.FirstAsync(b => b.Id == bookId);
        var book2 = await context2.Books.FirstAsync(b => b.Id == bookId);

        // Both users try to loan the same book
        book1.IsAvailable = BookStatus.Loaned;
        book1.RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 2 };

        book2.IsAvailable = BookStatus.Loaned;

        // Context 1 saves first - succeeds
        await context1.SaveChangesAsync();

        // Context 2 still holds the original RowVersion - must fail
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context2.SaveChangesAsync());
    }
}
