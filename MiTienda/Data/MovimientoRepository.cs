using MySqlConnector;
using MiTienda.Models;

namespace MiTienda.Data
{
    public class MovimientoRepository
    {
        public List<Movimiento> ObtenerHistorial(int limite = 50, int? productoId = null)
        {
            var lista = new List<Movimiento>();
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT m.id_movimiento, m.id_producto, p.nombre_producto, m.id_usuario, u.nombre_usuario,
                       m.tipo_movimiento, m.cantidad, m.stock_anterior, m.stock_nuevo,
                       m.fecha_movimiento, m.observacion
                FROM movimiento m
                JOIN producto p ON p.id_producto = m.id_producto
                JOIN usuario u ON u.id_usuario = m.id_usuario";

            if (productoId.HasValue)
            {
                cmd.CommandText += " WHERE m.id_producto = @productoId";
                cmd.Parameters.AddWithValue("@productoId", productoId.Value);
            }

            cmd.CommandText += " ORDER BY m.fecha_movimiento DESC, m.id_movimiento DESC LIMIT @limite;";
            cmd.Parameters.AddWithValue("@limite", limite);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Movimiento
                {
                    Id = reader.GetInt32(0),
                    ProductoId = reader.GetInt32(1),
                    ProductoNombre = reader.GetString(2),
                    UsuarioId = reader.GetInt32(3),
                    UsuarioNombre = reader.GetString(4),
                    TipoMovimiento = reader.GetString(5),
                    Cantidad = reader.GetInt32(6),
                    StockAnterior = reader.GetInt32(7),
                    StockNuevo = reader.GetInt32(8),
                    FechaMovimiento = reader.GetDateTime(9),
                    Observacion = reader.IsDBNull(10) ? null : reader.GetString(10),
                });
            }
            return lista;
        }
    }
}
