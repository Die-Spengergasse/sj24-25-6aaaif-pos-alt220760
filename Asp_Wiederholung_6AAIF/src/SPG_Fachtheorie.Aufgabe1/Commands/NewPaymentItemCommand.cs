using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public class NewPaymentItemCommand
    {
        [Required]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid article name")]
        public string ArticleName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Amount must be at least 1")]
        public int Amount { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid payment ID")]
        public int PaymentId { get; set; }
    }
}
