using MiTienda.Controls;
using MiTienda.Data;
using MiTienda.Forms;
using MiTienda.UI;

namespace MiTienda
{
    public partial class MainForm : Form
    {
        private readonly ProductoRepository _productoRepo = new();
        private readonly VentaRepository _ventaRepo = new();
        private readonly List<SidebarButton> _sidebarButtons = new();

        private Panel _contentPanel = null!;
        private Panel _cardsRow = null!;
        private TableLayoutPanel _rightLayout = null!;

        private DashboardCard _cardProductos = null!;
        private DashboardCard _cardVentasHoy = null!;
        private DashboardCard _cardTotalVendido = null!;
        private DashboardCard _cardStockAgotado = null!;

        private ProductosControl? _productosControl;
        private VentasControl? _ventasControl;
        private ReportesControl? _reportesControl;
        private InicioControl? _inicioControl;
        private ConfiguracionControl? _configuracionControl;

        public MainForm()
        {
            Text = "Mi Tienda - Sistema de Ventas e Inventario";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1400, 900);
            MinimumSize = new Size(1100, 700);
            BackColor = Theme.BackgroundDark;
            Font = Theme.FontRegular;

            DatabaseInitializer.VerificarConexion();

            BuildLayout();
            SelectSection("Inicio");
            CargarDatosDashboard();
        }

