using Microsoft.EntityFrameworkCore;
using AgendamentoSimulador.DTOs; 

namespace AgendamentoSimulador.Data
{
    internal class AppDbContext : DbContext
    {
        // Esta propriedade vira a tabela no banco de dados
        public DbSet<AgendamentoSimuladorDTO> Agendamentos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Define o nome do arquivo do banco de dados SQLite que será gerado
            optionsBuilder.UseSqlite("Data Source=AgendamentoSimulador.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<AgendamentoSimuladorDTO>().HasKey(a => a.Id);
        }
    }
}