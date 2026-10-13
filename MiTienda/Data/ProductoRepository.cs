using MySqlConnector;
using MiTienda.Models;

namespace MiTienda.Data
{
    public class ProductoRepository
    {
        private const string SelectBase = @"
            SELECT p.id_producto, p.nombre_producto, p.descripcion, p.precio_producto, p.costo_producto,
                   p.stock_producto, p.id_categoria, c.nombre_categoria,
                   p.id_proveedor, pr.nombre_proveedor, p.ubicacion_percha, p.codigo, p.tipo_banda,
                   p.tipo_oring, p.aplicacion_vehiculo, p.marca
            FROM producto p
            JOIN categoria c ON c.id_categoria = p.id_categoria
            JOIN proveedor pr ON pr.id_proveedor = p.id_proveedor";

        public List<Producto> ObtenerTodos(string? filtroNombre = null, int? filtroCategoriaId = null,
            decimal? filtroPrecio = null, string? filtroCodigo = null, string? filtroMarca = null,
            string? filtroTipoBanda = null, string? filtroTipoOring = null,
            string? filtroAplicacion = null, string? filtroDescripcion = null)
        {
            var lista = new List<Producto>();
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();

            var condiciones = new List<string>();
            if (!string.IsNullOrWhiteSpace(filtroNombre))
            {
                condiciones.Add("p.nombre_producto LIKE @nombre");
                cmd.Parameters.AddWithValue("@nombre", $"%{filtroNombre}%");
            }
            if (filtroCategoriaId.HasValue)
            {
                condiciones.Add("p.id_categoria = @categoriaId");
                cmd.Parameters.AddWithValue("@categoriaId", filtroCategoriaId.Value);
            }
            if (filtroPrecio.HasValue)
            {
                condiciones.Add("p.precio_producto = @precio");
                cmd.Parameters.AddWithValue("@precio", filtroPrecio.Value);
            }
            if (!string.IsNullOrWhiteSpace(filtroCodigo))
            {
                condiciones.Add("p.codigo LIKE @codigo");
                cmd.Parameters.AddWithValue("@codigo", $"%{filtroCodigo}%");
            }
            if (!string.IsNullOrWhiteSpace(filtroMarca))
            {
                condiciones.Add("p.marca = @marca");
                cmd.Parameters.AddWithValue("@marca", filtroMarca);
            }
            if (!string.IsNullOrWhiteSpace(filtroTipoBanda))
            {
                condiciones.Add("p.tipo_banda = @tipoBanda");
                cmd.Parameters.AddWithValue("@tipoBanda", filtroTipoBanda);
            }
            if (!string.IsNullOrWhiteSpace(filtroTipoOring))
            {
                condiciones.Add("p.tipo_oring = @tipoOring");
                cmd.Parameters.AddWithValue("@tipoOring", filtroTipoOring);
            }
            if (!string.IsNullOrWhiteSpace(filtroAplicacion))
            {
                condiciones.Add("p.aplicacion_vehiculo LIKE @aplicacion");
                cmd.Parameters.AddWithValue("@aplicacion", $"%{filtroAplicacion}%");
            }
            if (!string.IsNullOrWhiteSpace(filtroDescripcion))
            {
                condiciones.Add("p.descripcion LIKE @descripcion");
                cmd.Parameters.AddWithValue("@descripcion", $"%{filtroDescripcion}%");
            }

            cmd.CommandText = SelectBase;
            if (condiciones.Count > 0)
                cmd.CommandText += " WHERE " + string.Join(" AND ", condiciones);
            cmd.CommandText += " ORDER BY p.id_producto;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(Map(reader));
            return lista;
        }

