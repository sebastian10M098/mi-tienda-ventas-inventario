namespace MiTienda.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? UbicacionPercha { get; set; }
        public string? Codigo { get; set; }
        public string? TipoBanda { get; set; }
        public string? TipoOring { get; set; }
        public string? AplicacionVehiculo { get; set; }
        public string? Marca { get; set; }
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }
        public int Stock { get; set; }

        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;

        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;

        public string EstadoTexto => Stock <= 0 ? "Agotado" : "Disponible";
    }
}