        private void BuildLayout()
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Theme.SidebarBackground,
                Padding = new Padding(0, 16, 0, 16)
            };

            var logoPanel = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Theme.SidebarBackground };
            var logoLabel = new Label
            {
                Text = "🛒 Mi Tienda",
                Font = Theme.FontTitle,
                ForeColor = Theme.TextPrimary,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };
            logoPanel.Controls.Add(logoLabel);

            var menuItems = new (string Icon, string Text)[]
            {
                ("🏠", "Inicio"),
                ("🛒", "Ventas"),
                ("📦", "Productos"),
                ("📊", "Reportes"),
                ("⚙️", "Configuración"),
            };

            var menuPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = menuItems.Length * 44 + 20,
                Padding = new Padding(10, 10, 10, 0),
                BackColor = Theme.SidebarBackground
            };

            for (int i = menuItems.Length - 1; i >= 0; i--)
            {
                var (icon, text) = menuItems[i];
                var btn = new SidebarButton(icon, text);
                btn.Activated += (_, _) => SelectSection(text);
                _sidebarButtons.Add(btn);
                menuPanel.Controls.Add(btn);
            }

            sidebar.Controls.Add(menuPanel);
            sidebar.Controls.Add(logoPanel);

            var topBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Theme.TopBarBackground, Padding = new Padding(20, 0, 20, 0) };
            var dateLabel = new Label
            {
                Text = DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES")),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
            };
            var userLabel = new Label
            {
                Text = "👤 Administrador",
                Font = Theme.FontSemibold,
                ForeColor = Theme.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
            };
            topBar.Resize += (_, _) =>
            {
                dateLabel.Location = new Point(topBar.Width - dateLabel.Width - 20, 18);
                userLabel.Location = new Point(dateLabel.Left - userLabel.Width - 24, 18);
            };
            topBar.Controls.Add(dateLabel);
            topBar.Controls.Add(userLabel);

            _cardsRow = new Panel { Dock = DockStyle.Top, Height = 122, BackColor = Theme.BackgroundDark, Padding = new Padding(20, 10, 20, 8) };

            _cardProductos = new DashboardCard("📦", "Productos registrados", "0", Theme.AccentBlue);
            _cardVentasHoy = new DashboardCard("🛒", "Ventas del día", "0", Theme.AccentGreen);
            _cardTotalVendido = new DashboardCard("💲", "Total vendido (hoy)", "$ 0.00", Theme.AccentPurple);
            _cardStockAgotado = new DashboardCard("⚠️", "Productos agotados", "0", Theme.AccentOrange);

            _cardsRow.Controls.Add(_cardProductos);
            _cardsRow.Controls.Add(_cardVentasHoy);
            _cardsRow.Controls.Add(_cardTotalVendido);
            _cardsRow.Controls.Add(_cardStockAgotado);
            _cardsRow.Resize += (_, _) => AcomodarTarjetasDashboard();
            AcomodarTarjetasDashboard();

            _contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BackgroundDark, Padding = new Padding(20, 16, 20, 20) };
            MostrarPlaceholder();

            _rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                BackColor = Theme.BackgroundDark, Margin = Padding.Empty, Padding = Padding.Empty
            };
            _rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            _rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
            _rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _rightLayout.Controls.Add(topBar, 0, 0);
            _rightLayout.Controls.Add(_cardsRow, 0, 1);
            _rightLayout.Controls.Add(_contentPanel, 0, 2);
            var rightArea = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BackgroundDark, Margin = Padding.Empty };
            rightArea.Controls.Add(_rightLayout);

            Controls.Add(rightArea);
            Controls.Add(sidebar);
        }

        private void AcomodarTarjetasDashboard()
        {
            if (_cardProductos == null) return;
            var tarjetas = new[] { _cardProductos, _cardVentasHoy, _cardTotalVendido, _cardStockAgotado };
            int gap = 14;
            int usableWidth = _cardsRow.ClientSize.Width - _cardsRow.Padding.Horizontal;
            int width = Math.Max(170, (usableWidth - gap * (tarjetas.Length - 1)) / tarjetas.Length);
            int height = Math.Max(84, _cardsRow.ClientSize.Height - _cardsRow.Padding.Vertical);
            for (int i = 0; i < tarjetas.Length; i++)
            {
                tarjetas[i].Size = new Size(width, height);
                tarjetas[i].Location = new Point(_cardsRow.Padding.Left + i * (width + gap), _cardsRow.Padding.Top);
            }
        }

        private void MostrarPlaceholder()
        {
            _contentPanel.Controls.Clear();
            var placeholder = new Label
            {
                Text = "Esta sección todavía no está implementada.",
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _contentPanel.Controls.Add(placeholder);
        }

        private void SelectSection(string sectionName)
        {
            foreach (var btn in _sidebarButtons)
            {
                btn.IsActive = string.Equals(
                    btn.Controls.OfType<Label>().Last().Text,
                    sectionName,
                    StringComparison.OrdinalIgnoreCase);
            }

            _cardsRow.Visible = sectionName == "Inicio";
            _rightLayout.RowStyles[1].Height = sectionName == "Inicio" ? 122 : 0;
            _rightLayout.RowStyles[1].SizeType = SizeType.Absolute;
            Text = $"Mi Tienda - {sectionName}";

            _contentPanel.Controls.Clear();

            switch (sectionName)
            {
                case "Productos":
                    _productosControl ??= CrearProductosControl();
                    _contentPanel.Controls.Add(_productosControl);
                    _productosControl.RestablecerFiltros();
                    break;

                case "Ventas":
                    _ventasControl ??= CrearVentasControl();
                    _contentPanel.Controls.Add(_ventasControl);
                    break;

                case "Reportes":
                    _reportesControl ??= CrearReportesControl();
                    _contentPanel.Controls.Add(_reportesControl);
                    _reportesControl.Actualizar();
                    break;

                case "Configuración":
                    _configuracionControl ??= new ConfiguracionControl();
                    _contentPanel.Controls.Add(_configuracionControl);
                    break;

                case "Inicio":
                    _inicioControl ??= new InicioControl();
                    _contentPanel.Controls.Add(_inicioControl);
                    _inicioControl.Actualizar();
                    CargarDatosDashboard();
                    break;

                default:
                    MostrarPlaceholder();
                    break;
            }
        }

        private ProductosControl CrearProductosControl()
        {
            var control = new ProductosControl();
            control.DatosModificados += (_, _) => CargarDatosDashboard();
            control.GestionSolicitada += (_, _) => AbrirGestionProductos();
            control.ProductoParaVenta += (_, producto) =>
            {
                SelectSection("Ventas");
                _ventasControl?.PrepararProductoParaVenta(producto);
            };
            return control;
        }

        private void AbrirGestionProductos()
        {
            if (!ProductosControl.SolicitarClave()) return;
            using var gestion = new GestionProductosForm();
            gestion.DatosModificados += (_, _) =>
            {
                _productosControl?.CargarProductos();
                CargarDatosDashboard();
            };
            gestion.ShowDialog(this);
        }

        private VentasControl CrearVentasControl()
        {
            var control = new VentasControl();
            control.VentaRegistrada += (_, _) => CargarDatosDashboard();
            return control;
        }

        private ReportesControl CrearReportesControl()
        {
            var control = new ReportesControl();
            control.DatosModificados += (_, _) => CargarDatosDashboard();
            return control;
        }

        public void ActualizarTarjetas(int productos, int ventasHoy, decimal totalVendido, int productosAgotados)
        {
            _cardProductos.Value = productos.ToString();
            _cardVentasHoy.Value = ventasHoy.ToString();
            _cardTotalVendido.Value = totalVendido.ToString("C2", new System.Globalization.CultureInfo("en-US"));
            _cardStockAgotado.Value = productosAgotados.ToString();
        }

        public void CargarDatosDashboard()
        {
            try
            {
                int totalProductos = _productoRepo.ContarProductos();
                int ventasHoy = _ventaRepo.ContarVentasHoy();
                decimal totalVendidoHoy = _ventaRepo.TotalVendidoHoy();
                int productosAgotados = _productoRepo.ContarProductosAgotados();
                ActualizarTarjetas(totalProductos, ventasHoy, totalVendidoHoy, productosAgotados);
                _inicioControl?.Actualizar();
            }
            catch (Exception)
            {
                ActualizarTarjetas(0, 0, 0m, 0);
            }
        }
    }
}
