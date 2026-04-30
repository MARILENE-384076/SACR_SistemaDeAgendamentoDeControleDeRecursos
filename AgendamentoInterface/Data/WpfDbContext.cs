using AgendamentoInterface.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace AgendamentoInterface.Data
{
    public class WpfDbContext : DbContext
    {
        public DbSet<WpfAgendamentoDTO> AgendamentosWPF { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Identifica onde o executável está rodando 
            string caminhoExecucao = AppDomain.CurrentDomain.BaseDirectory;

            // Sobe 3 níveis (net8.0-windows -> Debug -> bin) para chegar na raiz do projeto
            string caminhoRaiz = Path.GetFullPath(Path.Combine(caminhoExecucao, @"..\..\..\"));

            // Define o nome do arquivo banco e monta o caminho final
            string nomeBanco = "sacr_interfacewpf.db";
            string caminhoFinal = Path.Combine(caminhoRaiz, nomeBanco);

            // --- LINHA DE TESTE: Verifica o caminho que o banco esta sendo salvo ---
            System.Diagnostics.Debug.WriteLine($"[DEBUG BANCO] O banco está sendo lido em: {caminhoFinal}");

            optionsBuilder.UseSqlite($"Data Source={caminhoFinal}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WpfAgendamentoDTO>().HasKey(a => a.Id);

            // Aceita o ID vindo da API sem tentar gerar um novo localmente
            modelBuilder.Entity<WpfAgendamentoDTO>()
                .Property(a => a.Id)
                .ValueGeneratedNever();

            base.OnModelCreating(modelBuilder);
        }
    }
}