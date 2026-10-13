namespace MiTienda.Models
{
    /// <summary>
    /// Representa una fila de la tabla 'venta': una venta = un producto + cantidad.
    /// Se usa tanto para leer el historial como para representar un ítem
    /// del carrito antes de registrar la venta.
    /// </summary>
    public class VentaLinea
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public DateTime FechaVenta { get; set; }
        public string? Observacion { get; set; }

        /// <summary>
        /// Precio del producto AL MOMENTO de mostrarlo en el carrito.
        /// La tabla 'venta' no guarda precio histórico, así que el subtotal
        /// se calcula con el precio actual del producto.
        /// </summary>
        public decimal PrecioUnitario { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Ganancia => (PrecioUnitario - CostoUnitario) * Cantidad;

        public decimal Subtotal => PrecioUnitario * Cantidad;
        public bool AplicaIva { get; set; }
        public decimal ValorIva { get; set; }
        public decimal Total => Subtotal + ValorIva;
    }
}
