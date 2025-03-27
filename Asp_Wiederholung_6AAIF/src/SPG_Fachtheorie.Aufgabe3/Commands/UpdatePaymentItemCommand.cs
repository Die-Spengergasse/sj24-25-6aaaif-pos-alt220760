using System;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Dtos
{
    public class UpdatePaymentItemCommand
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ArticleName is required.")]
        public string ArticleName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public int Amount { get; set; }


        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PaymentId must be greater than 0.")]
        public int PaymentId { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
