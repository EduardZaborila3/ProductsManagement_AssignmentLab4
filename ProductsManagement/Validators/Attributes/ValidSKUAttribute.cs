using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProductsManagement.Validators.Attributes;

public class ValidSKUAttribute : ValidationAttribute, IClientModelValidator
{
    public ValidSKUAttribute()
    {
        ErrorMessage = "SKU must be alphanumeric, can include hyphens, and be between 5 to 20 characters long.";
    }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;

        var sku = value.ToString();
        
        sku = sku?.Replace(" ", "");
        
        var regex = new Regex(@"^[a-zA-Z0-9\-]{5,20}$");

        if (sku != null && regex.IsMatch(sku))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage);
    }
    
    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.Add("data-val", "true");
        context.Attributes.Add("data-val-sku", ErrorMessage ?? "Invalid SKU format.");
        context.Attributes.Add("data-val-sku-pattern", "^[a-zA-Z0-9\\-]{5,20}$");
    }
}