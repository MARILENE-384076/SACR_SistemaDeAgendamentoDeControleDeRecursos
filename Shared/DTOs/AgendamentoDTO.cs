using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class AgendamentoDTO
    {
        // O ID é mantido para operações de GET, PUT e DELETE
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do recurso é obrigatório.")]
        public string RecursoNome { get; set; }
        public string RecursoTipo { get; set; }

        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "A data de término é obrigatória.")]
        public DateTime DataFim { get; set; }

        [Required(ErrorMessage = "O nome do responsável é obrigatório.")]
        public string Responsavel { get; set; }
        public string Departamento { get; set; }

        public string Status { get; set; }
        
    }
}