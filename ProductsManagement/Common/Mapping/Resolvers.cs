using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using AutoMapper;
using ProductsManagement.Features.Products;

namespace ProductsManagement.Mapping;

public class CategoryDisplayResolver : IValueResolver<Product, object, string>
{
    public string Resolve(Product source, object destination, string destMember, ResolutionContext context)
    {
        return source.Category switch
        {
            ProductCategory.Electronics => "Electronics & Technology",
            ProductCategory.Clothing => "Clothing & Fashion",
            ProductCategory.Books => "Books & Media",
            ProductCategory.Home => "Home & Garden",
            _ => "Uncategorized"
        };
    }
}

public class PriceFormatterResolver : IValueResolver<Product, object, string>
{
    public string Resolve(Product source, object destination, string destMember, ResolutionContext context)
    {
        var price = source.Category == ProductCategory.Home 
            ? Math.Round(source.Price * 0.9m, 2) 
            : source.Price;
        return price.ToString("C2", CultureInfo.CurrentCulture);
    }
}

public class ProductAgeResolver : IValueResolver<Product, object, string>
{
    public string Resolve(Product source, object destination, string destMember, ResolutionContext context)
    {
        var age = DateTime.UtcNow - source.ReleaseDate;
        if (age.TotalDays < 30)
        {
            return "New Release";
        }

        if (age.TotalDays < 365)
        {
            return $"{(int)(age.TotalDays / 30)} months old";
        }

        if (age.TotalDays < 1825)
        {
            return $"{(int)(age.TotalDays / 365)} years old";
        }

        if (Math.Abs(age.TotalDays - 1825) < 1)
        {
            return "Classic";
        }

        return "Vintage";
    }
}

public class BrandInitialsResolver : IValueResolver<Product, object, string>
{
    public string Resolve(Product source, object destination, string destMember, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source.Brand)) return "?";
        var parts = source.Brand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0][0].ToString().ToUpperInvariant();
        }
        return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpperInvariant();
    }
}

public class AvailabilityStatusResolver : IValueResolver<Product, object, string>
{
    public string Resolve(Product source, object destination, string destMember, ResolutionContext context)
    {
        if (!source.IsAvailable)
        {
            return "Out of Stock";
        }

        if (source.IsAvailable && source.StockQuantity == 0)
        {
            return "Unavailable";
        }

        if (source.IsAvailable && source.StockQuantity == 1)
        {
            return "Last Item";
        }

        if (source.IsAvailable && source.StockQuantity <= 5)
        {
            return "Limited Stock";
        }   
        
        return "In Stock";
    }
}