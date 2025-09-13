using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarEventSystem.Data;
using StarEventSystem.Models;

namespace StarEventSystem.Controllers
{
    public class TicketTypesController : Controller
    {
        private readonly StarEventSystemContext _context;

        public TicketTypesController(StarEventSystemContext context)
        {
            _context = context;
        }

        // GET: TicketTypes
        public async Task<IActionResult> Index()
        {
            var starEventSystemContext = _context.TicketTypes.Include(t => t.Event);
            return View(await starEventSystemContext.ToListAsync());
        }

        // GET: TicketTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketType = await _context.TicketTypes
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.TicketTypeId == id);
            if (ticketType == null)
            {
                return NotFound();
            }

            return View(ticketType);
        }

        // GET: TicketTypes/Create
        public IActionResult Create(int eventId)
        {
            var ticketType = new TicketType
            {
                EventId = eventId   // <-- make sure this is set
            };

            return View(ticketType);
        }

        // POST: TicketTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketTypeId,EventId,TypeName,Price,Seats")] TicketType ticketType)
        {
            if (ModelState.IsValid)
            {
                _context.TicketTypes.Add(ticketType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Events", new { id = ticketType.EventId });
            }
            return View(ticketType);
        }

        // GET: TicketTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketType = await _context.TicketTypes.FindAsync(id);
            if (ticketType == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventDescription", ticketType.EventId);
            return View(ticketType);
        }

        // POST: TicketTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketTypeId,EventId,TypeName,Price,Seats")] TicketType ticketType)
        {
            if (id != ticketType.TicketTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketTypeExists(ticketType.TicketTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventDescription", ticketType.EventId);
            return View(ticketType);
        }

        // GET: TicketTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketType = await _context.TicketTypes
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.TicketTypeId == id);
            if (ticketType == null)
            {
                return NotFound();
            }

            return View(ticketType);
        }

        // POST: TicketTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketType = await _context.TicketTypes.FindAsync(id);
            if (ticketType != null)
            {
                _context.TicketTypes.Remove(ticketType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketTypeExists(int id)
        {
            return _context.TicketTypes.Any(e => e.TicketTypeId == id);
        }
    }
}
