using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ApiAgendamento.Data;
using ApiAgendamento.Model;
using Shared; 

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

            // Validação Nome do Recurso
            if (string.IsNullOrWhiteSpace(dto.RecursoNome))
            {
                return BadRequest("O nome do recurso é obrigatório.");
            }

            // Validação Reponsavel
            if (string.IsNullOrWhiteSpace(dto.Responsavel))
            {
                return BadRequest("O nome do responsável é obrigatório.");
            }

            // Validação de Datas
            if (dto.DataFim <= dto.DataInicio)
            {
                return BadRequest("A data de término deve ser maior que a data de início.");
            }

            // Verifica se existe algum agendamento que conflite no horário
            var conflito = await _context.Agendamentos.AnyAsync(a =>
                a.RecursoNome == dto.RecursoNome &&
                dto.DataInicio < a.DataFim &&
                dto.DataFim > a.DataInicio);

            if (conflito)
            {
                return BadRequest("Este recurso já está reservado para o horário selecionado.");
            }

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
            { mensagem = "Agendamento registrado com sucesso!", id = novoAgendamento.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            // Busca o agendamento no banco de dados pelo ID.
            var agendamento = await _context.Agendamentos.FindAsync(id);

            // Se o banco retornar nulo - Erro 404)
            if (agendamento == null)
                return NotFound(new 
                { mensagem = $"Agendamento com ID {id} não foi encontrado." });

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
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, AgendamentoDTO dto)
        {
            // Valida se o ID da URL bate com o ID do objeto.
            if (id != dto.Id)
            {
                return BadRequest("O ID da URL não coincide com o ID do corpo da requisição.");
            }

            // Valida Nome do Recurso
            if (string.IsNullOrWhiteSpace(dto.RecursoNome))
                return BadRequest("O nome do recurso não pode ser vazio.");

            // Valida Responsável
            if (string.IsNullOrWhiteSpace(dto.Responsavel))
                return BadRequest("O responsável não pode ser vazio.");

            //Valida Data
            if (dto.DataFim <= dto.DataInicio)
            {
                return BadRequest("A data de término deve ser maior que a data de início.");
            }


            // Verifica se existe outro agendamento (a.Id != id) para o mesmo recurso
            // que se sobreponha a este horário
            var conflito = await _context.Agendamentos.AnyAsync(a =>
                a.Id != id &&
                a.RecursoNome == dto.RecursoNome &&
                dto.DataInicio < a.DataFim &&
                dto.DataFim > a.DataInicio);

            if (conflito)
            {
                return BadRequest("Não foi possível atualizar: Este recurso já está ocupado" +
                    " por outro agendamento neste horário.");
            }

            // Busca o registro existente no banco
            var agendamentoNoBanco = await _context.Agendamentos.FindAsync(id);

            if (agendamentoNoBanco == null)
            {
                return NotFound(new 
                { mensagem = "Agendamento não encontrado para atualização." });
            }

            // Atualiza as propriedades do banco com os dados do DTO
            agendamentoNoBanco.RecursoNome = dto.RecursoNome;
            agendamentoNoBanco.RecursoTipo = dto.RecursoTipo;
            agendamentoNoBanco.DataInicio = dto.DataInicio;
            agendamentoNoBanco.DataFim = dto.DataFim;
            agendamentoNoBanco.Responsavel = dto.Responsavel;
            agendamentoNoBanco.Departamento = dto.Departamento;
            agendamentoNoBanco.Status = dto.Status;

            // Salva as mudanças
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500,
                    "Erro ao atualizar o banco de dados.");
            }

            return Ok(new 
            { mensagem = "Agendamento atualizado com sucesso!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            // Busca o registro no banco para garantir que ele existe
            var agendamento = await _context.Agendamentos.FindAsync(id);

            // Se não existir, retorna o erro 404 (NotFound)
            if (agendamento == null)
            {
                return NotFound(new
                { mensagem = $"Não foi possível excluir: Agendamento com ID {id} não encontrado." });
            }

            // Se existir, remove o registro do banco
            _context.Agendamentos.Remove(agendamento);
            await _context.SaveChangesAsync();
           
            return Ok(new 
            { mensagem = "Agendamento removido com sucesso!" });
        }
    }
}

