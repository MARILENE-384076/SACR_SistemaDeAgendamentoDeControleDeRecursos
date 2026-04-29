using Microsoft.EntityFrameworkCore;
using Shared;

namespace ApiAgendamento.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Agendamento> Agendamentos { get; set; }
    }
}
