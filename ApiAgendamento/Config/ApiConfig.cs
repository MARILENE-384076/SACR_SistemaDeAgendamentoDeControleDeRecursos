namespace ApiAgendamento.Config
{
    public class ApiConfig
    {
        /// <summary>
        /// Limite máximo de horas permitido para uma única reserva.
        /// </summary>
        public int MaxHorasReserva { get; set; }

        /// <summary>
        /// Define com quantos dias de antecedência um recurso pode ser agendado.
        /// </summary>
        public int AntecedenciaMaximaDias { get; set; }

        /// <summary>
        /// Determina se o sistema permite agendamentos fora do horário comercial (ex: após as 18h).
        /// </summary>
        public bool PermitirForaHorarioComercial { get; set; }
    }
}
    
