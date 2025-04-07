using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record NewPaymentCommand(
        [Range(1, int.MaxValue, ErrorMessage = "Invalid cash desk number")]
        int CashDeskNumber,
        string PaymentType,
        [Range(1, int.MaxValue, ErrorMessage = "Invalid employee registration number")]
        int EmployeeRegistrationNumber) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(PaymentType))
                yield return new ValidationResult(
                    "Payment type is required",
                    new string[] { nameof(PaymentType) });
        }
    }
}
