using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using StarEventSystem.Data;
using StarEventSystem.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

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
                return Json(new { success = false, message = "Select at least one ticket." });
            }

            // Create Customer
            var customer = new Customer
            {
                Name = CustomerName,
                Email = CustomerEmail,
                Phone = CustomerPhone,
                Points = 50, // Initial points for new customer
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

            // Create OrderItems
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

                    // Optional: reduce available seats
                    ticket.Seats -= q.Value;
                }
            }

            // 3️⃣ Save order items first
            await _context.SaveChangesAsync();

            // 4️⃣ Generate QR Code (Base64)
            string qrText = $"OrderId:{order.OrderId};Customer:{customer.Name};Event:{order.EventId}";
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(10); // smaller size
            order.QrCodeBase64 = Convert.ToBase64String(qrBytes);

            // 5️⃣ Attach and mark only QrCodeBase64 as modified
            _context.Entry(order).Property(o => o.QrCodeBase64).IsModified = true;
            await _context.SaveChangesAsync();

            // 6️⃣ Redirect
            return Json(new { success = true, orderId = order.OrderId });
        }
        [Authorize]
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
