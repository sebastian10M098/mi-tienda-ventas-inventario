using MiTienda.Data;
using MiTienda.Forms;
using MiTienda.Models;
using MiTienda.UI;

namespace MiTienda.Controls
{
    public class VentasControl : UserControl
    {
        private readonly VentaRepository _ventaRepo = new();

        // TODO: cuando exista login, reemplazar por el usuario autenticado.
        private const int UsuarioActualId = 1;

        private readonly List<VentaLinea> _carrito = new();

        private NumericUpDown _numCantidad = null!;
        private Panel _confirmacionPanel = null!;
        private Label _lblProductoPendiente = null!;
        private Producto? _productoPendiente;
        private DataGridView _gridCarrito = null!;
        private DataGridView _gridHistorial = null!;
        private DateTimePicker _fechaHistorial = null!;
        private Button _btnIva = null!;
        private Label _lblSubtotal = null!;
        private Label _lblIva = null!;
        private Label _lblTotal = null!;
        private bool _aplicaIva;
        private const decimal PorcentajeIva = 0.15m;

        public event EventHandler? VentaRegistrada;

        public VentasControl()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundDark;
            BuildLayout();
        }

        private void BuildLayout()
        {
            var title = new Label
            {
                Text = "🛒  Ventas",
                Font = Theme.FontHeading,
                ForeColor = Theme.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Height = 40,
            };

            var contenido = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BackgroundDark };
            BuildCarritoPanel(contenido);
            Controls.Add(contenido);
            Controls.Add(title);
        }

        private void BuildCarritoPanel(Control parent)
        {
            var subtitle = new Label
            {
                Text = "Nueva venta",
                Font = Theme.FontTitle,
                ForeColor = Theme.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Height = 32,
            };

            _confirmacionPanel = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Theme.BackgroundDark, Visible = false };
            _lblProductoPendiente = StyleHelpers.CreateFieldLabel("Producto seleccionado");
            _lblProductoPendiente.Location = new Point(0, 4);

            _numCantidad = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Width = 60,
                Location = new Point(0, 29),
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
            };

            var cantidadLabel = StyleHelpers.CreateFieldLabel("Cantidad");
            cantidadLabel.Location = new Point(72, 31);
            var btnConfirmar = StyleHelpers.CreateButton("Aceptar y agregar", Theme.AccentGreen);
            btnConfirmar.Location = new Point(140, 25);
            btnConfirmar.Click += (_, _) => ConfirmarProductoPendiente();

            _confirmacionPanel.Controls.AddRange(new Control[] { _lblProductoPendiente, _numCantidad, cantidadLabel, btnConfirmar });

            _gridCarrito = new DataGridView { Dock = DockStyle.Fill };
            StyleHelpers.ApplyDarkGrid(_gridCarrito);
            _gridCarrito.ReadOnly = true;
            _gridCarrito.AutoGenerateColumns = false;
            _gridCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Producto", HeaderText = "Producto", DataPropertyName = "ProductoNombre", FillWeight = 160 });
            _gridCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", DataPropertyName = "PrecioUnitario", FillWeight = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            _gridCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Cant.", DataPropertyName = "Cantidad", FillWeight = 50 });
            _gridCarrito.Columns.Add(new DataGridViewTextBoxColumn { Name = "Subtotal", HeaderText = "Subtotal", DataPropertyName = "Subtotal", FillWeight = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 112, BackColor = Theme.BackgroundDark };

            var btnQuitar = StyleHelpers.CreateButton("Quitar seleccionado", Theme.InputBackground, Theme.TextPrimary);
            btnQuitar.Location = new Point(0, 4);
            btnQuitar.Click += (_, _) => QuitarDelCarrito();

            _btnIva = StyleHelpers.CreateButton("Agregar IVA (15%)", Theme.InputBackground, Theme.TextPrimary);
            _btnIva.Location = new Point(200, 4);
            _btnIva.Click += (_, _) => CalcularIva();

            _lblSubtotal = new Label
            {
                Text = "Subtotal: $ 0.00",
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary,
                AutoSize = true,
                Location = new Point(bottomPanel.Width - 150, 4),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _lblIva = new Label
            {
                Text = "IVA (0%): $ 0.00",
                Font = Theme.FontRegular,
                ForeColor = Theme.TextSecondary,
                AutoSize = true,
                Location = new Point(bottomPanel.Width - 150, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _lblTotal = new Label
            {
                Text = "Total: $ 0.00",
                Font = Theme.FontTitle,
                ForeColor = Theme.TextPrimary,
                AutoSize = true,
                Location = new Point(bottomPanel.Width - 150, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var btnRegistrar = StyleHelpers.CreateButton("🛒  Registrar venta ", Theme.AccentGreen);
            btnRegistrar.Location = new Point(0, 62);
            btnRegistrar.Width = 200;
            btnRegistrar.Click += (_, _) => RegistrarVenta();

            bottomPanel.Controls.Add(btnQuitar);
            bottomPanel.Controls.Add(_btnIva);
            bottomPanel.Controls.Add(_lblSubtotal);
            bottomPanel.Controls.Add(_lblIva);
            bottomPanel.Controls.Add(_lblTotal);
            bottomPanel.Controls.Add(btnRegistrar);

            parent.Controls.Add(_gridCarrito);
            parent.Controls.Add(bottomPanel);
            parent.Controls.Add(_confirmacionPanel);
            parent.Controls.Add(subtitle);
        }

        private void BuildHistorialPanel(Control parent)
        {
            var subtitle = new Label
            {
                Text = "🕐  Historial de ventas",
                Font = Theme.FontTitle,
                ForeColor = Theme.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Height = 32,
            };

            _gridHistorial = new DataGridView { Dock = DockStyle.Fill };
            StyleHelpers.ApplyDarkGrid(_gridHistorial);
            _gridHistorial.AutoGenerateColumns = false;
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fecha", HeaderText = "Fecha", DataPropertyName = "FechaVenta", FillWeight = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "Producto", HeaderText = "Producto", DataPropertyName = "ProductoNombre", FillWeight = 140 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Cant.", DataPropertyName = "Cantidad", FillWeight = 50 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total", DataPropertyName = "Total", FillWeight = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            _gridHistorial.Columns.Add(new DataGridViewButtonColumn { Name = "VerFactura", HeaderText = "Factura", Text = "🧾", UseColumnTextForButtonValue = true, FillWeight = 45, FlatStyle = FlatStyle.Flat });
            _gridHistorial.CellContentClick += GridHistorial_CellContentClick;

            var filtroPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 38,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Theme.BackgroundDark,
                Padding = new Padding(0, 2, 0, 2),
            };
            filtroPanel.Controls.Add(StyleHelpers.CreateFieldLabel("Fecha de venta"));
            _fechaHistorial = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false,
                Width = 125,
                Value = DateTime.Today,
                CalendarMonthBackground = Theme.InputBackground,
                CalendarForeColor = Theme.TextPrimary,
            };
            _fechaHistorial.ValueChanged += (_, _) => CargarHistorial();
            _fechaHistorial.MouseUp += (_, _) => BeginInvoke(new Action(CargarHistorial));
            filtroPanel.Controls.Add(_fechaHistorial);

            var limpiar = StyleHelpers.CreateButton("Ver todas", Theme.InputBackground, Theme.TextPrimary);
            limpiar.Height = 28;
            limpiar.Click += (_, _) =>
            {
                _fechaHistorial.Checked = false;
                CargarHistorial();
            };
            filtroPanel.Controls.Add(limpiar);

            parent.Controls.Add(_gridHistorial);
            parent.Controls.Add(filtroPanel);
            parent.Controls.Add(subtitle);
        }

        public void PrepararProductoParaVenta(Producto producto)
        {
            _productoPendiente = producto;
            _numCantidad.Value = 1;
            _numCantidad.Maximum = Math.Max(1, producto.Stock);
            _lblProductoPendiente.Text = $"Confirmar: {producto.Nombre}  |  Stock disponible: {producto.Stock}";
            _confirmacionPanel.Visible = true;
        }

        public void CalcularIva()
        {
            _aplicaIva = !_aplicaIva;
            foreach (var linea in _carrito)
            {
                linea.AplicaIva = _aplicaIva;
                ActualizarIva(linea);
            }

            _btnIva.Text = _aplicaIva ? "Quitar IVA" : "Agregar IVA (15%)";
            RefrescarCarrito();
        }

        private void ConfirmarProductoPendiente()
        {
            if (_productoPendiente is not Producto productoSeleccionado)
            {
                return;
            }

            int cantidad = (int)_numCantidad.Value;

            // Validación temprana en UI (el trigger igual valida en la BD por si acaso)
            int yaEnCarrito = _carrito.Where(l => l.ProductoId == productoSeleccionado.Id).Sum(l => l.Cantidad);
            if (yaEnCarrito + cantidad > productoSeleccionado.Stock)
            {
                MessageBox.Show(
                    $"Solo hay {productoSeleccionado.Stock} unidades de \"{productoSeleccionado.Nombre}\" en stock.",
                    "Stock insuficiente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var existente = _carrito.FirstOrDefault(l => l.ProductoId == productoSeleccionado.Id);
            if (existente != null)
            {
                existente.Cantidad += cantidad;
                ActualizarIva(existente);
            }
            else
            {
                _carrito.Add(new VentaLinea
                {
                    ProductoId = productoSeleccionado.Id,
                    ProductoNombre = productoSeleccionado.Nombre,
                    PrecioUnitario = productoSeleccionado.Precio,
                    CostoUnitario = productoSeleccionado.Costo,
                    Cantidad = cantidad,
                    AplicaIva = _aplicaIva,
                });
                ActualizarIva(_carrito[^1]);
            }

            RefrescarCarrito();
            _productoPendiente = null;
            _confirmacionPanel.Visible = false;
        }

        private void QuitarDelCarrito()
        {
            if (_gridCarrito.CurrentRow?.DataBoundItem is VentaLinea linea)
            {
                _carrito.Remove(linea);
                RefrescarCarrito();
            }
        }

        private void RefrescarCarrito()
        {
            _gridCarrito.DataSource = null;
            _gridCarrito.DataSource = _carrito.ToList();
            decimal subtotal = _carrito.Sum(l => l.Subtotal);
            decimal iva = _carrito.Sum(l => l.ValorIva);
            decimal total = _carrito.Sum(l => l.Total);
            _lblSubtotal.Text = "Subtotal: " + subtotal.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            _lblIva.Text = $"IVA ({(_aplicaIva ? 15 : 0)}%): " + iva.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            _lblTotal.Text = "Total: " + total.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        }

        private void RegistrarVenta()
        {
            if (_carrito.Count == 0)
            {
                MessageBox.Show("Agrega al menos un producto al carrito.", "Carrito vacío", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int facturaId = _ventaRepo.RegistrarVenta(_carrito, UsuarioActualId);

                MessageBox.Show($"Venta registrada correctamente.\nFactura #{facturaId}", "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _carrito.Clear();
                _aplicaIva = false;
                _btnIva.Text = "Agregar IVA (15%)";
                RefrescarCarrito();
                VentaRegistrada?.Invoke(this, EventArgs.Empty);
            }
            catch (StockInsuficienteException ex)
            {
                MessageBox.Show(
                    $"No se pudo registrar la venta: {ex.Message}\n\n" +
                    "No se descontó stock de ningún producto (se revirtió todo el carrito).",
                    "Stock insuficiente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo registrar la venta.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ActualizarIva(VentaLinea linea)
        {
            linea.ValorIva = linea.AplicaIva
                ? Math.Round(linea.Subtotal * PorcentajeIva, 2, MidpointRounding.AwayFromZero)
                : 0m;
        }

        public void CargarHistorial()
        {
        }

        private void GridHistorial_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _gridHistorial.Columns[e.ColumnIndex].Name != "VerFactura") return;
            if (_gridHistorial.Rows[e.RowIndex].DataBoundItem is not VentaLinea venta) return;

            using var factura = new FacturaVentaForm(venta);
            factura.ShowDialog(FindForm());
        }
    }
}
