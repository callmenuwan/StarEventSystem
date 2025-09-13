using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarEventSystem.Data;
using StarEventSystem.Models;

namespace StarEventSystem.Controllers
{
    public class OrdersController : Controller
    {
        private readonly StarEventSystemContext _context;

        public OrdersController(StarEventSystemContext context)
        {
            _context = context;
        }

        // POST: Orders/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(int EventId, Dictionary<int, int> Quantities)
        {
            var eventDetails = await _context.Event
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.EventId == EventId);

            if (eventDetails == null)
            {
                return NotFound();
            }

            // Filter out zero-quantity selections
            var selectedTickets = eventDetails.TicketTypes
                .Where(t => Quantities.ContainsKey(t.TicketTypeId) && Quantities[t.TicketTypeId] > 0)
                .Select(t => new
                {
                    TicketType = t,
                    Quantity = Quantities[t.TicketTypeId],
                    Subtotal = t.Price * Quantities[t.TicketTypeId]
                }).ToList();

            if (!selectedTickets.Any())
            {
                TempData["Error"] = "Please select at least one ticket.";
                return RedirectToAction("Details", "Home", new { id = EventId });
            }

            var total = selectedTickets.Sum(t => t.Subtotal);

            // Pass data to view using ViewBag
            ViewBag.SelectedTickets = selectedTickets;
            ViewBag.Total = total;
            ViewBag.Event = eventDetails;

            return View();
        }
    }
}
