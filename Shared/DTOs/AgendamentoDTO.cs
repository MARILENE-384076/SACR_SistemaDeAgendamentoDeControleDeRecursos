using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class AgendamentoDTO
    {
        // O ID é mantido para operações de GET, PUT e DELETE
        public int Id { get; set; }

        public string RecursoNome { get; set; }
        public string RecursoTipo { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public string Responsavel { get; set; }
        public string Departamento { get; set; }

        public string Status { get; set; }
        
    }
}