using MiTienda.Data;
using MiTienda.Models;
using MiTienda.UI;

namespace MiTienda.Controls
{
    public class InicioControl : UserControl
    {
        private readonly VentaRepository _ventas = new();
        private readonly ProductoRepository _productos = new();
        private ComboBox _periodo = null!;
        private GraficoVentasInicio _graficoVentas = null!;
        private GraficoInventarioInicio _graficoInventario = null!;

        public InicioControl()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundDark;
            Construir();
            Actualizar();
        }

        private void Construir()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2,
                BackColor = Theme.BackgroundDark, Padding = new Padding(0, 0, 0, 4)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 49));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 51));

            var ventasPanel = CrearPanel();
            layout.SetColumnSpan(ventasPanel, 2);
            var ventasTitulo = new Label { Text = "📊  Ventas por período", Dock = DockStyle.Top, Height = 32, ForeColor = Theme.TextPrimary, Font = Theme.FontTitle };
            _periodo = StyleHelpers.CreateComboBox();
            _periodo.Width = 132;
            _periodo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _periodo.Location = new Point(ventasPanel.Width - 150, 10);
            _periodo.DataSource = new[] { "Mes", "Últimos 7 días", "Año" };
            _periodo.SelectedIndexChanged += (_, _) => ActualizarVentas();
            ventasPanel.Resize += (_, _) => _periodo.Location = new Point(Math.Max(10, ventasPanel.Width - _periodo.Width - 14), 10);
            _graficoVentas = new GraficoVentasInicio { Dock = DockStyle.Fill };
            ventasPanel.Controls.Add(_graficoVentas);
            ventasPanel.Controls.Add(_periodo);
            ventasPanel.Controls.Add(ventasTitulo);

            var finanzasPanel = CrearPanel();
            finanzasPanel.Controls.Add(new InicioFinanzasResumen(_ventas) { Dock = DockStyle.Fill });
            finanzasPanel.Controls.Add(new Label { Text = "💰  Resumen financiero del día", Dock = DockStyle.Top, Height = 32, ForeColor = Theme.TextPrimary, Font = Theme.FontTitle });

            var inventarioPanel = CrearPanel();
            _graficoInventario = new GraficoInventarioInicio { Dock = DockStyle.Fill };
            inventarioPanel.Controls.Add(_graficoInventario);
            inventarioPanel.Controls.Add(new Label { Text = "📦  Estado del inventario", Dock = DockStyle.Top, Height = 32, ForeColor = Theme.TextPrimary, Font = Theme.FontTitle });

            layout.Controls.Add(ventasPanel, 0, 0);
            layout.Controls.Add(finanzasPanel, 0, 1);
            layout.Controls.Add(inventarioPanel, 1, 1);
            Controls.Add(layout);
        }

        private static Panel CrearPanel() => new()
        {
            Dock = DockStyle.Fill, BackColor = Theme.PanelBackground,
            Margin = new Padding(0, 0, 10, 10), Padding = new Padding(14, 10, 14, 10)
        };

        public void Actualizar()
        {
            ActualizarVentas();
            try { _graficoInventario.Resumen = _productos.ObtenerResumenInventario(); }
            catch { _graficoInventario.Resumen = new ResumenInventario(); }
            foreach (var resumen in Controls.OfType<TableLayoutPanel>().SelectMany(p => p.Controls.OfType<Panel>()).SelectMany(p => p.Controls.OfType<InicioFinanzasResumen>()))
                resumen.Actualizar();
        }

        private void ActualizarVentas()
        {
            if (_graficoVentas == null || _periodo == null) return;
            try
            {
                DateTime hoy = DateTime.Today;
                var datos = _periodo.Text switch
                {
                    "Últimos 7 días" => _ventas.VentasDiarias(hoy.AddDays(-6), hoy.AddDays(1)),
                    "Año" => _ventas.VentasDiarias(new DateTime(hoy.Year, 1, 1), new DateTime(hoy.Year + 1, 1, 1)),
                    _ => _ventas.VentasDiarias(new DateTime(hoy.Year, hoy.Month, 1), new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1)),
                };
                _graficoVentas.EstablecerDatos(datos, _periodo.Text);
            }
            catch { _graficoVentas.EstablecerDatos(new List<ResumenVentaDiaria>(), _periodo.Text); }
        }
    }

    internal class InicioFinanzasResumen : Control
    {
        private readonly VentaRepository _ventas;
        private decimal _totalVentas;
        private int _cantidadFacturas;
        public InicioFinanzasResumen() : this(new VentaRepository()) { }
        public InicioFinanzasResumen(VentaRepository ventas) { _ventas = ventas; DoubleBuffered = true; BackColor = Theme.PanelBackground; }
        public void Actualizar()
        {
            try { _totalVentas = _ventas.TotalVendidoHoy(); _cantidadFacturas = _ventas.ContarVentasHoy(); }
            catch { _totalVentas = 0; _cantidadFacturas = 0; }
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics; int gap = 12; int alto = Math.Max(84, Height - 24);
            int anchoVentas = Math.Max(130, (Width - gap) * 2 / 3);
            int anchoFacturas = Math.Max(100, Width - gap - anchoVentas);
            DibujarTarjeta(g, new Rectangle(0, 8, anchoVentas, alto), "Total vendido hoy", _totalVentas.ToString("C2", new System.Globalization.CultureInfo("en-US")), Theme.AccentGreen, "🪙");
            DibujarTarjeta(g, new Rectangle(anchoVentas + gap, 8, anchoFacturas, alto), "Facturas de hoy", _cantidadFacturas.ToString("N0"), Theme.AccentBlue, "🧾");
        }
        private static void DibujarTarjeta(Graphics g, Rectangle r, string titulo, string valor, Color color, string icono)
        {
            using var path = RoundedRect(r, 10); using var fondo = new SolidBrush(Color.FromArgb(23, 34, 53)); g.FillPath(fondo, path);
            using var borde = new Pen(Color.FromArgb(70, color), 1); g.DrawPath(borde, path);
            TextRenderer.DrawText(g, icono, Theme.FontTitle, new Rectangle(r.X + 12, r.Y + 8, 30, 28), color, TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, titulo, Theme.FontSmall, new Rectangle(r.X + 48, r.Y + 8, r.Width - 60, 28), Theme.TextSecondary, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, valor, Theme.FontCardValue, new Rectangle(r.X + 12, r.Y + 46, r.Width - 24, Math.Max(30, r.Height - 54)), Theme.TextPrimary, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath(); int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90); path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); path.AddArc(r.X, r.Bottom - d, d, d, 90, 90); path.CloseFigure(); return path;
        }
    }

    internal class GraficoVentasInicio : Control
    {
        private List<ResumenVentaDiaria> _datos = new();
        private string _periodo = "Mes";
        public GraficoVentasInicio() { DoubleBuffered = true; BackColor = Theme.PanelBackground; }
        public void EstablecerDatos(List<ResumenVentaDiaria> datos, string periodo) { _datos = datos; _periodo = periodo; Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); var g = e.Graphics; g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var datos = _datos;
            if (_periodo == "Año")
                datos = _datos.GroupBy(x => new DateTime(x.Fecha.Year, x.Fecha.Month, 1)).Select(x => new ResumenVentaDiaria { Fecha = x.Key, Unidades = x.Sum(y => y.Unidades), TotalVendido = x.Sum(y => y.TotalVendido) }).ToList();
            if (datos.Count == 0) { TextRenderer.DrawText(g, "Aún no hay ventas para este período.", Theme.FontRegular, ClientRectangle, Theme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); return; }
            var area = new Rectangle(48, 12, Math.Max(40, Width - 66), Math.Max(50, Height - 48));
            int n = _periodo == "Año" ? 12 : _periodo == "Últimos 7 días" ? 7 : DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
            DateTime inicio = _periodo == "Año" ? new DateTime(DateTime.Today.Year, 1, 1) : _periodo == "Últimos 7 días" ? DateTime.Today.AddDays(-6) : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var points = new List<PointF>(); decimal maxU = Math.Max(1, datos.Max(x => x.Unidades)); decimal maxV = Math.Max(1, datos.Max(x => x.TotalVendido));
            int baseY = area.Bottom;
            using var gridPen = new Pen(Color.FromArgb(45, 73, 104), 1);
            for (int row = 0; row <= 4; row++) { int y = area.Top + area.Height * row / 4; g.DrawLine(gridPen, area.Left, y, area.Right, y); TextRenderer.DrawText(g, (maxU * (4 - row) / 4).ToString("0"), Theme.FontSmall, new Rectangle(0, y - 9, 42, 18), Theme.TextSecondary, TextFormatFlags.Right | TextFormatFlags.VerticalCenter); }
            float step = area.Width / (float)n; int barW = Math.Max(4, (int)(step * 0.62));
            for (int i = 0; i < n; i++)
            {
                DateTime fecha = _periodo == "Año" ? inicio.AddMonths(i) : inicio.AddDays(i);
                var d = datos.FirstOrDefault(x => x.Fecha.Date == fecha.Date);
                int unidades = d?.Unidades ?? 0; decimal total = d?.TotalVendido ?? 0;
                int h = (int)(unidades / maxU * area.Height); int x = area.Left + (int)(i * step + (step - barW) / 2);
                using var bar = new SolidBrush(Color.FromArgb(30, 126, 238)); g.FillRectangle(bar, x, baseY - h, barW, h);
                float py = area.Bottom - (float)(total / maxV * area.Height); points.Add(new PointF(area.Left + i * step + step / 2, py));
                if (i % Math.Max(1, n / 10) == 0 || i == n - 1)
                {
                    string label = _periodo == "Año" ? fecha.ToString("MMM", new System.Globalization.CultureInfo("es-ES")) : fecha.ToString("dd");
                    TextRenderer.DrawText(g, label, Theme.FontSmall, new Rectangle(area.Left + (int)(i * step) - 8, baseY + 4, (int)step + 16, 18), Theme.TextSecondary, TextFormatFlags.HorizontalCenter);
                }
            }
            if (points.Count > 1) { using var pen = new Pen(Color.FromArgb(24, 214, 237), 2.2f); g.DrawLines(pen, points.ToArray()); }
            using var dot = new SolidBrush(Color.FromArgb(24, 214, 237)); foreach (var p in points) g.FillEllipse(dot, p.X - 3, p.Y - 3, 6, 6);
            int legendY = Height - 18;
            using var blue = new SolidBrush(Color.FromArgb(30, 126, 238)); g.FillRectangle(blue, area.Left + 10, legendY + 4, 11, 9);
            TextRenderer.DrawText(g, "Ventas (unidades)", Theme.FontSmall, new Point(area.Left + 26, legendY), Theme.TextSecondary);
            using var cyan = new SolidBrush(Color.FromArgb(24, 214, 237)); g.FillEllipse(cyan, area.Left + 160, legendY + 5, 8, 8);
            TextRenderer.DrawText(g, "Total vendido", Theme.FontSmall, new Point(area.Left + 174, legendY), Theme.TextSecondary);
        }
    }

    internal class GraficoInventarioInicio : Control
    {
        private ResumenInventario _resumen = new();
        public GraficoInventarioInicio() { DoubleBuffered = true; BackColor = Theme.PanelBackground; }
        public ResumenInventario Resumen { get => _resumen; set { _resumen = value; Invalidate(); } }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); var g = e.Graphics; g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var colors = new[] { Color.FromArgb(5, 190, 151), Color.FromArgb(250, 182, 45), Color.FromArgb(248, 62, 72) };
            int total = _resumen.Total;
            int size = Math.Max(72, Math.Min(142, Math.Min(Height - 30, Width - 220)));
            int x = 8, y = Math.Max(8, (Height - size) / 2);
            var ring = new Rectangle(x, y, size, size); float angle = -90;
            int[] values = { _resumen.ConStock, _resumen.StockCritico, _resumen.Agotados };
            if (total == 0) { using var empty = new Pen(Theme.BorderColor, 13); g.DrawEllipse(empty, ring); }
            else for (int i = 0; i < values.Length; i++) { float sweep = values[i] * 360f / total; using var pen = new Pen(colors[i], 14) { StartCap = System.Drawing.Drawing2D.LineCap.Flat, EndCap = System.Drawing.Drawing2D.LineCap.Flat }; g.DrawArc(pen, ring, angle, sweep); angle += sweep; }
            var center = new Rectangle(x + 12, y + size / 2 - 30, size - 24, 60);
            TextRenderer.DrawText(g, $"Total de\nproductos\n{total:N0}", Theme.FontSmall, center, Theme.TextPrimary, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            string[] labels = { "Con stock", "Stock crítico", "Agotados" };
            int legendX = size + 28; int rowH = Math.Max(29, (Height - 16) / 3);
            for (int i = 0; i < labels.Length; i++)
            {
                int ly = 8 + i * rowH; using var brush = new SolidBrush(colors[i]); g.FillEllipse(brush, legendX, ly + Math.Max(5, (rowH - 14) / 2), 14, 14);
                int valueX = Math.Max(legendX + 74, Width - 54);
                int labelWidth = Math.Max(48, valueX - legendX - 30);
                TextRenderer.DrawText(g, labels[i], Theme.FontSmall, new Rectangle(legendX + 22, ly, labelWidth, rowH), Theme.TextPrimary, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(g, values[i].ToString("N0"), Theme.FontSemibold, new Rectangle(valueX, ly, Math.Max(38, Width - valueX - 4), rowH), Theme.TextPrimary, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
