using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendamentoInterface.Services
{
    public class WpfAgendamentoDTO
    {      
        public int Id { get; set; }

        public string RecursoNome { get; set; } = string.Empty;
        public string RecursoTipo { get; set; } = string.Empty;

        // Horários do agendamento
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        // Informações de quem agendou
        public string Responsavel { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;

        // Propriedade extra: Útil para exibir na interface WPF
        public string ResumoExibicao => $"{RecursoNome} - {Responsavel} ({DataInicio:dd/MM HH:mm})";
    }
}

