using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class Agendamento
    {
        // Identificação Única
        public int Id { get; set; }

        // Dados do Recurso
        public string RecursoNome { get; set; } // Ex: "Sala de Reuniões 01"
        public string RecursoTipo { get; set; } // Ex: "Sala", "Projetor", "Carro"

        // Dados do Tempo (Essencial para o controle)
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        // Dados do Solicitante
        public string Responsavel { get; set; }
        public string Departamento { get; set; }

        // Controle de Status
        public string Status { get; set; } // Ex: "Confirmado", "Cancelado", "Finalizado"

        // Auditoria (Útil para o App Console monitorar)
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
