using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StarEventSystem.Models;

namespace StarEventSystem.Data
{
    public class StarEventSystemContext : DbContext
    {
        public StarEventSystemContext (DbContextOptions<StarEventSystemContext> options)
            : base(options)
        {
        }

        public DbSet<StarEventSystem.Models.Event> Event { get; set; } = default!;
    }
}
