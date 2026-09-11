using Cassandra;
using System;
using System.Threading.Tasks;

namespace prototipoGeminiready.Services
{
    public class MensajeService
    {
        private readonly ISession? _session;

        public MensajeService()
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

        public async Task GuardarMensajeAsync(Guid sesionId, string origen, string mensaje, string tipo)
        {
            EnsureSession();
            var query = "INSERT INTO mensajes (sesion_id, timestamp, origen, mensaje, tipo) VALUES (?, toTimestamp(now()), ?, ?, ?)";
            var prepared = _session.Prepare(query);
            var bound = prepared.Bind(sesionId, origen, mensaje, tipo);
            await _session.ExecuteAsync(bound);
        }
    }
}
