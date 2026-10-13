using MiTienda.Data;
using MiTienda.Forms;
using MiTienda.Models;
using MiTienda.UI;

namespace MiTienda.Controls
{
    public class ReportesControl : UserControl
    {
        private readonly VentaRepository _ventas = new();
        private ComboBox _periodo = null!;
        private DateTimePicker _fecha = null!;
        private GraficoBarras _barras = null!;
        private GraficoCircular _circular = null!;
        private DataGridView _historial = null!;
        private ContextMenuStrip _menuFactura = null!;
        public event EventHandler? DatosModificados;

        public ReportesControl()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundDark;
            Construir();
        }

        private void Construir()
        {
            var titulo = new Label { Text = "📊  Reportes", Dock = DockStyle.Top, Height = 42, Font = Theme.FontHeading, ForeColor = Theme.TextPrimary };
            var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, BackColor = Theme.BackgroundDark, FlowDirection = FlowDirection.LeftToRight };
            filtros.Controls.Add(StyleHelpers.CreateFieldLabel("Periodo"));
            _periodo = StyleHelpers.CreateComboBox(); _periodo.Width = 130;
            _periodo.DataSource = new[] { "Este mes", "Hoy", "Últimos 30 días", "Fecha exacta" };
            _periodo.SelectedIndexChanged += (_, _) => Actualizar();
            _fecha = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 120, Visible = false };
            _fecha.ValueChanged += (_, _) => Actualizar();
            _periodo.SelectedIndexChanged += (_, _) => _fecha.Visible = _periodo.Text == "Fecha exacta";
            filtros.Controls.Add(_periodo); filtros.Controls.Add(_fecha);

            var panelBarras = CrearTarjeta("Ventas por categoría", "Total vendido en el periodo seleccionado");
            _barras = new GraficoBarras { Dock = DockStyle.Fill };
            panelBarras.Controls.Add(_barras);
            var panelCircular = CrearTarjeta("Productos más vendidos", "Unidades vendidas en el periodo seleccionado");
            _circular = new GraficoCircular { Dock = DockStyle.Fill };
            panelCircular.Controls.Add(_circular);
            var graficos = new TableLayoutPanel { Dock = DockStyle.Top, Height = 470, ColumnCount = 2, Padding = new Padding(0, 6, 0, 8), BackColor = Theme.BackgroundDark };
            graficos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55)); graficos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            graficos.Controls.Add(panelBarras, 0, 0); graficos.Controls.Add(panelCircular, 1, 0);

            var subtitulo = new Label { Text = "🕐  Historial de facturas", Dock = DockStyle.Top, Height = 34, Font = Theme.FontTitle, ForeColor = Theme.TextPrimary };
            _historial = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true };
            StyleHelpers.ApplyDarkGrid(_historial);
            _historial.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Factura", DataPropertyName = "Id", FillWeight = 55 });
            _historial.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha", DataPropertyName = "Fecha", FillWeight = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            _historial.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Productos", DataPropertyName = "Productos", FillWeight = 65 });
            _historial.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unidades", DataPropertyName = "Unidades", FillWeight = 65 });
            _historial.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Total", FillWeight = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            _historial.Columns.Add(new DataGridViewButtonColumn { Name = "Factura", HeaderText = "Ver", Text = "🧾", UseColumnTextForButtonValue = true, FillWeight = 45 });
            _historial.CellContentClick += AbrirFactura;
            _menuFactura = new ContextMenuStrip();
            _menuFactura.Items.Add("Eliminar factura permanentemente", null, (_, _) => EliminarFacturaSeleccionada());
            _historial.CellMouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
                _historial.ClearSelection();
                _historial.Rows[e.RowIndex].Selected = true;
                _historial.CurrentCell = _historial.Rows[e.RowIndex].Cells[0];
                _menuFactura.Show(_historial, e.Location);
            };

            Controls.Add(_historial); Controls.Add(subtitulo); Controls.Add(graficos); Controls.Add(filtros); Controls.Add(titulo);
        }

        private Panel CrearTarjeta(string titulo, string detalle)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.PanelBackground, Margin = new Padding(0, 0, 10, 0), Padding = new Padding(14, 12, 14, 12) };
            panel.Controls.Add(new Label { Text = detalle, Dock = DockStyle.Top, Height = 22, ForeColor = Theme.TextSecondary, Font = Theme.FontSmall });
            panel.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Top, Height = 28, ForeColor = Theme.TextPrimary, Font = Theme.FontTitle });
            return panel;
        }

        public void Actualizar()
        {
            try
            {
                DateTime desde = _periodo.Text switch
                {
                    "Hoy" => DateTime.Today,
                    "Últimos 30 días" => DateTime.Today.AddDays(-29),
                    "Fecha exacta" => _fecha.Value.Date,
                    _ => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                };
                DateTime hasta = _periodo.Text switch
                {
                    "Fecha exacta" => desde.AddDays(1),
                    "Este mes" => desde.AddMonths(1),
                    _ => DateTime.Today.AddDays(1),
                };
                _barras.Datos = _ventas.VentasPorCategoria(desde, hasta);
                _circular.Datos = _ventas.ProductosMasVendidos(desde, hasta);
                _historial.DataSource = _ventas.ObtenerFacturas(desde, hasta);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los reportes.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFactura(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _historial.Columns[e.ColumnIndex].Name != "Factura") return;
            if (_historial.Rows[e.RowIndex].DataBoundItem is not Factura resumen) return;
            var factura = _ventas.ObtenerFactura(resumen.Id);
            if (factura == null) return;
            using var ventana = new FacturaVentaForm(factura); ventana.ShowDialog(FindForm());
        }

        private void EliminarFacturaSeleccionada()
        {
            if (_historial.CurrentRow?.DataBoundItem is not Factura factura) return;
            var confirmar = MessageBox.Show(
                $"¿Eliminar permanentemente la factura #{factura.Id} por {factura.Total:C2}?\n\n" +
                "Se borrará del historial y se devolverán las unidades al inventario.",
                "Confirmar eliminación permanente", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmar != DialogResult.Yes || !SolicitarClaveEliminarFactura()) return;

            try
            {
                if (!_ventas.EliminarFacturaPermanentemente(factura.Id))
                {
                    MessageBox.Show("La factura ya no existe.", "No se pudo eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Actualizar();
                    return;
                }
                Actualizar();
                DatosModificados?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("La factura fue eliminada y las unidades volvieron al inventario.", "Factura eliminada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo eliminar la factura. No se guardaron cambios.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool SolicitarClaveEliminarFactura()
        {
            using var dialogo = new Form
            {
                Text = "Confirmar eliminación",
                Size = new Size(370, 190),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Theme.BackgroundDark,
            };
            var etiqueta = StyleHelpers.CreateFieldLabel("Ingresa la contraseña para eliminar la factura");
            etiqueta.Location = new Point(22, 18);
            var clave = StyleHelpers.CreateTextBox();
            clave.Location = new Point(22, 46);
            clave.Width = 310;
            clave.PasswordChar = '●';
            var aceptar = StyleHelpers.CreateButton("Confirmar", Theme.AccentRed);
            aceptar.Location = new Point(196, 91);
            aceptar.DialogResult = DialogResult.OK;
            var cancelar = StyleHelpers.CreateButton("Cancelar", Theme.InputBackground, Theme.TextPrimary);
            cancelar.Location = new Point(86, 91);
            cancelar.DialogResult = DialogResult.Cancel;
            dialogo.Controls.AddRange(new Control[] { etiqueta, clave, aceptar, cancelar });
            dialogo.AcceptButton = aceptar;
            dialogo.CancelButton = cancelar;
            if (dialogo.ShowDialog(FindForm()) != DialogResult.OK) return false;
            try
            {
                if (new UsuarioRepository().ValidarClaveAdministrador(clave.Text)) return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo validar la cuenta de administrador.\n\n{ex.Message}", "Error de acceso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            MessageBox.Show("La contraseña no es correcta. La factura se conservó.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }

    public class GraficoBarras : Control
    {
        private List<ResumenCategoria> _datos = new();
        public List<ResumenCategoria> Datos { get => _datos; set { _datos = value; Invalidate(); } }
        public GraficoBarras() { DoubleBuffered = true; BackColor = Theme.PanelBackground; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); var g = e.Graphics; if (_datos.Count == 0) { TextoVacio(g); return; }
            decimal max = _datos.Max(x => x.Total); int baseY = Height - 50; int areaH = Math.Max(1, baseY - 25); int ancho = Math.Max(35, (Width - 30) / _datos.Count - 16);
            Color[] colores = { Theme.AccentBlue, Color.FromArgb(33, 197, 178), Theme.AccentOrange, Theme.AccentPurple, Color.FromArgb(244, 63, 94) };
            for (int i = 0; i < _datos.Count; i++) { var d = _datos[i]; int x = 24 + i * (ancho + 16); int alto = Math.Max(2, (int)(d.Total / max * areaH)); using var brocha = new SolidBrush(colores[i % colores.Length]); g.FillRectangle(brocha, x, baseY - alto, ancho, alto); DibujarCentrado(g, d.Total.ToString("C0"), new Rectangle(x - 15, baseY - alto - 22, ancho + 30, 20), Theme.TextPrimary, Theme.FontSmall); DibujarCentrado(g, d.Nombre, new Rectangle(x - 15, baseY + 5, ancho + 30, 35), Theme.TextSecondary, Theme.FontSmall); }
        }
        private void TextoVacio(Graphics g) => DibujarCentrado(g, "Aún no hay ventas para este periodo.", ClientRectangle, Theme.TextMuted, Theme.FontRegular);
        private static void DibujarCentrado(Graphics g, string texto, Rectangle rect, Color color, Font fuente) => TextRenderer.DrawText(g, texto, fuente, rect, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
    }

    public class GraficoCircular : Control
    {
        private List<ResumenProducto> _datos = new();
        public List<ResumenProducto> Datos { get => _datos; set { _datos = value; Invalidate(); } }
        public GraficoCircular() { DoubleBuffered = true; BackColor = Theme.PanelBackground; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); var g = e.Graphics; if (_datos.Count == 0) { TextRenderer.DrawText(g, "Aún no hay ventas para este periodo.", Theme.FontRegular, ClientRectangle, Theme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); return; }
            Color[] colores = { Theme.AccentBlue, Color.FromArgb(33, 197, 178), Theme.AccentOrange, Theme.AccentPurple, Color.FromArgb(244, 63, 94) }; int total = _datos.Sum(x => x.Unidades); var rect = new Rectangle(24, 35, Math.Min(180, Height - 65), Math.Min(180, Height - 65)); float angulo = -90;
            for (int i = 0; i < _datos.Count; i++) { float tramo = 360f * _datos[i].Unidades / total; using var brocha = new SolidBrush(colores[i % colores.Length]); g.FillPie(brocha, rect, angulo, tramo); angulo += tramo; }
            int y = 38; for (int i = 0; i < _datos.Count; i++) { using var brocha = new SolidBrush(colores[i % colores.Length]); g.FillEllipse(brocha, 230, y + 3, 10, 10); TextRenderer.DrawText(g, $"{_datos[i].Nombre}  ·  {_datos[i].Unidades}", Theme.FontSmall, new Point(247, y), Theme.TextPrimary); y += 29; }
        }
    }
}
