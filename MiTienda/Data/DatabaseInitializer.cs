using MySqlConnector;

namespace MiTienda.Data
{
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Verifica que se pueda conectar a MySQL al arrancar la app.
        /// Las tablas y los datos de ejemplo se crean con database/schema.sql
        /// en MySQL Workbench, no desde aquí.
        /// </summary>
        /// <returns>true si la conexión fue exitosa; false si falló (ya muestra el error al usuario).</returns>
        public static bool VerificarConexion()
        {
            try
            {
                using var conn = Database.GetConnection();
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "No se pudo conectar a la base de datos MySQL.\n\n" +
                    $"Detalle: {ex.Message}\n\n" +
                    "Revisa que:\n" +
                    "  1. MySQL esté corriendo (XAMPP / MySQL Server / etc.)\n" +
                    "  2. Los datos en appsettings.json sean correctos (servidor, usuario, contraseña)\n" +
                    "  3. Hayas ejecutado database/schema.sql en MySQL Workbench para crear la base 'inventario_db'",
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al leer la configuración de conexión: {ex.Message}",
                    "Error de configuración",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
