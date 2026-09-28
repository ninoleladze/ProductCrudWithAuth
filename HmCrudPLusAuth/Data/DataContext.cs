using HmCrudPLusAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace HmCrudPLusAuth.Data
{
    public class DataContext : DbContext
    {

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
        }
    }
}
