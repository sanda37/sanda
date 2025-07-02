using System;
using System.ComponentModel.DataAnnotations;
using sanda.Models;

public class RequireIfMobilityDisability : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = (SignUpRequest)validationContext.ObjectInstance;

        if (instance.HasMobilityDisability && string.IsNullOrEmpty(value?.ToString()))
        {
            return new ValidationResult("DisabilityProofPath is required when HasMobilityDisability is true.");
        }

        return ValidationResult.Success;
    }
}