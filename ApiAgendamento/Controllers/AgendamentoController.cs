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
            var agendamentos = await _context.Agendamentos.ToListAsync();

            // Mapeia os agendamentos do banco para o DTO, que é a estrutura que será retornada para o cliente.
            var listaDto = agendamentos.Select(a => new AgendamentoDTO
            {
                Id = a.Id,
                RecursoNome = a.RecursoNome,
                RecursoTipo = a.RecursoTipo,
                DataInicio = a.DataInicio,
                DataFim = a.DataFim,
                Responsavel = a.Responsavel,
                Departamento = a.Departamento,
                Status = a.Status
            }).ToList();

            return Ok(listaDto);
        }

        [HttpPost]
        public async Task<IActionResult> Criar(AgendamentoDTO dto)
        {
            if (dto == null) 
                return BadRequest("Dados inválidos.");

            // Mapeando do DTO, dado que o cliente enviou, para a Entidade (Banco)
            var novoAgendamento = new Agendamento
            {                
                RecursoNome = dto.RecursoNome,
                RecursoTipo = dto.RecursoTipo,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                Responsavel = dto.Responsavel,
                Departamento = dto.Departamento,
                Status = dto.Status
            };

            _context.Agendamentos.Add(novoAgendamento);
            await _context.SaveChangesAsync();

            return Ok(new 
            { mensagem = "Agendamento registrado com sucesso!" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            // Busca o agendamento no banco de dados pelo ID.
            var agendamento = await _context.Agendamentos.FindAsync(id);

            if (agendamento == null)
                return NotFound("Agendamento não encontrado.");

            // Mapeia a entidade do banco para o DTO.
            var dto = new AgendamentoDTO
            {
                Id = agendamento.Id,
                RecursoNome = agendamento.RecursoNome,
                RecursoTipo = agendamento.RecursoTipo,
                DataInicio = agendamento.DataInicio,
                DataFim = agendamento.DataFim,
                Responsavel = agendamento.Responsavel,
                Departamento = agendamento.Departamento,
                Status = agendamento.Status
            };

            return Ok(dto);
        }
    }
}

