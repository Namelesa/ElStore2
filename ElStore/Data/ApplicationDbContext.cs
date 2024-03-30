using ElStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ElStore.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Images> Images { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Description> Descriptions { get; set; }
    }
}
