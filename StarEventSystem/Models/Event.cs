using System.ComponentModel.DataAnnotations;

namespace StarEventSystem.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required, StringLength(200)]
        public string EventName { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string EventDescription { get; set; } = string.Empty;

        [Required]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public int OrganizerId { get; set; }

        public string? ImagePath { get; set; }

        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
