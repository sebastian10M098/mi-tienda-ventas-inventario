namespace MiTienda.Models
{
    public class ResumenVentaDiaria
    {
        public DateTime Fecha { get; set; }
        public int Unidades { get; set; }
        public decimal TotalVendido { get; set; }
    }

    public class ResumenInventario
    {
        public int ConStock { get; set; }
        public int StockCritico { get; set; }
        public int Agotados { get; set; }
        public int Total => ConStock + StockCritico + Agotados;
    }

    public class Factura
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int Productos { get; set; }
        public int Unidades { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public List<VentaLinea> Lineas { get; set; } = new();
    }

    public class ResumenCategoria
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class ResumenProducto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Unidades { get; set; }
    }
}
