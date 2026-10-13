using MySqlConnector;
using MiTienda.Models;

namespace MiTienda.Data
{
    public class ProveedorRepository
    {
        public List<Proveedor> ObtenerTodos(bool soloActivos = false)
        {
            var lista = new List<Proveedor>();
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_proveedor, nombre_proveedor, telefono, correo, direccion, estado FROM proveedor";
            if (soloActivos) cmd.CommandText += " WHERE estado = TRUE";
            cmd.CommandText += " ORDER BY nombre_proveedor;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Proveedor
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Telefono = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Correo = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Direccion = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Estado = reader.GetBoolean(5),
                });
            }
            return lista;
        }

        public int Insertar(Proveedor p)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO proveedor (nombre_proveedor, telefono, correo, direccion, estado)
                VALUES (@nombre, @telefono, @correo, @direccion, @estado);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@telefono", (object?)p.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@correo", (object?)p.Correo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@direccion", (object?)p.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@estado", p.Estado);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Actualizar(Proveedor p)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE proveedor
                SET nombre_proveedor = @nombre, telefono = @telefono, correo = @correo,
                    direccion = @direccion, estado = @estado
                WHERE id_proveedor = @id;";
            cmd.Parameters.AddWithValue("@nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@telefono", (object?)p.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@correo", (object?)p.Correo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@direccion", (object?)p.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@estado", p.Estado);
            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM proveedor WHERE id_proveedor = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
