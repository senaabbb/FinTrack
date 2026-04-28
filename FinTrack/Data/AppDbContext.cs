using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Stock> Stocks { get; set; } = null!;
        public DbSet<Models.StockPrice> StockPrices { get; set; } = null!;
        public DbSet<Models.Currency> Currencies { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StockPrice>()
                .HasOne(sp => sp.Stock)
                .WithMany(s => s.Prices)
                .HasForeignKey(sp => sp.StockId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Stock>()
                .HasIndex(s => s.Symbol)
                .IsUnique();

            modelBuilder.Entity<StockPrice>()
                .Property(sp => sp.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Currency>()
                .Property(c => c.Rate)
                .HasPrecision(18, 4);
        }

    }
}
