using System.Text.Json;
using MySqlConnector;

namespace MiTienda.Data
{
    /// <summary>
    /// Punto único para obtener conexiones a MySQL. Lee host/usuario/password
    /// desde appsettings.json (junto al .exe) para no hardcodear credenciales.
    /// </summary>
    public static class Database
    {
        private static string? _connectionString;

        public static string ConnectionString
        {
            get
            {
                _connectionString ??= BuildConnectionStringFromConfig();
                return _connectionString;
            }
        }

        public static MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        private static string BuildConnectionStringFromConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(
                    "No se encontró appsettings.json junto al ejecutable. " +
                    "Copia el archivo de ejemplo y coloca ahí tus datos de conexión a MySQL.",
                    configPath);
            }

            using var stream = File.OpenRead(configPath);
            using var doc = JsonDocument.Parse(stream);

            var mysql = doc.RootElement.GetProperty("ConnectionStrings").GetProperty("MySql");

            string server = mysql.GetProperty("Server").GetString() ?? "localhost";
            int port = mysql.GetProperty("Port").GetInt32();
            string database = mysql.GetProperty("Database").GetString() ?? "mitienda";
            string user = mysql.GetProperty("User").GetString() ?? "root";
            string password = mysql.GetProperty("Password").GetString() ?? "";

            var builder = new MySqlConnectionStringBuilder
            {
                Server = server,
                Port = (uint)port,
                Database = database,
                UserID = user,
                Password = password,
                // Útil en desarrollo: si la BD tarda en responder, no falla enseguida
                ConnectionTimeout = 10,
            };

            return builder.ConnectionString;
        }
    }
}
