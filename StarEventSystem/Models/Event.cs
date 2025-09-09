using System.ComponentModel.DataAnnotations;

namespace StarEventSystem.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(200)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string EventDescription { get; set; } = string.Empty;

        [Required]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public decimal TicketPrice { get; set; }

        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        // Temporary until Identity Roles come
        public int OrganizerId { get; set; }

        // store image file name/path
        public string? ImagePath { get; set; }

    }
}
