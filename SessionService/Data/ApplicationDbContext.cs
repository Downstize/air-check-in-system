using Microsoft.EntityFrameworkCore;
using SessionService.Models;

namespace SessionService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ActiveSession> ActiveSessions { get; set; }
}
