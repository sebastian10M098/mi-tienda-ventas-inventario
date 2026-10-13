using System.Security.Cryptography;
using System.Text;

namespace MiTienda.Data
{
    internal static class PasswordSecurity
    {
        private const int Iteraciones = 210_000;
        private const int LongitudSalt = 16;
        private const int LongitudHash = 32;
        private const string Prefijo = "pbkdf2-sha256";

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(LongitudSalt);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password), salt, Iteraciones, HashAlgorithmName.SHA256, LongitudHash);
            return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verificar(string password, string almacenada, out bool requiereActualizacion)
        {
            requiereActualizacion = false;
            string[] partes = almacenada.Split('$');
            if (partes.Length == 4 && partes[0] == Prefijo &&
                int.TryParse(partes[1], out int iteraciones) && iteraciones is >= 100_000 and <= 1_000_000)
            {
                try
                {
                    byte[] salt = Convert.FromBase64String(partes[2]);
                    byte[] hashEsperado = Convert.FromBase64String(partes[3]);
                    if (salt.Length != LongitudSalt || hashEsperado.Length != LongitudHash) return false;
                    byte[] hashIngresado = Rfc2898DeriveBytes.Pbkdf2(
                        Encoding.UTF8.GetBytes(password), salt, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);
                    return CryptographicOperations.FixedTimeEquals(hashIngresado, hashEsperado);
                }
                catch (FormatException) { return false; }
                catch (ArgumentException) { return false; }
            }

            // Actualiza gradualmente las contraseñas de texto plano de bases antiguas
            // cuando el usuario se autentica correctamente.
            byte[] entrada = Encoding.UTF8.GetBytes(password);
            byte[] guardada = Encoding.UTF8.GetBytes(almacenada);
            if (entrada.Length != guardada.Length || !CryptographicOperations.FixedTimeEquals(entrada, guardada))
                return false;
            requiereActualizacion = true;
            return true;
        }
    }
}
