using Microsoft.EntityFrameworkCore;

namespace ApiAgendamento.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Shared.Agendamento> Agendamentos { get; set; }
    }
}
