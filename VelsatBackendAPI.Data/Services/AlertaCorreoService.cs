using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model.AlarmasCorreo;

namespace VelsatBackendAPI.Data.Services
{
    public class AlertaCorreoService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AlertaCorreoService> _logger;

        // TODO: mover a variables de entorno (mismo patrón que las ConnectionStrings)
        private const string MailerSendApiUrl = "https://api.mailersend.com/v1/email";
        private const string MailerSendApiToken = "mlsn.703ae2fd8dc61cdc70947b80c28ff29e2ffb9a4e50bd0b1cc73ca6ea6d09ecd7";
        private const string RemitenteEmail = "alertas@ideatec.com.pe";
        private const string RemitenteNombre = "Velsat SAC";

        private static readonly HttpClient _httpClient = new HttpClient();

        public AlertaCorreoService(IServiceProvider serviceProvider, ILogger<AlertaCorreoService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IServiceScope? scope = null;
                IUnitOfWork? uow = null;

                try
                {
                    scope = _serviceProvider.CreateScope();
                    var factory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();

                    // ✅ Crear UnitOfWork (NECESITA transacciones porque hace UPDATE)
                    uow = factory.Create();

                    var repo = uow.AlertaRepository;

                    var alertas = await repo.ObtenerAlertasNoEnviadasAsync();

                    if (alertas.Any())
                    {
                        foreach (var alerta in alertas)
                        {
                            try
                            {
                                await EnviarCorreoAsync("rentaautoschiclayo@gmail.com", alerta);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error enviando correo de alerta para el evento {Codigo} (device {DeviceID})", alerta.Codigo, alerta.DeviceID);
                            }
                        }

                        // ✅ Marcar como enviadas
                        await repo.MarcarComoEnviadasAsync(alertas.Select(a => a.Codigo).ToList());

                        // ✅ CRÍTICO: Commit de la transacción
                        uow.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el ciclo de AlertaCorreoService");
                }
                finally
                {
                    // ✅ CRÍTICO: Disponer en orden inverso
                    uow?.Dispose();
                    scope?.Dispose();
                }

                // Esperar 3 segundos antes del siguiente ciclo
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        private async Task EnviarCorreoAsync(string correo, RegistroAlarmas alerta)
        {
            string tituloAlerta = (alerta.EventType, alerta.AlarmType) switch
            {
                ("alarm", "powerCut") => "🚨 Alerta! Desconexión de Batería",
                ("alarm", "sos") => "🚨 Alerta! Botón de Pánico",
                ("deviceOverspeed", _) => "🚨 Alerta! Exceso de Velocidad",
                _ => "🚨 Alerta! Evento Desconocido"
            };

            var payload = new
            {
                from = new { email = RemitenteEmail, name = RemitenteNombre },
                to = new[] { new { email = correo } },
                cc = new[] { new { email = "cmyg@velsat.com.pe" } },
                subject = tituloAlerta,
                html = GenerarCuerpoCorreo(alerta)
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, MailerSendApiUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", MailerSendApiToken);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"MailerSend respondió {(int)response.StatusCode} {response.StatusCode}: {body}");
            }
        }

        private string GenerarCuerpoCorreo(RegistroAlarmas alerta)
        {
            DateTime fechaHora = DateTimeOffset.FromUnixTimeSeconds(alerta.Timestamp).ToLocalTime().DateTime;
            var fecha = fechaHora.ToString("dd/MM/yyyy");
            var hora = fechaHora.ToString("HH:mm:ss");

            string tituloAlerta;

            switch (alerta.EventType, alerta.AlarmType)
            {
                case ("alarm", "powerCut"):
                    tituloAlerta = "DESCONEXIÓN DE BATERÍA";
                    break;
                case ("alarm", "sos"):
                    tituloAlerta = "BOTÓN DE PÁNICO";
                    break;
                case ("deviceOverspeed", _):
                    tituloAlerta = "EXCESO DE VELOCIDAD";
                    break;
                default:
                    tituloAlerta = "ALERTA DESCONOCIDA";
                    break;
            }

            return $@"
    <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    color: #333;
                    margin: 0;
                    padding: 0;
                    word-wrap: break-word;
                }}
                .container {{
                    width: 100%;
                    max-width: 600px;
                    margin: auto;
                    background-color: #f4f4f4;
                }}
                .body {{
                    background-color: white;
                    border-radius: 0 0 8px 8px;
                }}
            </style>
        </head>

        <body>
            <div class='container'>
                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #fff; padding: 20px; text-align: center;'>
                    <tr>
                        <td>
                            <h2 style='margin: 0; font-size: 14px; color: #001d3d;'>CENTRAL DE MONITOREO Y GESTIÓN</h2>
                        </td>
                    </tr>
                </table>

                <div class='body'>
                    <div style='display: flex; align-items: center; justify-content: space-between; background-color: #f4f4f4; padding: 20px;'>
                        <div style='width: 60%;'>
                            <p style='font-size: 11px; color: #001d3d; font-weight: bold; margin: 0; text-transform: uppercase; margin-bottom: 10px;'>Alerta detectada en su unidad {alerta.DeviceID}</p>

                            <div style='margin-top: 20px;'>
                                <h2 style='font-size: 16px; color: #d00000; margin: 5px 0 10px 0;'>{tituloAlerta}</h2>
                                <p style='margin: 3px 0; font-size: 11px;'><strong>Fecha:</strong> {fecha}</p>
                                <p style='margin: 3px 0; font-size: 11px;'><strong>Hora:</strong> {hora}</p>
                                <p style='margin: 3px 0; font-size: 11px;'><strong>Ubicación:</strong> {alerta.Latitude}, {alerta.Longitude}</p>
                            </div>
                        </div>
                    </div>

                    <div style='width: 100%;'>
                        <div style='width: 80%; margin: 0 auto;'>
                            <hr style='border: none; border-top: 0.5px solid black; margin: 20px 0;' />
                            <p style='text-align: center; font-size: 12px;'>
                                Estamos comprometidos con brindarle a usted el mejor servicio. Gracias por su preferencia.
                            </p>
                            <hr style='border: none; border-top: 0.5px solid black; margin: 20px 0;' />
                        </div>
                    </div>

                    <div style='background-color: #fff; text-align: center; padding: 20px; color: #001d3d; font-size: 11px; font-family: Arial, sans-serif;'>
                        <p style='margin: 5px 0;'><strong>Central de Monitoreo y Gestión</strong></p>
                        <p style='margin: 5px 0;'>989112975 - 952075325</p>
                        <p style='margin: 5px 0;'>cmyg@velsat.com.pe</p>
                        <p style='margin: 5px 0;'>Av. Juan Pablo Fernandini 1439 Int. 603F, Pueblo Libre, Lima.</p>
                        <hr style='border: none; border-top: 1px solid #001d3d; margin: 15px 0;' />
                        <p style='margin: 0;'>© {DateTime.Now.Year} Todos los derechos reservados.</p>
                    </div>
                </div>
            </div>
        </body>
    </html>";
        }
    }
}