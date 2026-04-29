using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared; 

namespace AgendamentoSimulador
{
    class Program
    {
        static async Task Main(string[] args)
        {            
            var http = new HttpClient();
            var random = new Random();
            string urlApi = "https://localhost:7075/api/Agendamento";        

            Console.WriteLine("=== SIMULADOR DE AGENDAMENTOS EM TEMPO REAL INICIADO... ===");
            Console.WriteLine("Pressione Ctrl+C para parar a simulação.\n");

            // O loop infinito simula o envio constante de agendamentos para a API
            while (true)
            {
                // GERAÇÃO ALEATÓRIA DE DADOS
                // Sorteia um recurso de 1 a 10 para aumentar as chances de colisão/sucesso
                int idRecurso = random.Next(1, 3);

                // Sorteia um deslocamento de dias e horas para o futuro
                int diasNoFuturo = random.Next(0, 2);
                int horaInicio = random.Next(6, 22); 

                var novoAgendamento = new Agendamento
                {
                    RecursoNome = $"Equipamento {idRecurso}",
                    RecursoTipo = "Hardware",
                    Responsavel = $"Usuário_{random.Next(100, 999)}",
                    Departamento = "Simulação Automática",
                    DataInicio = DateTime.Now.AddDays(diasNoFuturo).Date.AddHours(horaInicio),
                    DataFim = DateTime.Now.AddDays(diasNoFuturo).Date.AddHours(horaInicio + 2),
                    Status = "Pendente"
                };

                // ENVIO PARA A API
                try
                {
                    // Envia o pacote para a URL da API
                    var resposta = await http.PostAsJsonAsync(urlApi, novoAgendamento);

                    // EXIBIÇÃO DO RESULTADO NO CONSOLE
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");

                    if (resposta.IsSuccessStatusCode)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[{timestamp}] SUCESSO: {novoAgendamento.RecursoNome} reservado.");
                    }
                    else
                    {
                        // Aqui captura quando o Random gera um horário que já existe no SQLite
                        string msgErro = await resposta.Content.ReadAsStringAsync();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[{timestamp}] CONFLITO: {novoAgendamento.RecursoNome} ocupado. Detalhe: {msgErro}");
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] ERRO DE CONEXÃO: {ex.Message}");
                }

                Console.ResetColor();

                // INTERVALO ALEATÓRIO ENTRE ENVIOS
                // Pausa entre 1 e 4 segundos para não travar o processamento da sua máquina
                await Task.Delay(random.Next(1000, 4000));
            }
        }
    }
}