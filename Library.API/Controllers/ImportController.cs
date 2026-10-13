using Library.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Controllers;

// TEMPORARY: one-time import of the local database into Render. Remove after use.
[ApiController]
[Route("api/admin/import")]
[Authorize(Policy = "AdminOnly")]
public class ImportController : ControllerBase
{
    private readonly AppDbContext _context;

    public ImportController(AppDbContext context)
    {
        _context = context;
    }

    public record CategoryRow(int Id, string Name);
    public record LibraryRow(int Id, string Name);
    public record MemberRow(int Id, string Name, string Email, string PasswordHash, bool IsAdmin);
    public record BookRow(int Id, string Title, string Author, string ISBN, int PublishYear, string IsAvailable, int CategoryId, int? LibraryId);
    public record LoanRow(int Id, int BookId, int MemberId, DateTime LoanDate, DateTime DueDate, DateTime? ReturnDate, string Status);

    public record ImportRequest(
        List<CategoryRow> Categories,
        List<LibraryRow> Libraries,
        List<MemberRow> Members,
        List<BookRow> Books,
        List<LoanRow> Loans);

    [HttpPost]
    public async Task<IActionResult> Import(ImportRequest data)
    {
        var db = _context.Database;
        await using var tx = await db.BeginTransactionAsync();

        await db.ExecuteSqlRawAsync("TRUNCATE \"Loans\", \"Books\", \"Members\", \"Libraries\", \"Categories\" RESTART IDENTITY CASCADE");

        foreach (var c in data.Categories)
            await db.ExecuteSqlInterpolatedAsync($"INSERT INTO \"Categories\" (\"Id\", \"Name\") OVERRIDING SYSTEM VALUE VALUES ({c.Id}, {c.Name})");

        foreach (var l in data.Libraries)
            await db.ExecuteSqlInterpolatedAsync($"INSERT INTO \"Libraries\" (\"Id\", \"Name\") OVERRIDING SYSTEM VALUE VALUES ({l.Id}, {l.Name})");

        foreach (var m in data.Members)
            await db.ExecuteSqlInterpolatedAsync($"INSERT INTO \"Members\" (\"Id\", \"Name\", \"Email\", \"PasswordHash\", \"IsAdmin\") OVERRIDING SYSTEM VALUE VALUES ({m.Id}, {m.Name}, {m.Email}, {m.PasswordHash}, {m.IsAdmin})");

        foreach (var b in data.Books)
            await db.ExecuteSqlInterpolatedAsync($"INSERT INTO \"Books\" (\"Id\", \"Title\", \"Author\", \"ISBN\", \"PublishYear\", \"IsAvailable\", \"CategoryId\", \"LibraryId\") OVERRIDING SYSTEM VALUE VALUES ({b.Id}, {b.Title}, {b.Author}, {b.ISBN}, {b.PublishYear}, {b.IsAvailable}, {b.CategoryId}, {b.LibraryId})");

        foreach (var l in data.Loans)
            await db.ExecuteSqlInterpolatedAsync($"INSERT INTO \"Loans\" (\"Id\", \"BookId\", \"MemberId\", \"LoanDate\", \"DueDate\", \"ReturnDate\", \"Status\") OVERRIDING SYSTEM VALUE VALUES ({l.Id}, {l.BookId}, {l.MemberId}, {ToUtc(l.LoanDate)}, {ToUtc(l.DueDate)}, {ToUtc(l.ReturnDate)}, {l.Status})");

        // continue auto-numbering after the highest imported Id
        foreach (var table in new[] { "Categories", "Libraries", "Members", "Books", "Loans" })
            await db.ExecuteSqlRawAsync($"SELECT setval(pg_get_serial_sequence('\"{table}\"', 'Id'), GREATEST((SELECT COALESCE(MAX(\"Id\"), 0) FROM \"{table}\"), 1), (SELECT COUNT(*) > 0 FROM \"{table}\"))");

        await tx.CommitAsync();

        return Ok(new
        {
            categories = data.Categories.Count,
            libraries = data.Libraries.Count,
            members = data.Members.Count,
            books = data.Books.Count,
            loans = data.Loans.Count
        });
    }

    private static DateTime ToUtc(DateTime d) => DateTime.SpecifyKind(d, DateTimeKind.Utc);
    private static DateTime? ToUtc(DateTime? d) => d is null ? null : ToUtc(d.Value);
}
