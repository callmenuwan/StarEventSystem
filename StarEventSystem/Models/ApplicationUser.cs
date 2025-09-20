using Microsoft.AspNetCore.Identity;

namespace StarEventSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int Points { get; set; } = 0;  // Loyalty points
        public string FullName { get; set; } = string.Empty;  // Optional extra field
    }
}
