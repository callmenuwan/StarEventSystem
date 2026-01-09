using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarEventSystem.Data;
using StarEventSystem.Models;
using System.Text;

namespace StarEventSystem.Controllers
{
    [Authorize] // Only admin users can access [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly StarEventSystemContext _context;

        public AdminController(StarEventSystemContext context)
        {
            _context = context;
        }

        

        // Dashboard Summary
        public async Task<IActionResult> Dashboard(DateTime? startDate, DateTime? endDate)
        {
            var orders = _context.Orders.Include(o => o.OrderItems).Include(o => o.Event).AsQueryable();

            if (startDate.HasValue)
                orders = orders.Where(o => o.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                orders = orders.Where(o => o.CreatedAt <= endDate.Value);

            var totalRevenue = await orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
            var totalTickets = await orders.SelectMany(o => o.OrderItems).SumAsync(oi => (int?)oi.Quantity) ?? 0;
            var totalEvents = await _context.Event.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            // Revenue by Event for Chart
            var revenueByEvent = await orders
                .GroupBy(o => o.Event.EventName)
                .Select(g => new
                {
                    EventName = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount)
                }).ToListAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalTickets = totalTickets;
            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.RevenueByEvent = revenueByEvent;

            // Revenue by Organizer
            var organizerRevenue = _context.Orders
                .Include(o => o.Event)
                .GroupBy(o => o.Event.OrganizerId)
                .Select(g => new { Organizer = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .ToList();

            ViewBag.RevenueByOrganizerLabels = organizerRevenue.Select(x => x.Organizer).ToList();
            ViewBag.RevenueByOrganizerValues = organizerRevenue.Select(x => x.Total).ToList();

            // Revenue by Month
            var monthlyRevenue = _context.Orders
                .AsEnumerable() // switch to client-side processing
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:00}",
                    Total = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToList();

            ViewBag.RevenueByMonthLabels = monthlyRevenue.Select(x => x.Month).ToList();
            ViewBag.RevenueByMonthValues = monthlyRevenue.Select(x => x.Total).ToList();

            // Revenue by Location (if Event has a Location property)
            var locationRevenue = _context.Orders
                .Include(o => o.Event)
                .GroupBy(o => o.Event.Location)
                .Select(g => new { Location = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .ToList();

            ViewBag.RevenueByLocationLabels = locationRevenue.Select(x => x.Location).ToList();
            ViewBag.RevenueByLocationValues = locationRevenue.Select(x => x.Total).ToList();

            return View();
        }

        // Export CSV
        [HttpGet]
        public async Task<IActionResult> ExportCsv(DateTime? startDate, DateTime? endDate)
        {
            var orders = _context.Orders.Include(o => o.Event).AsQueryable();

            if (startDate.HasValue)
                orders = orders.Where(o => o.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                orders = orders.Where(o => o.CreatedAt <= endDate.Value);

            var data = await orders
                .GroupBy(o => o.Event.EventName)
                .Select(g => new
                {
                    EventName = g.Key,
                    TicketsSold = g.SelectMany(x => x.OrderItems).Sum(i => i.Quantity),
                    Revenue = g.Sum(x => x.TotalAmount)
                })
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Event Name,Tickets Sold,Revenue (USD)");
            foreach (var row in data)
                csv.AppendLine($"{row.EventName},{row.TicketsSold},{row.Revenue}");

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "AdminSystemReport.csv");
        }
    }
}
