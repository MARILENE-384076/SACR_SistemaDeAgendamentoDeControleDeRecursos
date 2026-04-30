using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ApiAgendamento.Model;
using Shared; 

namespace AgendamentoInterface
{
    public partial class MainWindow : Window
    {
        // Endereço da API (Porta 7075)
        private readonly HttpClient _client = new 
            HttpClient { BaseAddress = new Uri("https://localhost:7075/") };

        public MainWindow()
        {
            InitializeComponent();
            _ = CarregarDados(); 
        }
        
        private async void BtnReservar_Click(object sender, RoutedEventArgs e)
        {
            var novoAgendamento = new AgendamentoDTO
            {
                RecursoNome = txtRecurso.Text,
                Responsavel = txtResponsavel.Text,
                DataInicio = dpInicio.SelectedDate ?? DateTime.Now,
                DataFim = dpFim.SelectedDate ?? DateTime.Now.AddHours(1),
                Status = "Confirmado",
                RecursoTipo = "Equipamento",
                Departamento = "Geral"
            };

            try
            {
                var response = await _client.PostAsJsonAsync("api/Agendamento", novoAgendamento);

                if (response.IsSuccessStatusCode)
                {
                    txtStatus.Text = "✅ Reserva realizada com sucesso!";
                    txtStatus.Foreground = Brushes.Green;
                    await CarregarDados(); // Atualiza a tabela 
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    txtStatus.Text = $"❌ Erro: {erro}";
                    txtStatus.Foreground = Brushes.OrangeRed;
                }
            }
            catch (Exception)
            {
                txtStatus.Text = "Erro de comunicação com a API.";
                txtStatus.Foreground = Brushes.Red;
            }
        }
        
        private async void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            await CarregarDados();
            txtStatus.Text = "Lista de monitoramento atualizada.";
            txtStatus.Foreground = Brushes.Gray;
        }

        // Método auxiliar para buscar os dados no banco através da API
        private async Task CarregarDados()
        {
            try
            {
                var lista = await _client.GetFromJsonAsync<List<AgendamentoDTO>>("api/Agendamento");
                dgAgendamentos.ItemsSource = lista;
            }
            catch (Exception)
            {
                txtStatus.Text = "Não foi possível carregar os dados.";
            }
        }
    }
}