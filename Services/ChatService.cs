using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace prototipoGeminiready.Services
{
    public class ChatService
    {
        private readonly ISession? _session;

        public ChatService()
        {
            try
            {
                _session = CreateSession();
            }
            catch (Exception)
            {
                _session = null;
            }
        }

        private static ISession CreateSession()
        {
            try
            {
                var cluster = Cluster.Builder()
                    .AddContactPoint(AppSettings.CassandraHost)
                    .WithPort(AppSettings.CassandraPort)
                    .Build();

                return cluster.Connect(AppSettings.CassandraKeyspace);
            }
            catch (NoHostAvailableException ex)
            {
                throw new InvalidOperationException(
                    $"No se pudo conectar a Cassandra en {AppSettings.CassandraHost}:{AppSettings.CassandraPort}. Verifica que Cassandra esté ejecutándose y que el keyspace '{AppSettings.CassandraKeyspace}' exista.",
                    ex);
            }
        }

        private void EnsureSession()
        {
            if (_session == null)
                throw new InvalidOperationException(
                    "La conexión a Cassandra no está disponible. Revisa CASSANDRA_HOST, CASSANDRA_PORT y CASSANDRA_KEYSPACE.");
        }

        public async Task GuardarMensajeAsync(Guid sesionId, DateTime timestamp, string origen, string mensaje, string tipo)
        {
            EnsureSession();
            var query = "INSERT INTO mensajes (sesion_id, timestamp, origen, mensaje, tipo) VALUES (?, ?, ?, ?, ?)";
            var prepared = _session.Prepare(query);
            var bound = prepared.Bind(sesionId, timestamp, origen, mensaje, tipo);
            await _session.ExecuteAsync(bound);
        }

        public async Task CrearSesionAsync(string rfc, Guid sesionId, DateTime inicio, DateTime fin, string resumen)
        {
            EnsureSession();
            var query = "INSERT INTO sesiones (rfc, sesion_id, fecha_inicio, fecha_fin, resumen) VALUES (?, ?, ?, ?, ?)";
            var prepared = _session.Prepare(query);
            var bound = prepared.Bind(rfc, sesionId, inicio, fin, resumen);
            await _session.ExecuteAsync(bound);
        }

        public async Task<List<string>> ObtenerMensajesDeSesionAsync(Guid sesionId)
        {
            EnsureSession();
            var query = "SELECT mensaje FROM mensajes WHERE sesion_id = ?";
            var prepared = _session.Prepare(query);
            var bound = prepared.Bind(sesionId);
            var result = await _session.ExecuteAsync(bound);

            return result.Select(row => row.GetValue<string>("mensaje")).ToList();
        }

        public async Task<string> ObtenerUltimoResumenAsync(string rfc)
        {
            EnsureSession();
            var query = "SELECT resumen FROM sesiones WHERE rfc = ? LIMIT 1";
            var prepared = _session.Prepare(query);
            var bound = prepared.Bind(rfc);
            var result = await _session.ExecuteAsync(bound);

            var row = result.FirstOrDefault();
            return row != null ? row.GetValue<string>("resumen") : "";
        }
    }
}
