namespace MiTienda.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public override string ToString() => Nombre; // para que se vea bien en ComboBox
    }
}
