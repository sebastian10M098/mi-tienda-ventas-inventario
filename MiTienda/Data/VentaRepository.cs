using MySqlConnector;
using MiTienda.Models;

namespace MiTienda.Data
{
    public class StockInsuficienteException : Exception
    {
        public StockInsuficienteException(string message) : base(message) { }
    }

    public class VentaRepository
    {
        public int RegistrarVenta(List<VentaLinea> carrito, int usuarioId)
        {
            using var conn = Database.GetConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                decimal subtotal = carrito.Sum(x => x.Subtotal);
                decimal iva = carrito.Sum(x => x.ValorIva);
                decimal total = carrito.Sum(x => x.Total);
                using var facturaCmd = conn.CreateCommand();
                facturaCmd.Transaction = tx;
                facturaCmd.CommandText = @"INSERT INTO factura (id_usuario, subtotal, valor_iva, total)
                    VALUES (@usuarioId, @subtotal, @iva, @total); SELECT LAST_INSERT_ID();";
                facturaCmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                facturaCmd.Parameters.AddWithValue("@subtotal", subtotal);
                facturaCmd.Parameters.AddWithValue("@iva", iva);
                facturaCmd.Parameters.AddWithValue("@total", total);
                int facturaId = Convert.ToInt32(facturaCmd.ExecuteScalar());

                using var ventaCmd = conn.CreateCommand();
                ventaCmd.Transaction = tx;
                ventaCmd.CommandText = @"INSERT INTO venta
                    (id_factura, id_producto, id_usuario, cantidad, precio_unitario, costo_unitario, aplica_iva, valor_iva, observacion)
                    VALUES (@facturaId, @productoId, @usuarioId, @cantidad, @precioUnitario, @costoUnitario, @aplicaIva, @valorIva, @observacion);";
                foreach (var item in carrito)
                {
                    ventaCmd.Parameters.Clear();
                    ventaCmd.Parameters.AddWithValue("@facturaId", facturaId);
                    ventaCmd.Parameters.AddWithValue("@productoId", item.ProductoId);
                    ventaCmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                    ventaCmd.Parameters.AddWithValue("@cantidad", item.Cantidad);
                    ventaCmd.Parameters.AddWithValue("@precioUnitario", item.PrecioUnitario);
                    ventaCmd.Parameters.AddWithValue("@costoUnitario", item.CostoUnitario);
                    ventaCmd.Parameters.AddWithValue("@aplicaIva", item.AplicaIva);
                    ventaCmd.Parameters.AddWithValue("@valorIva", item.ValorIva);
                    ventaCmd.Parameters.AddWithValue("@observacion", (object?)item.Observacion ?? "Venta mostrador");
                    ventaCmd.ExecuteNonQuery();
                }
                tx.Commit();
                return facturaId;
            }
            catch (MySqlException ex) when (ex.Message.Contains("Stock insuficiente"))
            {
                tx.Rollback();
                throw new StockInsuficienteException(ex.Message);
            }
        }

        public List<Factura> ObtenerFacturas(DateTime? fecha = null)
        {
            var lista = new List<Factura>();
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT f.id_factura, f.fecha_factura, COUNT(v.id_venta), COALESCE(SUM(v.cantidad), 0),
                       f.subtotal, f.valor_iva, f.total FROM factura f JOIN venta v ON v.id_factura = f.id_factura "
                + (fecha.HasValue ? "WHERE DATE(f.fecha_factura) = @fecha " : string.Empty) + @"
                GROUP BY f.id_factura, f.fecha_factura, f.subtotal, f.valor_iva, f.total
                ORDER BY f.fecha_factura DESC, f.id_factura DESC;";
            if (fecha.HasValue) cmd.Parameters.AddWithValue("@fecha", fecha.Value.Date);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(new Factura
            {
                Id = reader.GetInt32(0), Fecha = reader.GetDateTime(1), Productos = reader.GetInt32(2),
                Unidades = reader.GetInt32(3), Subtotal = reader.GetDecimal(4), Iva = reader.GetDecimal(5), Total = reader.GetDecimal(6),
            });
            return lista;
        }

        public List<Factura> ObtenerFacturas(DateTime desde, DateTime hasta)
        {
            var lista = new List<Factura>();
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT f.id_factura, f.fecha_factura, COUNT(v.id_venta), COALESCE(SUM(v.cantidad), 0),
                       f.subtotal, f.valor_iva, f.total
                FROM factura f JOIN venta v ON v.id_factura = f.id_factura
                WHERE f.fecha_factura >= @desde AND f.fecha_factura < @hasta
                GROUP BY f.id_factura, f.fecha_factura, f.subtotal, f.valor_iva, f.total
                ORDER BY f.fecha_factura DESC, f.id_factura DESC;";
            cmd.Parameters.AddWithValue("@desde", desde);
            cmd.Parameters.AddWithValue("@hasta", hasta);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(new Factura
            {
                Id = reader.GetInt32(0), Fecha = reader.GetDateTime(1), Productos = reader.GetInt32(2),
                Unidades = reader.GetInt32(3), Subtotal = reader.GetDecimal(4), Iva = reader.GetDecimal(5), Total = reader.GetDecimal(6),
            });
            return lista;
        }

