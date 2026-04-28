using ApiAgendamento.Data; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
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
                
    }
}