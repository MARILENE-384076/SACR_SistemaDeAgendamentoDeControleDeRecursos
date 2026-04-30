using AgendamentoInterface.Services;
using AgendamentoInterface.Services.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoInterface.Data
{    
    public class WpfDbContext : DbContext
    {
        // O DbSet representa a coleção de agendamentos que serão armazenados no banco Local.
        public DbSet<WpfAgendamentoDTO> AgendamentosWPF { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configura o banco local SQLite
            optionsBuilder.UseSqlite("Data Source=sacr_interfacewpf.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Garante que o Id do DTO seja a chave primária no banco local
            modelBuilder.Entity<WpfAgendamentoDTO>().HasKey(a => a.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
}