        public Producto? ObtenerPorId(int id)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = SelectBase + " WHERE p.id_producto = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public int Insertar(Producto p)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO producto
                    (nombre_producto, codigo, descripcion, tipo_banda, tipo_oring, aplicacion_vehiculo, marca, precio_producto, stock_producto,
                     ubicacion_percha, id_categoria, id_proveedor, costo_producto)
                VALUES
                    (@nombre, @codigo, @descripcion, @tipoBanda, @tipoOring, @aplicacion, @marca, @precio, @stock, @ubicacion, @categoriaId, @proveedorId, @costo);
                SELECT LAST_INSERT_ID();";
            AgregarParametros(cmd, p);
            // El trigger trg_producto_entrada registra automáticamente el
            // movimiento de ENTRADA con el stock inicial.
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Actualizar(Producto p)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE producto
                SET nombre_producto = @nombre, codigo = @codigo, descripcion = @descripcion,
                    tipo_banda = @tipoBanda, tipo_oring = @tipoOring, aplicacion_vehiculo = @aplicacion, marca = @marca,
                    precio_producto = @precio, costo_producto = @costo, stock_producto = @stock,
                    ubicacion_percha = @ubicacion,
                    id_categoria = @categoriaId, id_proveedor = @proveedorId
                WHERE id_producto = @id;";
            AgregarParametros(cmd, p);
            cmd.Parameters.AddWithValue("@id", p.Id);
            // Si el nuevo stock es mayor al anterior, trg_producto_aumento_stock
            // registra el movimiento de ENTRADA automáticamente.
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Intenta eliminar físicamente el producto. Si tiene movimientos o
        /// ventas asociadas (FK), MySQL rechaza la operación para conservar el historial.
        /// </summary>
        /// <returns>true si se pudo eliminar; false si tiene datos asociados.</returns>
        public bool Eliminar(int id)
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM producto WHERE id_producto = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException ex) when (ex.Number == 1451) // FK constraint
            {
                return false;
            }
        }

        public int ContarProductos()
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM producto;";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int ContarProductosAgotados()
        {
            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) FROM producto
                WHERE stock_producto <= 0;";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public ResumenInventario ObtenerResumenInventario()
        {
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT
                COALESCE(SUM(CASE WHEN stock_producto > 5 THEN 1 ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN stock_producto BETWEEN 1 AND 5 THEN 1 ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN stock_producto <= 0 THEN 1 ELSE 0 END), 0)
                FROM producto;";
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return new ResumenInventario();
            return new ResumenInventario { ConStock = reader.GetInt32(0), StockCritico = reader.GetInt32(1), Agotados = reader.GetInt32(2) };
        }

        private static void AgregarParametros(MySqlCommand cmd, Producto p)
        {
            cmd.Parameters.AddWithValue("@nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@codigo", (object?)p.Codigo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@descripcion", (object?)p.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tipoBanda", (object?)p.TipoBanda ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tipoOring", (object?)p.TipoOring ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@aplicacion", (object?)p.AplicacionVehiculo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@marca", (object?)p.Marca ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ubicacion", (object?)p.UbicacionPercha ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@precio", p.Precio);
            cmd.Parameters.AddWithValue("@costo", p.Costo);
            cmd.Parameters.AddWithValue("@stock", p.Stock);
            cmd.Parameters.AddWithValue("@categoriaId", p.CategoriaId);
            cmd.Parameters.AddWithValue("@proveedorId", p.ProveedorId);
        }

        private static Producto Map(MySqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
            Precio = reader.GetDecimal(3),
            Costo = reader.GetDecimal(4),
            Stock = reader.GetInt32(5),
            CategoriaId = reader.GetInt32(6),
            CategoriaNombre = reader.GetString(7),
            ProveedorId = reader.GetInt32(8),
            ProveedorNombre = reader.GetString(9),
            UbicacionPercha = reader.IsDBNull(10) ? null : reader.GetString(10),
            Codigo = reader.IsDBNull(11) ? null : reader.GetString(11),
            TipoBanda = reader.IsDBNull(12) ? null : reader.GetString(12),
            TipoOring = reader.IsDBNull(13) ? null : reader.GetString(13),
            AplicacionVehiculo = reader.IsDBNull(14) ? null : reader.GetString(14),
            Marca = reader.IsDBNull(15) ? null : reader.GetString(15),
        };
    }
}
