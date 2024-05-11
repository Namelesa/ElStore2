
using ElStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ElStore.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<DescriptionPC> DescriptionPC { get; set; }
        public DbSet<HearphoneDescriptions> HearphoneDescriptions { get; set; }
        public DbSet<Images> Images { get; set; }
    }
}
