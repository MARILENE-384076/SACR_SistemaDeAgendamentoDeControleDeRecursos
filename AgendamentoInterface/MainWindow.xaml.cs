using AgendamentoInterface.Data;
using AgendamentoInterface.Services;
using AgendamentoInterface.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AgendamentoInterface
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _client = new
            HttpClient
        { BaseAddress = new Uri("https://localhost:7075/") };

        public MainWindow()
        {
            InitializeComponent();
            _ = CarregarDados();
        }

        private async void BtnReservar_Click(object sender, RoutedEventArgs e)
        {
            var novoAgendamento = new WpfAgendamentoDTO
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
                    
                    await SalvarNoBancoWPF(novoAgendamento);

                    await CarregarDados();
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    txtStatus.Text = $"❌ Erro: {erro}";
                    txtStatus.Foreground = Brushes.OrangeRed;
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Erro de comunicação com a API: {ex.Message}";
                txtStatus.Foreground = Brushes.Red;
            }
        }

        private async void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            await CarregarDados();
            txtStatus.Text = "Lista de monitoramento atualizada.";
            txtStatus.Foreground = Brushes.Gray;
        }

        private async Task CarregarDados()
        {
            try
            {
                var lista = await _client.GetFromJsonAsync<List<WpfAgendamentoDTO>>("api/Agendamento");
                dgAgendamentos.ItemsSource = lista;
            }
            catch (Exception)
            {
                txtStatus.Text = "Não foi possível carregar os dados da API.";
                txtStatus.Foreground = Brushes.Red;
            }
        }
        
        private async Task SalvarNoBancoWPF(WpfAgendamentoDTO dto)
        {
            try
            {
                using (var db = new WpfDbContext())
                {
                    db.Database.EnsureCreated();
                    db.AgendamentosWPF.Add(dto);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar no banco WPF: {ex.Message}");
            }
        }
    }
}