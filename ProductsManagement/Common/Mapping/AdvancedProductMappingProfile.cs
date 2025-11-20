using System.Runtime.InteropServices.ComTypes;
using ProductsManagement.Features.Products;
using AutoMapper;
using ProductsManagement.Features.Products.DTO;

namespace ProductsManagement.Mapping;

public class AdvancedProductMappingProfile : Profile
{
    public AdvancedProductMappingProfile()
    {
        CreateMap<CreateProductProfileRequest, Product>()
            .ForMember(d => d.Id, o => o.MapFrom(_ => Guid.NewGuid()))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.IsAvailable, o => o.MapFrom(s => s.StockQuantity > 0))
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        CreateMap<Product, ProductProfileDto>()
            .ForMember(d => d.CategoryDisplayName,
                o => o.MapFrom<CategoryDisplayResolver>())
            .ForMember(d => d.FormattedPrice,
                o => o.MapFrom<PriceFormatterResolver>())
            .ForMember(d => d.ProductAge,
                o => o.MapFrom<ProductAgeResolver>())
            .ForMember(d => d.BrandInitials,
                o => o.MapFrom<BrandInitialsResolver>())
            .ForMember(d => d.AvailabiityStatus,
                o => o.MapFrom<AvailabilityStatusResolver>())
            .ForMember(d => d.ImageUrl,
                o => o.MapFrom(src => src.Category == ProductCategory.Home
                    ? null
                    : src.ImageUrl))
            .ForMember(d => d.Price, o => o.MapFrom(src =>
                src.Category == ProductCategory.Home ? Math.Round(src.Price * 0.9m, 2) : src.Price));
    }
}