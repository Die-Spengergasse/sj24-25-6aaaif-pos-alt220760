using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record UpdateConfirmedCommand(DateTime Confirmed) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var maxAllowedTime = DateTime.UtcNow.AddMinutes(1);
            if (Confirmed > maxAllowedTime)
            {
                yield return new ValidationResult(
                    $"Confirmed date cannot be more than 1 minute in the future.",
                    new[] { nameof(Confirmed) });
            }
        }
    }
}
