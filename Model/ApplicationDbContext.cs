using Microsoft.EntityFrameworkCore;

namespace OilTraderAI.Model
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<Intraday> Intraday { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=OilTraderData.db");
    }
}