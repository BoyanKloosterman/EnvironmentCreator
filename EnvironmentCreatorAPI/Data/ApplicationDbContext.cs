using Microsoft.EntityFrameworkCore;
using EnvironmentCreatorAPI.Models;

namespace EnvironmentCreatorAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Environment2D> Environments { get; set; }
        public DbSet<Object2D> Objects { get; set; }
    }
}
