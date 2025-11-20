using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ProductsManagement.Validators.Attributes;

public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _min;
    private readonly decimal _max;

    public PriceRangeAttribute(double min, double max)
    {
        _min = (decimal)min;
        _max = (decimal)max;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is decimal price)
        {
            if (price >= _min && price <= _max)
            {
                return ValidationResult.Success;
            }
        }
        
        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(CultureInfo.CurrentCulture,
            "Price must be between {0} and {1}.",
            _min.ToString("C2"), _max.ToString("C2"));
    }
}