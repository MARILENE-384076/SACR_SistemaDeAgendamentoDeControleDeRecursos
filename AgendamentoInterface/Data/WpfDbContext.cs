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
            // 1. Identifica onde o executável está rodando agora
            string caminhoExecucao = AppDomain.CurrentDomain.BaseDirectory;

            // 2. Sobe 3 níveis (net8.0-windows -> Debug -> bin) para chegar na raiz do projeto
            string caminhoRaiz = Path.GetFullPath(Path.Combine(caminhoExecucao, @"..\..\..\"));

            // 3. Define o nome do arquivo e monta o caminho final
            string nomeBanco = "sacr_interfacewpf.db";
            string caminhoFinal = Path.Combine(caminhoRaiz, nomeBanco);

            // --- LINHA DE TESTE: Verifique o resultado na aba 'Output' (Saída) do Visual Studio ---
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