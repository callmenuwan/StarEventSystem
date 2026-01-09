using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarEventSystem.Data;
using StarEventSystem.Models;
using System.Text;

namespace StarEventSystem.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly StarEventSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(StarEventSystemContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /User/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Dashboard summary
            var totalOrders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .CountAsync();

            var totalSpent = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalSpent = totalSpent;

            // ✅ Load recent orders to display on dashboard
            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.TicketType)
                .Include(o => o.Event)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Orders = orders;

            return View(user);
        }

        // GET: /User/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            return View(user);
        }

        // POST: /User/Profile
        [HttpPost]
        public async Task<IActionResult> Profile(string fullName, string email, string? password)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!string.IsNullOrEmpty(fullName))
                user.FullName = fullName;

            if (!string.IsNullOrEmpty(email))
                user.Email = email;

            var result = await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pwdResult = await _userManager.ResetPasswordAsync(user, token, password);
                if (!pwdResult.Succeeded)
                {
                    TempData["Error"] = string.Join(", ", pwdResult.Errors.Select(e => e.Description));
                    return RedirectToAction("Profile");
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // GET: /User/ExportOrdersCsv
        [HttpGet]
        public async Task<IActionResult> ExportOrdersCsv()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.TicketType)
                .Include(o => o.Event)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("OrderId,Event,Ticket,Quantity,Price,TotalAmount,OrderDate");

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    csv.AppendLine($"{order.OrderId},{order.Event.EventName},{item.TicketType.TypeName},{item.Quantity},{item.Price},{order.TotalAmount},{order.CreatedAt:yyyy-MM-dd}");
                }
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "OrderHistory.csv");
        }
    }
}
