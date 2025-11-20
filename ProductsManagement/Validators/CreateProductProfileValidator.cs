using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductsManagement.Features.Products.DTO;
using ProductsManagement.Persistence;           
using ProductsManagement.Features.Products;    

namespace ProductsManagement.Validators;

public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
{
    private readonly ProductManagementContext _context;
    private readonly ILogger<CreateProductProfileValidator> _logger;
    
    public CreateProductProfileValidator(
        ProductManagementContext context,
        ILogger<CreateProductProfileValidator> logger)
    {
        _context = context;
        _logger = logger;
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .Must(BeValidName).WithMessage("Name contains inappropriate content");
        
        RuleFor(x => x.Brand)
            .NotEmpty()
            .Length(2, 100)
            .Must(BeValidBrandName).WithMessage("Brand contains invalid characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(10000);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .LessThan(100000);
        
        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Release date cannot be in the future")
            .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("Release date cannot be before year 1900");
        
        RuleFor(x => x.ImageUrl)
            .Must(BeValidImageUrl)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("ImageUrl must be a valid HTTP/HTTPS URL ending with an image extension (.jpg, .png, etc.)");
        
        RuleFor(x => x.SKU)
            .NotEmpty()
            .Must(BeValidSKU).WithMessage("SKU must be alphanumeric with hyphens")
            .MustAsync(BeUniqueSKU).WithMessage("SKU already exists in the system");
        
        RuleFor(x => x)
            .MustAsync(BeUniqueName)
            .WithMessage("This product name already exists for this brand")
            .OverridePropertyName("Name");
        
        When(x => x.Category == ProductCategory.Electronics, () =>
        {
            RuleFor(x => x.Price).GreaterThanOrEqualTo(50).WithMessage("Electronics must cost at least $50.00"); 
            RuleFor(x => x.Name).Must(ContainTechnologyKeywords).WithMessage("Electronics name must contain technology keywords");
            RuleFor(x => x.ReleaseDate).Must(date => date >= DateTime.UtcNow.AddYears(-5)).WithMessage("Electronics cannot be older than 5 years");
        });
        
        When(x => x.Category == ProductCategory.Home, () =>
        {
            RuleFor(x => x.Price).LessThanOrEqualTo(200).WithMessage("Home products cannot exceed $200.00");
            RuleFor(x => x.Name).Must(BeAppropriateForHome).WithMessage("Name not appropriate for Home category");
        });

        RuleFor(x => x)
            .Must(x => !(x.Price > 100 && x.StockQuantity > 20))
            .WithMessage("High-value products are limited to 20 stock units")
            .OverridePropertyName("StockQuantity");
        
        RuleFor(x => x)
            .MustAsync(PassBusinessRules)
            .WithMessage("Daily product creation limit (500) reached");
    }
    
    private bool BeValidName(string name)
    {
        var badWords = new[] { 
            "scam", "fraud", "fake", "replica", "counterfeit", 
            "illegal", "banned", "stolen", "pirated", "warez",
            "xxx", "adult", "porn", "nsfw", "nude",
            "hate", "racist", "kill", "murder", "weapon" 
        };
        return !badWords.Any(w => name.ToLower().Contains(w));
    }

    private async Task<bool> BeUniqueName(CreateProductProfileRequest request, CancellationToken token)
    {
        _logger.LogInformation("Checking uniqueness for Name '{Name}' and Brand '{Brand}'", request.Name, request.Brand);
        
        return !await _context.Products.AnyAsync(
            p => p.Brand == request.Brand && p.Name == request.Name, token);
    }

    private bool BeValidBrandName(string brand)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(brand, @"^[a-zA-Z0-9\s\-'.]+$");
    }
    private bool BeValidSKU(string sku)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[A-Za-z0-9\-]+$");
    }

    private async Task<bool> BeUniqueSKU(string sku, CancellationToken token)
    {
        _logger.LogInformation("Validating uniqueness for SKU: {SKU}", sku);
        return !await _context.Products.AnyAsync(p => p.SKU == sku, token);
    }

    private bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        
        bool isUrl = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        if (!isUrl) return false;
        
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        return validExtensions.Any(ext => url.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> PassBusinessRules(CreateProductProfileRequest request, CancellationToken token)
    {
        var today = DateTime.UtcNow.Date;
        var count = await _context.Products.CountAsync(p => p.CreatedAt >= today, token);
        
        if (count >= 500)
        {
            _logger.LogWarning("Daily product limit reached. Current count: {Count}", count);
            return false;
        }
        return true;
    }
    
    private bool ContainTechnologyKeywords(string name)
    {
        var techWords = new[] {
            "Smart", "Digital", "Electric", "Pro", "Tech", 
            "Wireless", "Bluetooth", "WiFi", "Connect", "5G",
            "Ultra", "Turbo", "Power", "Hyper", "Mega", "Giga",
            "Cyber", "Nano", "Quantum", "AI", "Intelligent", "Robot",
            "Sonic", "Laser", "Vision", "HD", "Pixel" 
        };
        return techWords.Any(w => name.Contains(w, StringComparison.OrdinalIgnoreCase));
    }

    private bool BeAppropriateForHome(string name)
    {
        var inappropriateForHome = new[] 
        { 
            "Industrial", "Toxic", "Hazard", 
            "Commercial", "Factory", "Warehouse", "Bulk", "Heavy-Duty",
            "Poison", "Lethal", "Explosive", "Radioactive", "Biohazard", 
            "Corrosive", "Acid", "Flammable", "Combustible",
            "High-Voltage", "Asbestos", "Medical", "Surgical", "Waste"
        };
        return !inappropriateForHome.Any(w => name.Contains(w, StringComparison.OrdinalIgnoreCase));
    }
}