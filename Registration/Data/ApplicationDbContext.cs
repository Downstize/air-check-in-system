using Microsoft.EntityFrameworkCore;
using Registration.Models;

namespace Registration.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }
        
        public DbSet<SeatReservation> SeatReservations { get; set; }
        public DbSet<RegistrationRecord> RegistrationRecords { get; set; }
        
        public DbSet<Payment> Payments { get; set; }
        

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<SeatReservation>()
                .HasKey(s => s.SeatReservationId);

            mb.Entity<RegistrationRecord>()
                .HasKey(r => r.RegistrationRecordId);
            
            mb.Entity<Payment>().
                HasKey(p => p.PaymentId);
        }
    }
}