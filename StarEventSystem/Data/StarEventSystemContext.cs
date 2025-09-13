using Microsoft.EntityFrameworkCore;
using StarEventSystem.Models;

namespace StarEventSystem.Data
{
    public class StarEventSystemContext : DbContext
    {
        public StarEventSystemContext(DbContextOptions<StarEventSystemContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Event { get; set; } = default!;
        public DbSet<TicketType> TicketTypes { get; set; } = default!;
        public DbSet<Customer> Customers { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderItem> OrderItems { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TicketType -> Event
            modelBuilder.Entity<TicketType>()
                .HasOne(tt => tt.Event)
                .WithMany(e => e.TicketTypes)
                .HasForeignKey(tt => tt.EventId)
                .OnDelete(DeleteBehavior.Restrict); // important

            // Order -> Event
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Event)
                .WithMany(e => e.Orders)
                .HasForeignKey(o => o.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> Customer
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem -> Order
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem -> TicketType
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.TicketType)
                .WithMany(tt => tt.OrderItems)
                .HasForeignKey(oi => oi.TicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision
            modelBuilder.Entity<TicketType>().Property(tt => tt.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>().Property(oi => oi.Price).HasPrecision(18, 2);
        }

    }
}
