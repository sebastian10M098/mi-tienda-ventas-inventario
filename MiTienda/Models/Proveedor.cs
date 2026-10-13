namespace MiTienda.Models
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public bool Estado { get; set; } = true;

        public override string ToString() => Nombre; // para que se vea bien en ComboBox
    }
}
