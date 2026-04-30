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
            // Validação do Campo Recurso
            if (string.IsNullOrWhiteSpace(txtRecurso.Text))
            {
                txtStatus.Text = "O campo 'Recurso' é obrigatório.";
                txtStatus.Foreground = Brushes.Orange;
                txtRecurso.Focus();
                return;
            }

            // Validação do Campo Responsável
            if (string.IsNullOrWhiteSpace(txtResponsavel.Text))
            {
                txtStatus.Text = " O campo 'Responsável' é obrigatório.";
                txtStatus.Foreground = Brushes.Orange;
                txtResponsavel.Focus();
                return;
            }

            // Validação das Datas - Garante que a data não esteja no passado
            if (dpInicio.SelectedDate < DateTime.Today)
            {
                txtStatus.Text = "A data de início não pode ser anterior à data atual.";
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
                    txtStatus.Text = "Reserva realizada com sucesso na API!";
                    txtStatus.Foreground = Brushes.Green;

                    // Tenta salvar no banco local após sucesso na API
                    await SalvarNoBancoWPF(novoAgendamento);
                    await CarregarDados();
                }
                else
                {
                    // Erro vindo da API - ex: recurso ocupado
                    var erroApi = await response.Content.ReadAsStringAsync();
                    txtStatus.Text = $"❌ A API recusou o registro: {erroApi}";
                    txtStatus.Foreground = Brushes.OrangeRed;
                }
            }
            // Tratamento de falha de conexão
            catch (HttpRequestException)
            {
                txtStatus.Text = "Falha de conexão: Verifique se o servidor da API está ligado.";
                txtStatus.Foreground = Brushes.Red;
            }
            // Tratamento de erros inesperados
            catch (Exception ex)
            {
                txtStatus.Text = $"Erro inesperado: {ex.Message}";
                txtStatus.Foreground = Brushes.Red;
            }
        }

        private async Task CarregarDados()
        {
            try
            {
                // Tenta buscar os dados da API
                var lista = await _client.GetFromJsonAsync<List<WpfAgendamentoDTO>>("api/Agendamento");

                if (lista == null)
                {
                    txtStatus.Text = "A API retornou uma lista vazia ou inválida.";
                    txtStatus.Foreground = Brushes.Orange;
                    return;
                }

                // Atualiza a interface com sucesso
                dgAgendamentos.ItemsSource = lista;
                txtStatus.Text = "Sincronização com a API realizada.";
                txtStatus.Foreground = Brushes.Gray;
            }
            catch (HttpRequestException ex)
            {
                // Falha de conexão (API Offline ou porta errada)
                txtStatus.Text = "Erro de Conexão: O servidor da API não foi encontrado.";
                txtStatus.Foreground = Brushes.Red;

                // Log para depuração
                System.Diagnostics.Debug.WriteLine($"Falha de rede: {ex.Message}");
            }
            catch (System.Text.Json.JsonException ex)
            {
                // Problema na estrutura do JSON (Dados incompatíveis)
                txtStatus.Text = "Erro de Dados: A resposta da API é incompatível com o DTO.";
                txtStatus.Foreground = Brushes.OrangeRed;

                System.Diagnostics.Debug.WriteLine($"Erro de Serialização: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Erro para qualquer falha inesperada
                txtStatus.Text = $"Erro inesperado ao carregar: {ex.Message}";
                txtStatus.Foreground = Brushes.Red;
            }
        }
        private async Task SalvarNoBancoWPF(WpfAgendamentoDTO dto)
        {
            // Validação de Integridade do Objeto
            if (dto == null)
            {
                txtStatus.Text = "Erro interno: O agendamento está vazio.";
                txtStatus.Foreground = Brushes.Orange;
                return;
            }

            try
            {
                using (var db = new AgendamentoInterface.Data.WpfDbContext())
                {
                    // Validação/Criação da Infraestrutura do Banco de Dados sacr_interface.db
                    bool criadoAgora = await db.Database.EnsureCreatedAsync();

                    if (criadoAgora)
                    {
                        System.Diagnostics.Debug.WriteLine("Base de dados SQLite criada com sucesso.");
                    }

                    // Persistência dos Dados
                    db.AgendamentosWPF.Add(dto);
                    await db.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine("Cópia salva com sucesso no banco local.");
                }
            }
            catch (System.IO.IOException ex)
            {
                // Arquivo bloqueado ou sem permissão
                txtStatus.Text = "Erro de Acesso: O banco local está bloqueado ou o disco está cheio.";
                txtStatus.Foreground = Brushes.Red;
                System.Diagnostics.Debug.WriteLine($"Erro de IO no SQLite: {ex.Message}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Falha ao inserir (ex: violação de chave primária ou constraint)
                txtStatus.Text = " Erro de Banco: Não foi possível gravar os dados localmente.";
                txtStatus.Foreground = Brushes.OrangeRed;
                System.Diagnostics.Debug.WriteLine($"Erro de Update: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                // Falha para qualquer outra falha inesperada
                txtStatus.Text = "Falha inesperada ao salvar no banco local.";
                txtStatus.Foreground = Brushes.Red;
                System.Diagnostics.Debug.WriteLine($"Erro Geral SQLite: {ex.Message}");
            }
        }

        private async void BtnAtualizar_Click(object sender, RoutedEventArgs e) => await CarregarDados();
    }
}