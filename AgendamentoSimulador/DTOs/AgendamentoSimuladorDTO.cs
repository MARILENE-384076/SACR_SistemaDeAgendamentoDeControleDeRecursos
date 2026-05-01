using System;

namespace AgendamentoSimulador.DTOs
{
    public class AgendamentoSimuladorDTO
    {
        // Chave Primária para o banco SQLite local
        public int Id { get; set; }

        public string RecursoNome { get; set; } = string.Empty;

        public string RecursoTipo { get; set; } = string.Empty;

        public string Responsavel { get; set; } = string.Empty;

        public string Departamento { get; set; } = string.Empty;

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }

        public string Status { get; set; } = "Pendente";

        // Propriedade opcional para log local: registra quando o simulador gerou este dado
        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}