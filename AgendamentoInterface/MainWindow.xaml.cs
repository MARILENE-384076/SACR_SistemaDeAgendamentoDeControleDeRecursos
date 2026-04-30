using AgendamentoInterface.Data;
using AgendamentoInterface.Services;
using AgendamentoInterface.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using AgendamentoInterface.Data;

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
            InicializarHorarios();
            _ = CarregarDados();
        }

        private void InicializarHorarios()
        {
            // Preenche as ComboBoxes de 00 a 23 e 00 a 59
            for (int i = 0; i < 24; i++)
            {
                cbHoraInicio.Items.Add(i.ToString("D2"));
                cbHoraFim.Items.Add(i.ToString("D2"));
            }
            for (int i = 0; i < 60; i += 5)
            { // Incremento de 5 em 5 minutos
                cbMinutoInicio.Items.Add(i.ToString("D2"));
                cbMinutoFim.Items.Add(i.ToString("D2"));
            }
            // Valores padrão
            cbHoraInicio.SelectedIndex = 8; cbMinutoInicio.SelectedIndex = 0;
            cbHoraFim.SelectedIndex = 9; cbMinutoFim.SelectedIndex = 0;
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

        private async void BtnReservar_Click(object sender, RoutedEventArgs e)
        {
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
                    txtStatus.Text = "✅ Reserva realizada com sucesso!";
                    txtStatus.Foreground = Brushes.Green;
                    await SalvarNoBancoWPF(novoAgendamento);
                    await CarregarDados();
                }
                else
                {
                    txtStatus.Text = "❌ Erro ao salvar na API.";
                    txtStatus.Foreground = Brushes.OrangeRed;
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Erro: {ex.Message}";
                txtStatus.Foreground = Brushes.Red;
            }
        }

        private async Task CarregarDados()
        {
            try
            {
                var lista = await _client.GetFromJsonAsync<List<WpfAgendamentoDTO>>("api/Agendamento");
                dgAgendamentos.ItemsSource = lista;
            }
            catch { /* Tratamento de erro */ }
        }

        private async Task SalvarNoBancoWPF(WpfAgendamentoDTO dto)
        {
            try
            {
                // Usando o contexto de dados do seu projeto WPF
                using (var db = new AgendamentoInterface.Data.WpfDbContext())
                {
                    // Verifica se o arquivo sacr_interface.db existe, se não, ele cria
                    await db.Database.EnsureCreatedAsync();

                    // Adiciona o seu DTO próprio na tabela AgendamentosWPF
                    db.AgendamentosWPF.Add(dto);

                    // Salva as alterações de forma assíncrona
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Caso ocorra erro no SQLite local, avisamos no console de depuração
                System.Diagnostics.Debug.WriteLine($"Erro no banco WPF: {ex.Message}");
            }
        }

        private async void BtnAtualizar_Click(object sender, RoutedEventArgs e) => await CarregarDados();
    }
}