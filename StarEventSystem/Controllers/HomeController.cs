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
        public async Task<IActionResult> Index()
        {
            var eventsList = await _context.Event.ToListAsync();
            return View(eventsList); // ✅ pass events to the view
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