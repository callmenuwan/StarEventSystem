using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarEventSystem.Data;
using StarEventSystem.Models;

namespace StarEventSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StarEventSystemContext _context; // ✅ add DB context

        public HomeController(ILogger<HomeController> logger, StarEventSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /
        public async Task<IActionResult> Index(string type, string location, DateTime? startDate, DateTime? endDate)
        {
            var events = _context.Event.AsQueryable(); // 👈 use Event, not Events

            // Filter by type
            if (!string.IsNullOrEmpty(type))
            {
                events = events.Where(e => e.EventType == type);
            }

            // Filter by location
            if (!string.IsNullOrEmpty(location))
            {
                events = events.Where(e => e.Location.Contains(location));
            }

            // Filter by date range (overlap check)
            if (startDate.HasValue && endDate.HasValue)
            {
                events = events.Where(e => e.StartDate <= endDate && e.EndDate >= startDate);
            }
            else if (startDate.HasValue)
            {
                events = events.Where(e => e.EndDate >= startDate);
            }
            else if (endDate.HasValue)
            {
                events = events.Where(e => e.StartDate <= endDate);
            }

            // Populate event type dropdown
            var eventTypes = await _context.Event
                .Select(e => e.EventType)
                .Distinct()
                .ToListAsync();

            ViewData["EventTypes"] = eventTypes;

            // Keep current filter values
            ViewData["CurrentType"] = type;
            ViewData["CurrentLocation"] = location;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(await events.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // GET: /Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventDetails = await _context.Event
                .Include(e => e.TicketTypes)   // ✅ load ticket types with event
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventDetails == null)
            {
                return NotFound();
            }

            return View(eventDetails);
        }
    }
}