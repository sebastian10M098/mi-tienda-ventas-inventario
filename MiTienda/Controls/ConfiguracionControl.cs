using MiTienda.Data;
using MiTienda.UI;

namespace MiTienda.Controls
{
    public class ConfiguracionControl : UserControl
    {
        public ConfiguracionControl()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundDark;

            var panel = new Panel
            {
                Dock = DockStyle.Top, Height = 180, BackColor = Theme.PanelBackground,
                Padding = new Padding(22), Margin = new Padding(0)
            };
            var titulo = new Label
            {
                Text = "⚙️  Configuración", AutoSize = true,
                Font = Theme.FontHeading, ForeColor = Theme.TextPrimary,
                Location = new Point(22, 18)
            };
            var detalle = new Label
            {
                Text = "Administra el acceso de administrador utilizado para proteger operaciones sensibles.",
                AutoSize = true, Font = Theme.FontRegular, ForeColor = Theme.TextSecondary,
                Location = new Point(22, 58)
            };
            var cambiarClave = StyleHelpers.CreateButton("Cambiar contraseña de administrador", Theme.AccentBlue);
            cambiarClave.Location = new Point(22, 100);
            cambiarClave.Click += (_, _) => CambiarClave();
            panel.Controls.AddRange(new Control[] { titulo, detalle, cambiarClave });
            Controls.Add(panel);
        }

        private void CambiarClave()
        {
            using var dialogo = new Form
            {
                Text = "Cambiar contraseña de administrador", Size = new Size(440, 340),
                StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false, BackColor = Theme.BackgroundDark
            };
            var actual = Campo(dialogo, "Contraseña actual", 22);
            var nueva = Campo(dialogo, "Nueva contraseña (mínimo 12 caracteres)", 86);
            var confirmar = Campo(dialogo, "Confirmar nueva contraseña", 150);
            var guardar = StyleHelpers.CreateButton("Guardar", Theme.AccentGreen);
            guardar.Location = new Point(265, 220);
            guardar.Click += (_, _) =>
            {
                if (nueva.Text.Length < 12)
                {
                    MessageBox.Show("Usa al menos 12 caracteres para la nueva contraseña.", "Contraseña muy corta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!string.Equals(nueva.Text, confirmar.Text, StringComparison.Ordinal))
                {
                    MessageBox.Show("La confirmación no coincide con la nueva contraseña.", "Revisa los datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    if (!new UsuarioRepository().CambiarClaveAdministrador(actual.Text, nueva.Text))
                    {
                        MessageBox.Show("La contraseña actual no es válida.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    MessageBox.Show("La contraseña de administrador se actualizó.", "Configuración guardada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dialogo.DialogResult = DialogResult.OK;
                    dialogo.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo cambiar la contraseña.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            var cancelar = StyleHelpers.CreateButton("Cancelar", Theme.InputBackground, Theme.TextPrimary);
            cancelar.Location = new Point(150, 220);
            cancelar.DialogResult = DialogResult.Cancel;
            dialogo.Controls.AddRange(new Control[] { guardar, cancelar });
            dialogo.AcceptButton = guardar;
            dialogo.CancelButton = cancelar;

            dialogo.ShowDialog(FindForm());
        }

        private static TextBox Campo(Form form, string titulo, int y)
        {
            var etiqueta = StyleHelpers.CreateFieldLabel(titulo);
            etiqueta.Location = new Point(24, y);
            var texto = StyleHelpers.CreateTextBox();
            texto.Location = new Point(24, y + 22);
            texto.Width = 370;
            texto.PasswordChar = '●';
            form.Controls.Add(etiqueta);
            form.Controls.Add(texto);
            return texto;
        }
    }
}
