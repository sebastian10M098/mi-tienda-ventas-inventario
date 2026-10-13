using MySqlConnector;
using MiTienda.Models;

namespace MiTienda.Data
{
    public class CategoriaRepository
    {
        public List<Categoria> ObtenerTodas()
        {
            var lista = new List<Categoria>();
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_categoria, nombre_categoria, descripcion FROM categoria ORDER BY nombre_categoria;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Categoria
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                });
            }
            return lista;
        }

        public int Insertar(Categoria c)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO categoria (nombre_categoria, descripcion)
                VALUES (@nombre, @descripcion);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@nombre", c.Nombre);
            cmd.Parameters.AddWithValue("@descripcion", (object?)c.Descripcion ?? DBNull.Value);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Actualizar(Categoria c)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE categoria SET nombre_categoria = @nombre, descripcion = @descripcion
                WHERE id_categoria = @id;";
            cmd.Parameters.AddWithValue("@nombre", c.Nombre);
            cmd.Parameters.AddWithValue("@descripcion", (object?)c.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", c.Id);
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM categoria WHERE id_categoria = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
