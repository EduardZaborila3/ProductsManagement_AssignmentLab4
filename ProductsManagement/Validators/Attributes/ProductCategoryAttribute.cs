using System.ComponentModel.DataAnnotations;
using ProductsManagement.Features.Products;

namespace ProductsManagement.Validators.Attributes;

public class ProductCategoryAttribute : ValidationAttribute
{
    private readonly ProductCategory[] _allowedCategories;
    
    public ProductCategoryAttribute(params ProductCategory[] allowedCategories)
    {
        _allowedCategories = allowedCategories;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is ProductCategory category)
        {
            if (_allowedCategories.Contains(category))
            {
                return ValidationResult.Success;
            }
        }

        var allowedList = string.Join(", ", _allowedCategories);
        return new ValidationResult($"Category not allowed. Allowed: {allowedList}");
    }
}