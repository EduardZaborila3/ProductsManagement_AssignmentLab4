using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ProductsManagement.Features.Products;
using ProductsManagement.Features.Products.DTO;
using ProductsManagement.Mapping;
using ProductsManagement.Persistence;
using ProductsManagement.Validators;
using Xunit;
namespace ProductsManagement.Tests;

public class CreateProductHandlerIntegrationTests : IDisposable
{
    private readonly ProductManagementContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<CreateProductHandler>> _loggerMock;
    private readonly Mock<ILogger<CreateProductProfileValidator>> _validatorLoggerMock;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ProductManagementContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // 
            .Options;
        _context = new ProductManagementContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AdvancedProductMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _cache = new MemoryCache(new MemoryCacheOptions());

        _loggerMock = new Mock<ILogger<CreateProductHandler>>();
        _validatorLoggerMock = new Mock<ILogger<CreateProductProfileValidator>>();

        var validator = new CreateProductProfileValidator(_context, _validatorLoggerMock.Object);

        _handler = new CreateProductHandler(
            _context,               
            _mapper,                
            validator,             
            _loggerMock.Object,    
            _cache
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _cache.Dispose();
    }

    [Fact]
    public async Task Handle_ValidElectronicsProductRequest_CreatesProductWithCorrectMappings()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Ultra Smart Watch Pro",
            Brand = "TechGiant",
            SKU = "SMART-WATCH-001",
            Category = ProductCategory.Electronics,
            Price = 150.00m,
            ReleaseDate = DateTime.UtcNow.AddDays(-29),
            StockQuantity = 15,
            ImageUrl = "https://example.com/watch.png"
        };

        var result = await _handler.Handle(request);

        var createdResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Created<ProductProfileDto>>(result);
        var dto = createdResult.Value;

        Assert.NotNull(dto);
        Assert.Equal("Electronics & Technology", dto.CategoryDisplayName);
        
        Assert.Equal("T", dto.BrandInitials); 

        Assert.Equal("New Release", dto.ProductAge);

        Assert.StartsWith("$", dto.FormattedPrice);

        Assert.Equal("In Stock", dto.AvailabiityStatus);

        VerifyLogger(2001, LogLevel.Information);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ThrowsValidationExceptionWithLogging()
    {
        var existingProduct = new Product
        {
            Name = "Existing Product",
            Brand = "BrandA",
            SKU = "DUPLICATE-SKU",
            Category = ProductCategory.Clothing,
            CreatedAt = DateTime.UtcNow
        };
        _context.Products.Add(existingProduct);
        await _context.SaveChangesAsync();

        var request = new CreateProductProfileRequest
        {
            Name = "New Product",
            Brand = "BrandB",
            SKU = "DUPLICATE-SKU", // Duplicate
            Category = ProductCategory.Clothing,
            Price = 50,
            StockQuantity = 10
        };

        var result = await _handler.Handle(request);

        var badRequestResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.BadRequest<List<FluentValidation.Results.ValidationFailure>>>(result);
        var errors = badRequestResult.Value;

        Assert.NotNull(errors);

        Assert.Contains(errors, e => e.ErrorMessage.Contains("already exists"));

        VerifyLogger(2002, LogLevel.Warning);
    }

    [Fact]
    public async Task Handle_HomeProductRequest_AppliesDiscountAndConditionalMapping()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Garden Chair",
            Brand = "HomeStyle",
            SKU = "HOME-CHAIR-001",
            Category = ProductCategory.Home,
            Price = 100.00m,
            StockQuantity = 50,
            ImageUrl = "https://example.com/chair.jpg",
            ReleaseDate = DateTime.UtcNow.AddYears(-1)
        };

        var result = await _handler.Handle(request);

        var createdResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Created<ProductProfileDto>>(result);
        var dto = createdResult.Value;

        Assert.NotNull(dto);

        Assert.Equal("Home & Garden", dto.CategoryDisplayName);

        Assert.Null(dto.ImageUrl);

        Assert.Equal(90.00m, dto.Price);
    }

    private void VerifyLogger(int eventId, LogLevel level)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.Is<EventId>(e => e.Id == eventId),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}