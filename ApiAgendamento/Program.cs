using ApiAgendamento.Config;
using ApiAgendamento.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// REGISTRO DAS CONFIGURAÇÕES
// lê a seção "ApiConfig" do appsettings.json e joga para a classe ApiConfig.cs
builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection("ApiConfig"));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Lê o arquivo XML gerado para exibir os ///summary e remarks na documentação
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});

// Configuração do Entity Framework Core para usar SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "documentação";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SIMI Sistema Integrado de Monitoramento Industrial v1");
    options.DocumentTitle = "SARC - (Sistema Integrado de Agendamento de Controle de Recursos) - Docs";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
