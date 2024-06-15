using ElStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElStore.Data
{
    public class ApplicationDbContext: IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<DescriptionPC> DescriptionPC { get; set; }
        public DbSet<HearphoneDescriptions> HearphoneDescriptions { get; set; }
        public DbSet<Images> Images { get; set; }
        
        public DbSet<Users> Users { get; set; }
    }
}
