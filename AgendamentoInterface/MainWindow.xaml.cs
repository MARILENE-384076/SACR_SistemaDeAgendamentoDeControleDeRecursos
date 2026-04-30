using AgendamentoInterface.Data;
using AgendamentoInterface.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.Json; // Adicione esta biblioteca

namespace AgendamentoInterface
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _client = new HttpClient { BaseAddress = new Uri("https://localhost:7075/") };

        // Configuração para garantir que o C# entenda o JSON da API mesmo com letras diferentes
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public MainWindow()
        {
            InitializeComponent();
            InicializarHorarios();
            _ = CarregarDados();
        }

        private void InicializarHorarios()
        {
            for (int i = 0; i < 24; i++) { cbHoraInicio.Items.Add(i.ToString("D2")); cbHoraFim.Items.Add(i.ToString("D2")); }
            for (int i = 0; i < 60; i += 5) { cbMinutoInicio.Items.Add(i.ToString("D2")); cbMinutoFim.Items.Add(i.ToString("D2")); }
            cbHoraInicio.SelectedIndex = 8;
            cbHoraFim.SelectedIndex = 9;
            dpInicio.SelectedDate = DateTime.Now;
            dpFim.SelectedDate = DateTime.Now;
        }

        private DateTime CombinarDataHora(DatePicker dp, ComboBox cbH, ComboBox cbM)
        {
            DateTime data = dp.SelectedDate ?? DateTime.Now;
            // Uso do TryParse para evitar o erro de 'string format' que você teve
            int.TryParse(cbH.Text, out int hora);
            int.TryParse(cbM.Text, out int minuto);
            return new DateTime(data.Year, data.Month, data.Day, hora, minuto, 0);
        }

        private async void BtnReservar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRecurso.Text) || string.IsNullOrWhiteSpace(txtResponsavel.Text))
            {
                txtStatus.Text = "⚠ Preencha Recurso e Responsável!";
                txtStatus.Foreground = Brushes.Orange;
                return;
            }

            var novoAgendamento = new WpfAgendamentoDTO
            {
                RecursoNome = txtRecurso.Text,
                Responsavel = txtResponsavel.Text,
                DataInicio = CombinarDataHora(dpInicio, cbHoraInicio, cbMinutoInicio),
                DataFim = CombinarDataHora(dpFim, cbHoraFim, cbMinutoFim),
                Status = "Confirmado",
                RecursoTipo = "Equipamento",
                Departamento = "Geral"
            };

            try
            {
                var response = await _client.PostAsJsonAsync("api/Agendamento", novoAgendamento);

                if (response.IsSuccessStatusCode)
                {
                    // AQUI ESTÁ A CHAVE: Usando as _options para o objeto não vir vazio
                    var agendamentoComIdOficial = await response.Content.ReadFromJsonAsync<WpfAgendamentoDTO>(_options);

                    if (agendamentoComIdOficial != null)
                    {
                        txtStatus.Text = "✅ Sincronizado com sucesso!";
                        txtStatus.Foreground = Brushes.Green;

                        await SalvarNoBancoWPF(agendamentoComIdOficial);
                        await CarregarDados();
                    }
                }
                else
                {
                    txtStatus.Text = $"❌ API recusou: {await response.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"❌ Erro: {ex.Message}";
            }
        }

        private async Task SalvarNoBancoWPF(WpfAgendamentoDTO dto)
        {
            try
            {
                using var db = new WpfDbContext();
                await db.Database.EnsureCreatedAsync();

                // Evita duplicar o ID 48 (ou qualquer outro)
                bool jaExiste = await db.AgendamentosWPF.AnyAsync(a => a.Id == dto.Id);

                if (!jaExiste)
                {
                    db.AgendamentosWPF.Add(dto);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro SQLite: {ex.Message}");
            }
        }

        private async Task CarregarDados()
        {
            try
            {
                // Também usamos as _options aqui para a grade não ficar em branco
                var lista = await _client.GetFromJsonAsync<List<WpfAgendamentoDTO>>("api/Agendamento", _options);
                dgAgendamentos.ItemsSource = lista;
            }
            catch
            {
                txtStatus.Text = "⚠ API indisponível.";
            }
        }

        private async void BtnAtualizar_Click(object sender, RoutedEventArgs e) => await CarregarDados();
    }
}