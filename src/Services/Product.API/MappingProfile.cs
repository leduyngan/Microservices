using AutoMapper;
// using Infrastructure.Extensions;
using Infrastructure.Extensions;
using Product.API.Entities;
using Shared.DTOs.Product;

namespace Product.API;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CatalogProduct, ProductDto>();
        CreateMap<CreateProductDto, CatalogProduct>();
        CreateMap<UpdateProductDto, CatalogProduct>()
        .IgnoreAllNonExisting();
    }
}