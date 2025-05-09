using Baggage.Models;
using Microsoft.EntityFrameworkCore;

namespace Baggage.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) 
            : base(opts) { }
        
        public DbSet<PaidOption> PaidOptions { get; set; }
        public DbSet<BaggageRegistration> BaggageRegistrations { get; set; }
        public DbSet<BaggagePayment> BaggagePayment { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<PaidOption>()
                .HasKey(p => p.PaidOptionId);

            mb.Entity<BaggageRegistration>()
                .HasKey(r => r.RegistrationId);
            
            mb.Entity<BaggagePayment>()
                .HasKey(p => p.PaymentId);
        }
    }
}