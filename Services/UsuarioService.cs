using Cassandra;
using prototipoGeminiready.Services;

public class UsuarioService
{
    private readonly ISession? _session;

    public UsuarioService()
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

    public async Task<bool> RegistrarUsuarioAsync(string rfc, string correo, string contrasena)
    {
        EnsureSession();

        var queryRfc = "SELECT rfc FROM usuarios WHERE rfc = ?";
        var statementRfc = _session.Prepare(queryRfc);
        var boundRfc = statementRfc.Bind(rfc);
        var resultRfc = await _session.ExecuteAsync(boundRfc);

        if (resultRfc.Any())
            throw new Exception("Ya existe un usuario con ese RFC.");

        var queryCorreo = "SELECT email FROM usuarios WHERE email = ?";
        var statementCorreo = _session.Prepare(queryCorreo);
        var boundCorreo = statementCorreo.Bind(correo);
        var resultCorreo = await _session.ExecuteAsync(boundCorreo);

        if (resultCorreo.Any())
            throw new Exception("Ya existe un usuario con ese correo.");

        var insertQuery = "INSERT INTO usuarios (rfc, email, password_hash) VALUES (?, ?, ?)";
        var insertStatement = _session.Prepare(insertQuery);
        var insertBound = insertStatement.Bind(rfc, correo, contrasena);
        await _session.ExecuteAsync(insertBound);

        return true;
    }

    public async Task<bool> ValidarCredencialesAsync(string rfc, string password)
    {
        EnsureSession();

        var query = "SELECT password_hash FROM usuarios WHERE rfc = ?";
        var prepared = _session.Prepare(query);
        var bound = prepared.Bind(rfc);
        var result = await _session.ExecuteAsync(bound);

        var row = result.FirstOrDefault();
        if (row == null)
            return false;

        var storedHash = row.GetValue<string>("password_hash");

        return password == storedHash;
    }
}
