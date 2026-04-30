using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ApiAgendamento.Data;
using ApiAgendamento.Model;
using ApiAgendamento.Config;
using Shared;

namespace ApiAgendamento.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento de agendamentos de recursos do sistema SACR.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AgendamentoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ApiConfig _config;

        /// <summary>
        /// Construtor da Controller de Agendamento.
        /// </summary>
        /// <param name="context">Contexto do banco de dados injetado.</param>
        /// <param name="config">Configurações da API injetadas via IOptions.</param>
        public AgendamentoController(AppDbContext context, IOptions<ApiConfig> config)
        {
            _context = context;
            _config = config.Value;
        }

        /// <summary>
        /// Lista todos os agendamentos registrados no sistema.
        /// </summary>
        /// <returns>Uma lista de agendamentos convertidos para DTO.</returns>
        /// <response code="200">Retorna a lista de agendamentos com sucesso.</response>
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            var agendamentos = await _context.Agendamentos.ToListAsync();

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

        /// <summary>
        /// Registra um novo agendamento de recurso.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/Agendamento
        ///     {
        ///        "recursoNome": "Sala de Reuniões",
        ///        "recursoTipo": "Ambiente",
        ///        "dataInicio": "2026-05-15T09:00:00",
        ///        "dataFim": "2026-05-15T11:00:00",
        ///        "responsavel": "Marilene",
        ///        "departamento": "TI",
        ///        "status": "Confirmado"
        ///     }
        /// </remarks>
        /// <param name="dto">Dados do agendamento enviados pelo cliente.</param>
        /// <response code="200">Agendamento registrado com sucesso.</response>
        /// <response code="400">Se houver conflito de horário ou dados inválidos.</response>
        [HttpPost]
        public async Task<IActionResult> Criar(AgendamentoDTO dto)
        {
            if (dto == null)
                return BadRequest("Dados inválidos.");

            if (string.IsNullOrWhiteSpace(dto.RecursoNome))
                return BadRequest("O nome do recurso é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Responsavel))
                return BadRequest("O nome do responsável é obrigatório.");

            if (dto.DataFim <= dto.DataInicio)
                return BadRequest("A data de término deve ser maior que a data de início.");

            // Validação de conflito de horário (Regra de Negócio)
            var conflito = await _context.Agendamentos.AnyAsync(a =>
                a.RecursoNome == dto.RecursoNome &&
                dto.DataInicio < a.DataFim &&
                dto.DataFim > a.DataInicio);

            if (conflito)
                return BadRequest("Este recurso já está reservado para o horário selecionado.");

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

            return Ok(new { mensagem = "Agendamento registrado com sucesso!", id = novoAgendamento.Id });
        }

        /// <summary>
        /// Busca os detalhes de um agendamento específico através do ID.
        /// </summary>
        /// <param name="id">ID numérico do registro.</param>
        /// <response code="200">Retorna o agendamento encontrado.</response>
        /// <response code="404">Caso o ID não exista no banco de dados.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);

            if (agendamento == null)
                return NotFound(new { mensagem = $"Agendamento com ID {id} não foi encontrado." });

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

        /// <summary>
        /// Atualiza as informações de um agendamento já existente.
        /// </summary>
        /// <param name="id">ID do registro a ser atualizado.</param>
        /// <param name="dto">Objeto com as novas informações.</param>
        /// <response code="200">Atualização realizada com sucesso.</response>
        /// <response code="400">Dados inválidos ou conflito de horários.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, AgendamentoDTO dto)
        {
            if (id != dto.Id)
                return BadRequest("O ID da URL não coincide com o ID do corpo da requisição.");

            if (string.IsNullOrWhiteSpace(dto.RecursoNome))
                return BadRequest("O nome do recurso não pode ser vazio.");

            if (string.IsNullOrWhiteSpace(dto.Responsavel))
                return BadRequest("O responsável não pode ser vazio.");

            if (dto.DataFim <= dto.DataInicio)
                return BadRequest("A data de término deve ser maior que a data de início.");

            // Verifica conflito ignorando o próprio registro que está sendo editado
            var conflito = await _context.Agendamentos.AnyAsync(a =>
                a.Id != id &&
                a.RecursoNome == dto.RecursoNome &&
                dto.DataInicio < a.DataFim &&
                dto.DataFim > a.DataInicio);

            if (conflito)
                return BadRequest("Não foi possível atualizar: Este recurso já está ocupado por outro agendamento neste horário.");

            var agendamentoNoBanco = await _context.Agendamentos.FindAsync(id);

            if (agendamentoNoBanco == null)
                return NotFound(new { mensagem = "Agendamento não encontrado para atualização." });

            agendamentoNoBanco.RecursoNome = dto.RecursoNome;
            agendamentoNoBanco.RecursoTipo = dto.RecursoTipo;
            agendamentoNoBanco.DataInicio = dto.DataInicio;
            agendamentoNoBanco.DataFim = dto.DataFim;
            agendamentoNoBanco.Responsavel = dto.Responsavel;
            agendamentoNoBanco.Departamento = dto.Departamento;
            agendamentoNoBanco.Status = dto.Status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Erro ao atualizar o banco de dados.");
            }

            return Ok(new { mensagem = "Agendamento atualizado com sucesso!" });
        }

        /// <summary>
        /// Remove permanentemente um agendamento do banco de dados.
        /// </summary>
        /// <param name="id">ID do agendamento a ser excluído.</param>
        /// <response code="200">Registro removido com sucesso.</response>
        /// <response code="404">Agendamento não encontrado.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);

            if (agendamento == null)
                return NotFound(new { mensagem = $"Não foi possível excluir: Agendamento com ID {id} não encontrado." });

            _context.Agendamentos.Remove(agendamento);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Agendamento removido com sucesso!" });
        }
    }
}