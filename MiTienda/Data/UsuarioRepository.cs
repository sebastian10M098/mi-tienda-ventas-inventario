using MySqlConnector;
using MiTienda.Models;

namespace MiTienda.Data
{
    public class UsuarioRepository
    {
        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id_usuario, nombre_usuario, correo, rol, estado, fecha_registro
                FROM usuario ORDER BY nombre_usuario;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(Map(reader));
            return lista;
        }

        public Usuario? ObtenerPorId(int id)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id_usuario, nombre_usuario, correo, rol, estado, fecha_registro
                FROM usuario WHERE id_usuario = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        /// <summary>
        /// Valida credenciales y actualiza a PBKDF2 las contraseñas de texto
        /// plano heredadas cuando se usan correctamente.
        /// </summary>
        public Usuario? ValidarCredenciales(string correo, string password)
        {
            int id;
            string guardada;
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id_usuario, password FROM usuario WHERE correo = @correo AND estado = TRUE;";
                cmd.Parameters.AddWithValue("@correo", correo.Trim());
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;
                id = reader.GetInt32(0);
                guardada = reader.GetString(1);
            }

            if (!PasswordSecurity.Verificar(password, guardada, out bool actualizar)) return null;
            if (actualizar) GuardarHash(id, PasswordSecurity.HashPassword(password));
            return ObtenerPorId(id);
        }

        public bool ValidarClaveAdministrador(string password)
        {
            var administradores = new List<(int Id, string Hash)>();
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id_usuario, password FROM usuario WHERE rol = 'ADMIN' AND estado = TRUE;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) administradores.Add((reader.GetInt32(0), reader.GetString(1)));
            }

            foreach (var administrador in administradores)
            {
                if (!PasswordSecurity.Verificar(password, administrador.Hash, out bool actualizar)) continue;
                if (actualizar) GuardarHash(administrador.Id, PasswordSecurity.HashPassword(password));
                return true;
            }
            return false;
        }

        public bool CambiarClaveAdministrador(string actual, string nueva)
        {
            var administradores = new List<(int Id, string Hash)>();
            using (var conn = Database.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id_usuario, password FROM usuario WHERE rol = 'ADMIN' AND estado = TRUE;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) administradores.Add((reader.GetInt32(0), reader.GetString(1)));
            }

            foreach (var administrador in administradores)
            {
                if (!PasswordSecurity.Verificar(actual, administrador.Hash, out _)) continue;
                GuardarHash(administrador.Id, PasswordSecurity.HashPassword(nueva));
                return true;
            }
            return false;
        }

        public int Insertar(Usuario u)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO usuario (nombre_usuario, correo, password, rol, estado)
                VALUES (@nombre, @correo, @password, @rol, @estado);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@nombre", u.NombreUsuario);
            cmd.Parameters.AddWithValue("@correo", u.Correo);
            cmd.Parameters.AddWithValue("@password", PasswordSecurity.HashPassword(u.Password));
            cmd.Parameters.AddWithValue("@rol", u.Rol);
            cmd.Parameters.AddWithValue("@estado", u.Estado);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Actualizar(Usuario u)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE usuario
                SET nombre_usuario = @nombre, correo = @correo, rol = @rol, estado = @estado
                WHERE id_usuario = @id;";
            cmd.Parameters.AddWithValue("@nombre", u.NombreUsuario);
            cmd.Parameters.AddWithValue("@correo", u.Correo);
            cmd.Parameters.AddWithValue("@rol", u.Rol);
            cmd.Parameters.AddWithValue("@estado", u.Estado);
            cmd.Parameters.AddWithValue("@id", u.Id);
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM usuario WHERE id_usuario = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static Usuario Map(MySqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            NombreUsuario = reader.GetString(1),
            Correo = reader.GetString(2),
            Rol = reader.GetString(3),
            Estado = reader.GetBoolean(4),
            FechaRegistro = reader.GetDateTime(5),
        };

        private static void GuardarHash(int usuarioId, string hash)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE usuario SET password = @password WHERE id_usuario = @id;";
            cmd.Parameters.AddWithValue("@password", hash);
            cmd.Parameters.AddWithValue("@id", usuarioId);
            cmd.ExecuteNonQuery();
        }
    }
}
