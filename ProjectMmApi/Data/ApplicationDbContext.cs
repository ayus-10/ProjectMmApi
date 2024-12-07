using Microsoft.EntityFrameworkCore;
using ProjectMmApi.Models.Entities;

namespace ProjectMmApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Friend> Friends { get; set; }
    }
}
