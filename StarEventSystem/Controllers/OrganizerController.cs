using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarEventSystem.Data;
using StarEventSystem.Models;
using System.Text;

namespace StarEventSystem.Controllers
{
    [Authorize]
    public class OrganizerController : Controller
    {
        private readonly StarEventSystemContext _context;

        public OrganizerController(StarEventSystemContext context)
        {
            _context = context;
        }

        // Dashboard (with optional date filters)
        public async Task<IActionResult> Dashboard(DateTime? startDate, DateTime? endDate)
        {
            var organizerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (organizerIdClaim == null)
                return Unauthorized();

            //var query = _context.Event
            //    .Include(e => e.Orders)
            //        .ThenInclude(o => o.OrderItems)
            //    .Where(e => e.OrganizerId.ToString() == organizerIdClaim);

            var query = _context.Event
    .Include(e => e.Orders)
        .ThenInclude(o => o.OrderItems)
    .AsQueryable();

            // 🔍 Apply date filter (based on order creation date)
            if (startDate.HasValue)
                query = query.Where(e => e.Orders.Any(o => o.CreatedAt >= startDate));

            if (endDate.HasValue)
                query = query.Where(e => e.Orders.Any(o => o.CreatedAt <= endDate));

            var reports = await query
                .Select(e => new OrganizerEventReportViewModel
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    TotalTicketsSold = e.Orders.SelectMany(o => o.OrderItems).Sum(oi => (int?)oi.Quantity) ?? 0,
                    TotalRevenue = e.Orders.SelectMany(o => o.OrderItems).Sum(oi => (decimal?)(oi.Quantity * oi.Price)) ?? 0m
                })
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(reports);
        }

        // CSV Export
        [HttpGet]
        public async Task<IActionResult> ExportCsv(DateTime? startDate, DateTime? endDate)
        {
            var organizerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (organizerIdClaim == null)
                return Unauthorized();

            //var query = _context.Event
            //    .Include(e => e.Orders)
            //        .ThenInclude(o => o.OrderItems)
            //    .Where(e => e.OrganizerId.ToString() == organizerIdClaim);

            var query = _context.Event
    .Include(e => e.Orders)
        .ThenInclude(o => o.OrderItems)
    .AsQueryable();


            if (startDate.HasValue)
                query = query.Where(e => e.Orders.Any(o => o.CreatedAt >= startDate));
            if (endDate.HasValue)
                query = query.Where(e => e.Orders.Any(o => o.CreatedAt <= endDate));

            var data = await query
                .Select(e => new
                {
                    e.EventName,
                    TicketsSold = e.Orders.SelectMany(o => o.OrderItems).Sum(oi => (int?)oi.Quantity) ?? 0,
                    Revenue = e.Orders.SelectMany(o => o.OrderItems).Sum(oi => (decimal?)(oi.Quantity * oi.Price)) ?? 0m
                })
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Event Name,Tickets Sold,Revenue (USD)");
            foreach (var row in data)
                csv.AppendLine($"{row.EventName},{row.TicketsSold},{row.Revenue}");

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "EventReport.csv");
        }
    }
}
