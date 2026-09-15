using AutoMapper;
using Library.Core.DTOs.Loans;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;
using Library.Service.Implementations;
using Library.Service.Mappings;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Library.Tests.Services;

public class LoanServiceTests
{
    private readonly Mock<ILoanRepository> _loanRepositoryMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly IMapper _mapper;
    private readonly LoanService _sut;

    public LoanServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<LibraryMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
        _sut = new LoanService(_loanRepositoryMock.Object, _bookRepositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task CreateAsync_BookIsAvailable_LoanIsCreatedAndBookMarkedAsLoaned()
    {
        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Author = "Author",
            ISBN = "1234567890",
            Category = new Category { Id = 1, Name = "General" },
            IsAvailable = BookStatus.Available
        };

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _bookRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _loanRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Loan
            {
                Id = 10,
                BookId = book.Id,
                Book = book,
                MemberId = 5,
                Member = new Member { Id = 5, Name = "Member", Email = "m@test.com" },
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                Status = LoanStatus.Active
            });

        var request = new LoanCreateRequest { BookId = book.Id };

        var result = await _sut.CreateAsync(memberId: 5, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(BookStatus.Loaned, book.IsAvailable);
        _bookRepositoryMock.Verify(r => r.Update(book), Times.Once);
        _loanRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_BookAlreadyLoaned_ThrowsInvalidOperationException()
    {
        var book = new Book
        {
            Id = 2,
            Title = "Unavailable Book",
            Author = "Author",
            ISBN = "0987654321",
            Category = new Category { Id = 1, Name = "General" },
            IsAvailable = BookStatus.Loaned
        };

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var request = new LoanCreateRequest { BookId = book.Id };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateAsync(memberId: 5, request, CancellationToken.None));

        _loanRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
