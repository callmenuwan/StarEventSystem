using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarEventSystem.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } = null!;

        [Required]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;

        public string? QrCodeBase64 { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}