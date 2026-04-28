using ApiAgendamento.Data; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using Shared;
using Shared.DTOs;

namespace ApiAgendamento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgendamentoController : ControllerBase
    {
        private readonly AppDbContext _context;

        // O CONSTRUTOR:O ASP.NET injeta o banco para usar na variável _context.
        public AgendamentoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            // Busca a lista do banco de dados
            var listaDoBanco = await _context.Agendamentos.ToListAsync();

            // Cria uma nova lista vazia para os DTOs
            var listaExibicao = new List<AgendamentoDTO>();

            // Transforma cada item do banco em um DTO de exibição
            foreach (var item in listaDoBanco)
            {
                var dto = new AgendamentoDTO
                {
                    Id = item.Id,
                    NomePaciente = item.NomePaciente,
                    DataHora = item.DataHora,
                    Procedimento = item.Procedimento
                };
                listaExibicao.Add(dto);
            }

            return Ok(listaExibicao);
        }

        // POST: Transforma DTO -> Banco
        [HttpPost]
        public async Task<IActionResult> Criar(AgendamentoDTO agendamentoDto)
        {

            if (agendamentoDto == null)
                return BadRequest("Dados inválidos");

            // Pega o que veio da "tela" (DTO) e passa para o banco (Entidade)
            var entidadeBanco = new Agendamento
            {
                NomePaciente = agendamentoDto.NomePaciente,
                DataHora = agendamentoDto.DataHora,
                Procedimento = agendamentoDto.Procedimento
            };

            // Adiciona na fila e salva no arquivo .db
            _context.Agendamentos.Add(entidadeBanco);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Salvo no banco com sucesso!" });
        }
    }
}

    