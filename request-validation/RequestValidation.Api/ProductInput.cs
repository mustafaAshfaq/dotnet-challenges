using System.ComponentModel.DataAnnotations;
namespace RequestValidation.Api;

public sealed record ProductInput(string Name, int Quantity):IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Name is required.", new[] { nameof(Name) });
        }

        if (Quantity <= 0 || Quantity > 100)
        {
            yield return new ValidationResult("Quantity must be greater than zero.", new[] { nameof(Quantity) });
        }
    }
}
