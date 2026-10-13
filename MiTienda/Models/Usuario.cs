namespace MiTienda.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = "EMPLEADO";
        public bool Estado { get; set; } = true;
        public DateTime FechaRegistro { get; set; }

        public override string ToString() => NombreUsuario;
    }
}
