namespace ProductsManagement.Features.Products.DTO;

public class ProductProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CategoryDisplayName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string FormattedPrice { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ImageUrl  { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }
    public string ProductAge { get; set; } = string.Empty;
    public string BrandInitials { get; set; } = string.Empty;
    public string AvailabiityStatus { get; set; } = string.Empty;
}