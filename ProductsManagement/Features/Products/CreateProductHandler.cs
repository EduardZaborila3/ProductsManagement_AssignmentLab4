using System.Diagnostics;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using ProductsManagement.Features.Products.DTO;
using ProductsManagement.Logging; 
using ProductsManagement.Persistence;

namespace ProductsManagement.Features.Products;

public class CreateProductHandler
{
    private readonly ProductManagementContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductProfileRequest> _validator;
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly IMemoryCache _cache;

    public CreateProductHandler(
        ProductManagementContext context,
        IMapper mapper,
        IValidator<CreateProductProfileRequest> validator,
        ILogger<CreateProductHandler> logger,
        IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IResult> Handle(CreateProductProfileRequest request)
    {
        var totalStopwatch = Stopwatch.StartNew();
        
        var operationId = Guid.NewGuid().ToString("N")[..8];
        
        var validationDuration = TimeSpan.Zero;
        var dbDuration = TimeSpan.Zero;
        
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationId"] = operationId,
            ["ProductSKU"] = request.SKU,
            ["ProductName"] = request.Name
        });

        _logger.LogInformation(LogEvents.ProductCreationStarted, 
            "Starting creation: Name={Name} | Brand={Brand} | SKU={SKU} | Category={Category}", 
            request.Name, request.Brand, request.SKU, request.Category);

        try
        {
            var valSw = Stopwatch.StartNew();
            
            _logger.LogInformation(LogEvents.SKUValidationPerformed, "Validating SKU uniqueness and format...");

            _logger.LogInformation(LogEvents.StockValidationPerformed, "Validating stock limits...");

            var validationResult = await _validator.ValidateAsync(request);
            
            valSw.Stop();
            validationDuration = valSw.Elapsed;

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(LogEvents.ProductValidationFailed, 
                    "Validation failed for SKU {SKU}. Errors: {Errors}", 
                    request.SKU, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                
                LogMetrics(operationId, request, validationDuration, dbDuration, totalStopwatch.Elapsed, false, "Validation Failed");

                return Results.BadRequest(validationResult.Errors);
            }

            var product = _mapper.Map<Product>(request);

            var dbSw = Stopwatch.StartNew();
            
            _logger.LogInformation(LogEvents.DatabaseOperationStarted, "Saving product to database...");

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            dbSw.Stop();
            dbDuration = dbSw.Elapsed;

            _logger.LogInformation(LogEvents.DatabaseOperationCompleted, 
                "Database save completed. New ProductId: {Id}", product.Id);

            _cache.Remove("all_products");
            
            _logger.LogInformation(LogEvents.CacheOperationPerformed, "Cache invalidated for key 'all_products'");

            var productDto = _mapper.Map<ProductProfileDto>(product);

            totalStopwatch.Stop();

            LogMetrics(operationId, request, validationDuration, dbDuration, totalStopwatch.Elapsed, true, null);

            return Results.Created($"/products/{product.Id}", productDto);
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();
            
            _logger.LogError(ex, "Unexpected error creating product {SKU}", request.SKU);
            
            LogMetrics(operationId, request, validationDuration, dbDuration, totalStopwatch.Elapsed, false, ex.Message);
            
            throw; 
        }
    }

    private void LogMetrics(
        string opId, 
        CreateProductProfileRequest req, 
        TimeSpan valTime, 
        TimeSpan dbTime, 
        TimeSpan totalTime, 
        bool success, 
        string? error)
    {
        var metrics = new ProductCreationMetrics(
            OperationId: opId,
            ProductName: req.Name,
            SKU: req.SKU,
            Category: req.Category,
            ValidationDuration: valTime,
            DatabaseSaveDuration: dbTime,
            TotalDuration: totalTime,
            Success: success,
            ErrorReason: error
        );

        _logger.LogProductCreationMetrics(metrics);
    }
}