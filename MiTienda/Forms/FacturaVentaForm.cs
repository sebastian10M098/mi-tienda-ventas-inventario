using MiTienda.Models;
using MiTienda.UI;

namespace MiTienda.Forms
{
    public class FacturaVentaForm : Form
    {
        public FacturaVentaForm(VentaLinea venta) : this(new Factura
        {
            Id = venta.FacturaId == 0 ? venta.Id : venta.FacturaId,
            Fecha = venta.FechaVenta,
            Subtotal = venta.Subtotal,
            Iva = venta.ValorIva,
            Total = venta.Total,
            Lineas = new List<VentaLinea> { venta },
        }) { }

        public FacturaVentaForm(Factura factura)
        {
            Text = $"Factura #{factura.Id}";
            Size = new Size(620, 520);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.BackgroundDark;

            var titulo = new Label
            {
                Text = $"Factura #{factura.Id}  ·  {factura.Fecha:dd/MM/yyyy HH:mm}",
                Dock = DockStyle.Top, Height = 48, Padding = new Padding(20, 12, 0, 0),
                Font = Theme.FontTitle, ForeColor = Theme.TextPrimary,
            };
            var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true };
            StyleHelpers.ApplyDarkGrid(grid);
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", DataPropertyName = "ProductoNombre", FillWeight = 180 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cant.", DataPropertyName = "Cantidad", FillWeight = 45 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Precio", DataPropertyName = "PrecioUnitario", FillWeight = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Subtotal", DataPropertyName = "Subtotal", FillWeight = 75, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            grid.DataSource = factura.Lineas;

            var pie = new Panel { Dock = DockStyle.Bottom, Height = 96, BackColor = Theme.BackgroundDark, Padding = new Padding(20, 8, 20, 8) };
            var resumen = new Label
            {
                Text = $"Subtotal: {factura.Subtotal:C2}     IVA: {factura.Iva:C2}     Total: {factura.Total:C2}",
                AutoSize = true, Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Location = new Point(20, 12),
            };
            var cerrar = StyleHelpers.CreateButton("Cerrar", Theme.InputBackground, Theme.TextPrimary);
            cerrar.DialogResult = DialogResult.OK; cerrar.Location = new Point(20, 50);
            pie.Controls.AddRange(new Control[] { resumen, cerrar });
            Controls.Add(grid); Controls.Add(pie); Controls.Add(titulo);
        }
    }
}
