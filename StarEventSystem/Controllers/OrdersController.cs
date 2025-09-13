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
        public async Task<IActionResult> PlaceOrder(int EventId, decimal TotalAmount, string CustomerName, string CustomerEmail, string CustomerPhone, Dictionary<int, int> Quantities)
        {
            if (!Quantities.Any(q => q.Value > 0))
            {
                TempData["Error"] = "Select at least one ticket.";
                return RedirectToAction("Details", "Home", new { id = EventId });
            }

            // Create Customer
            var customer = new Customer
            {
                Name = CustomerName,
                Email = CustomerEmail,
                Phone = CustomerPhone
            };
            _context.Add(customer);
            await _context.SaveChangesAsync();

            // Create Order
            var order = new Order
            {
                EventId = EventId,
                CustomerId = customer.CustomerId,
                TotalAmount = TotalAmount
            };
            _context.Add(order);
            await _context.SaveChangesAsync();

            // Add OrderItems
            foreach (var q in Quantities.Where(x => x.Value > 0))
            {
                var ticket = await _context.TicketTypes.FindAsync(q.Key);
                if (ticket != null)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        TicketTypeId = ticket.TicketTypeId,
                        Quantity = q.Value,
                        Price = ticket.Price
                    };
                    _context.Add(orderItem);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(int EventId, Dictionary<int, int> Quantities)
        {
            var eventDetails = await _context.Event
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.EventId == EventId);

            if (eventDetails == null)
                return NotFound();

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

            ViewBag.SelectedTickets = selectedTickets;
            ViewBag.Total = total;
            ViewBag.Event = eventDetails;

            return View(); // renders Checkout.cshtml
        }

        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.TicketType)
                .Include(o => o.Event)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
