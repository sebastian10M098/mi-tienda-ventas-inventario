using MiTienda.Controls;
using MiTienda.UI;

namespace MiTienda.Forms
{
    public class GestionProductosForm : Form
    {
        public event EventHandler? DatosModificados;

        public GestionProductosForm()
        {
            Text = "Gestión de productos";
            Size = new Size(1180, 760);
            MinimumSize = new Size(900, 620);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.BackgroundDark;

            var gestion = new ProductosControl(modoGestion: true);
            gestion.DatosModificados += (_, _) => DatosModificados?.Invoke(this, EventArgs.Empty);
            Controls.Add(gestion);
        }
    }
}
