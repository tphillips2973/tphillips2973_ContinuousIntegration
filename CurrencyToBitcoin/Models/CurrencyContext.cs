using Microsoft.EntityFrameworkCore;

namespace CurrencyToBitcoin.Models
{
    public class CurrencyContext : DbContext
    {
        public CurrencyContext(DbContextOptions<CurrencyContext> options)
               : base(options)
        {

        }

        public DbSet<Currency> Currencies { get; set; }
    }
}
