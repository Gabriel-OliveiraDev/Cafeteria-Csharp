using Cafeteria.Models;
using Microsoft.EntityFrameworkCore;

namespace Cafeteria.Contexts
{
    public class CafeteriaContext : DbContext
    {
        public CafeteriaContext(DbContextOptions<CafeteriaContext> options) : base(options)
        {
        }

        public DbSet<Client> Client => Set<Client>();
        public DbSet<Supplier> Supplier => Set<Supplier>();
        public DbSet<Product> Product => Set<Product>();
        public DbSet<Transaction> Transaction => Set<Transaction>();

    }
}
