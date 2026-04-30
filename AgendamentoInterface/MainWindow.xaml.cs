using AgendamentoInterface.Data;
using AgendamentoInterface.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AgendamentoInterface
{
    public partial class MainWindow : Window
    {
        // Cliente para comunicação com a sua API
        private readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7075/")
        };

        public MainWindow()
        {
            InitializeComponent();
            InicializarHorarios();
            _ = CarregarDados(); // Carrega a grade ao abrir o programa
        }

        // --- MÉTODOS DE APOIO (Interface) ---

        private void InicializarHorarios()
        {
            for (int i = 0; i < 24; i++)
            {
                cbHoraInicio.Items.Add(i.ToString("D2"));
                cbHoraFim.Items.Add(i.ToString("D2"));
            }
            for (int i = 0; i < 60; i += 5)
            {
                cbMinutoInicio.Items.Add(i.ToString("D2"));
                cbMinutoFim.Items.Add(i.ToString("D2"));
            }
            cbHoraInicio.SelectedIndex = 8;
            cbHoraFim.SelectedIndex = 9;
            dpInicio.SelectedDate = DateTime.Now;
            dpFim.SelectedDate = DateTime.Now;
        }

            private DateTime CombinarDataHora(DatePicker dp, ComboBox cbH, ComboBox cbM)
            {
                DateTime data = dp.SelectedDate ?? DateTime.Now;
                int hora = int.Parse(cbH.Text ?? "0");
                int minuto = int.Parse(cbM.Text ?? "0");
                return new DateTime(data.Year, data.Month, data.Day, hora, minuto, 0);
        }

        // --- LÓGICA PRINCIPAL (Botão Reservar) ---

        private async void BtnReservar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validação simples de campos obrigatórios
            if (string.IsNullOrWhiteSpace(txtRecurso.Text) || string.IsNullOrWhiteSpace(txtResponsavel.Text))
            {
                txtStatus.Text = "⚠ Preencha Recurso e Responsável!";
                txtStatus.Foreground = Brushes.Orange;
                return;
            }

            // 2. Montagem do objeto (DTO) com os dados da tela
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
                // 3. Tenta salvar na API primeiro
                var response = await _client.PostAsJsonAsync("api/Agendamento", novoAgendamento);

                if (response.IsSuccessStatusCode)
                {
                    // SUCESSO NA API: Agora pegamos o objeto que a API devolveu (com o ID gerado por ela)
                    var agendamentoComIdOficial = await response.Content.ReadFromJsonAsync<WpfAgendamentoDTO>();

                    if (agendamentoComIdOficial != null)
                    {
                        txtStatus.Text = "✅ Reserva salva na API e no Banco Local!";
                        txtStatus.Foreground = Brushes.Green;

                        // 4. Salva uma cópia idêntica no SQLite local
                        await SalvarNoBancoWPF(agendamentoComIdOficial);

                        await CarregarDados(); // Atualiza a lista na tela
                    }
                }
                else
                {
                    // Se a API recusar (ex: conflito de horário)
                    string erro = await response.Content.ReadAsStringAsync();
                    txtStatus.Text = $"❌ API recusou: {erro}";
                    txtStatus.Foreground = Brushes.DarkRed;
                }
            }
            catch (Exception ex)
            {
                // Se o servidor estiver desligado ou houver erro de rede
                txtStatus.Text = $"❌ Erro de conexão ou sistema: {ex.Message}";
                txtStatus.Foreground = Brushes.Red;
            }
        }

        // --- PERSISTÊNCIA LOCAL (SQLite) ---

        private async Task SalvarNoBancoWPF(WpfAgendamentoDTO dto)
        {
            try
            {
                using (var db = new WpfDbContext())
                {
                    // Garante que a tabela exista
                    await db.Database.EnsureCreatedAsync();

                    // Proteção: Só adiciona se esse ID ainda não existir localmente
                    bool jaExiste = await db.AgendamentosWPF.AnyAsync(a => a.Id == dto.Id);

                    if (!jaExiste)
                    {
                        db.AgendamentosWPF.Add(dto);
                        await db.SaveChangesAsync(); // Grava fisicamente no sacr_interfacewpf.db
                    }
                }
            }
            catch (Exception ex)
            {
                // Log de erro silencioso para não travar a tela principal
                System.Diagnostics.Debug.WriteLine($"Erro ao gravar SQLite: {ex.Message}");
            }
        }

        // --- CARREGAMENTO DE DADOS ---

        private async Task CarregarDados()
        {
            try
            {
                var lista = await _client.GetFromJsonAsync<List<WpfAgendamentoDTO>>("api/Agendamento");
                dgAgendamentos.ItemsSource = lista;
            }
            catch
            {
                txtStatus.Text = "⚠ Visualizando apenas modo offline (API indisponível).";
            }
        }

        private async void BtnAtualizar_Click(object sender, RoutedEventArgs e) => await CarregarDados();
    }
}