        public bool EliminarFacturaPermanentemente(int facturaId)
        {
            using var conn = Database.GetConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                using (var restaurarStock = conn.CreateCommand())
                {
                    restaurarStock.Transaction = tx;
                    restaurarStock.CommandText = @"UPDATE producto p
                        JOIN (SELECT id_producto, SUM(cantidad) AS cantidad
                              FROM venta WHERE id_factura = @facturaId GROUP BY id_producto) v
                          ON v.id_producto = p.id_producto
                        SET p.stock_producto = p.stock_producto + v.cantidad;";
                    restaurarStock.Parameters.AddWithValue("@facturaId", facturaId);
                    restaurarStock.ExecuteNonQuery();
                }

                using (var eliminarVentas = conn.CreateCommand())
                {
                    eliminarVentas.Transaction = tx;
                    eliminarVentas.CommandText = "DELETE FROM venta WHERE id_factura = @facturaId;";
                    eliminarVentas.Parameters.AddWithValue("@facturaId", facturaId);
                    eliminarVentas.ExecuteNonQuery();
                }

                using var eliminarFactura = conn.CreateCommand();
                eliminarFactura.Transaction = tx;
                eliminarFactura.CommandText = "DELETE FROM factura WHERE id_factura = @facturaId;";
                eliminarFactura.Parameters.AddWithValue("@facturaId", facturaId);
                bool eliminada = eliminarFactura.ExecuteNonQuery() > 0;
                tx.Commit();
                return eliminada;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public Factura? ObtenerFactura(int facturaId)
        {
            var factura = ObtenerFacturas().FirstOrDefault(x => x.Id == facturaId);
            if (factura == null) return null;
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT v.id_venta, v.id_factura, v.id_producto, p.nombre_producto, v.cantidad,
                       v.precio_unitario, v.costo_unitario, v.aplica_iva, v.valor_iva, v.observacion
                FROM venta v JOIN producto p ON p.id_producto = v.id_producto
                WHERE v.id_factura = @facturaId ORDER BY v.id_venta;";
            cmd.Parameters.AddWithValue("@facturaId", facturaId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) factura.Lineas.Add(new VentaLinea
            {
                Id = reader.GetInt32(0), FacturaId = reader.GetInt32(1), ProductoId = reader.GetInt32(2),
                ProductoNombre = reader.GetString(3), Cantidad = reader.GetInt32(4), PrecioUnitario = reader.GetDecimal(5),
                CostoUnitario = reader.GetDecimal(6), AplicaIva = reader.GetBoolean(7), ValorIva = reader.GetDecimal(8), Observacion = reader.IsDBNull(9) ? null : reader.GetString(9),
            });
            return factura;
        }

        // Compatibilidad temporal: el historial visible vive ahora en Reportes.
        public List<VentaLinea> ObtenerHistorial(DateTime? fecha = null) => new();

        public List<ResumenCategoria> VentasPorCategoria(DateTime desde, DateTime hasta)
        {
            var lista = new List<ResumenCategoria>();
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT c.nombre_categoria, COALESCE(SUM(v.cantidad * v.precio_unitario + v.valor_iva), 0)
                FROM factura f JOIN venta v ON v.id_factura = f.id_factura
                JOIN producto p ON p.id_producto = v.id_producto JOIN categoria c ON c.id_categoria = p.id_categoria
                WHERE f.fecha_factura >= @desde AND f.fecha_factura < @hasta GROUP BY c.nombre_categoria ORDER BY 2 DESC;";
            cmd.Parameters.AddWithValue("@desde", desde); cmd.Parameters.AddWithValue("@hasta", hasta); using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(new ResumenCategoria { Nombre = reader.GetString(0), Total = reader.GetDecimal(1) });
            return lista;
        }

        public List<ResumenProducto> ProductosMasVendidos(DateTime desde, DateTime hasta)
        {
            var lista = new List<ResumenProducto>();
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT p.nombre_producto, SUM(v.cantidad) FROM factura f JOIN venta v ON v.id_factura = f.id_factura
                JOIN producto p ON p.id_producto = v.id_producto WHERE f.fecha_factura >= @desde AND f.fecha_factura < @hasta
                GROUP BY p.id_producto, p.nombre_producto ORDER BY 2 DESC LIMIT 5;";
            cmd.Parameters.AddWithValue("@desde", desde); cmd.Parameters.AddWithValue("@hasta", hasta); using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(new ResumenProducto { Nombre = reader.GetString(0), Unidades = reader.GetInt32(1) });
            return lista;
        }

        public int ContarVentasHoy()
        {
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM factura WHERE DATE(fecha_factura) = CURDATE();";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public decimal TotalVendidoHoy()
        {
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(SUM(total), 0) FROM factura WHERE DATE(fecha_factura) = CURDATE();";
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public decimal TotalCostoVendidoHoy()
        {
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(SUM(v.cantidad * v.costo_unitario), 0) FROM factura f JOIN venta v ON v.id_factura = f.id_factura WHERE DATE(f.fecha_factura) = CURDATE();";
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public decimal GananciaHoy()
        {
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(SUM(v.cantidad * (v.precio_unitario - v.costo_unitario)), 0) FROM factura f JOIN venta v ON v.id_factura = f.id_factura WHERE DATE(f.fecha_factura) = CURDATE();";
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public List<ResumenVentaDiaria> VentasDiarias(DateTime desde, DateTime hasta)
        {
            var lista = new List<ResumenVentaDiaria>();
            using var conn = Database.GetConnection(); using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT DATE(f.fecha_factura), COALESCE(SUM(v.cantidad), 0),
                    COALESCE(SUM(v.cantidad * v.precio_unitario + v.valor_iva), 0)
                FROM factura f JOIN venta v ON v.id_factura = f.id_factura
                WHERE f.fecha_factura >= @desde AND f.fecha_factura < @hasta
                GROUP BY DATE(f.fecha_factura) ORDER BY DATE(f.fecha_factura);";
            cmd.Parameters.AddWithValue("@desde", desde.Date);
            cmd.Parameters.AddWithValue("@hasta", hasta.Date);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(new ResumenVentaDiaria
            {
                Fecha = reader.GetDateTime(0), Unidades = reader.GetInt32(1), TotalVendido = reader.GetDecimal(2)
            });
            return lista;
        }
    }
}
