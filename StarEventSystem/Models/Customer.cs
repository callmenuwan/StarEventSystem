using System.ComponentModel.DataAnnotations;

namespace StarEventSystem.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        public int Points { get; set; } = 0;  // ✅ new column for loyalty points

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
