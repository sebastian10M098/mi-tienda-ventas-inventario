namespace MiTienda.Models
{
    public class Movimiento
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty; // ENTRADA / SALIDA
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string? Observacion { get; set; }
    }
}
