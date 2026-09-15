using AutoMapper;
using Library.Core.DTOs.Books;
using Library.Core.DTOs.Categories;
using Library.Core.DTOs.Loans;
using Library.Core.DTOs.Members;
using Library.Core.Entities;

namespace Library.Service.Mappings;

public class LibraryMappingProfile : Profile
{
    public LibraryMappingProfile()
    {
        CreateMap<Book, BookResponse>()
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable.ToString()))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<BookCreateRequest, Book>();
        CreateMap<BookUpdateRequest, Book>();

        CreateMap<Category, CategoryResponse>();
        CreateMap<CategoryRequest, Category>();

        CreateMap<Member, MemberResponse>();

        CreateMap<Loan, LoanResponse>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title))
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
