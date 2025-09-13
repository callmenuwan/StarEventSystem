using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarEventSystem.Models
{
    public class TicketType
    {
        [Key]
        public int TicketTypeId { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }  // <-- add ? to make it nullable

        [Required, StringLength(100)]
        public string TypeName { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Seats { get; set; }
    }
}
