using MiTienda.Models;
using MiTienda.UI;

namespace MiTienda.Forms
{
    public class ReposicionStockForm : Form
    {
        private readonly NumericUpDown _cantidad;
        public int CantidadRecibida => (int)_cantidad.Value;

        public ReposicionStockForm(Producto producto)
        {
            Text = "Actualizar stock";
            Size = new Size(420, 235);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.BackgroundDark;

            var productoLabel = new Label
            {
                Text = $"{producto.Nombre}\nStock actual: {producto.Stock}",
                ForeColor = Theme.TextPrimary,
                Font = Theme.FontSemibold,
                Location = new Point(24, 22),
                Size = new Size(350, 44),
            };
            var etiqueta = StyleHelpers.CreateFieldLabel("Unidades recibidas");
            etiqueta.Location = new Point(24, 78);
            _cantidad = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 999999,
                Value = 1,
                Location = new Point(24, 100),
                Width = 160,
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
            };
            var guardar = StyleHelpers.CreateButton("Actualizar", Theme.AccentGreen);
            guardar.Location = new Point(202, 144);
            guardar.DialogResult = DialogResult.OK;
            var cancelar = StyleHelpers.CreateButton("Cancelar", Theme.InputBackground, Theme.TextPrimary);
            cancelar.Location = new Point(88, 144);
            cancelar.DialogResult = DialogResult.Cancel;
            Controls.AddRange(new Control[] { productoLabel, etiqueta, _cantidad, guardar, cancelar });
            AcceptButton = guardar;
            CancelButton = cancelar;
        }
    }
}
