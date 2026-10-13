using MiTienda.Data;
using MiTienda.Models;
using MiTienda.UI;

namespace MiTienda.Forms
{
    public class ProductoForm : Form
    {
        private readonly CategoriaRepository _categoriaRepo = new();
        private readonly ProveedorRepository _proveedorRepo = new();
        private readonly Producto? _productoExistente;

        private TextBox _txtNombre = null!;
        private TextBox _txtCodigo = null!;
        private TextBox _txtDescripcion = null!;
        private TextBox _txtUbicacion = null!;
        private TextBox _txtAplicacion = null!;
        private TextBox _txtMarca = null!;
        private ComboBox _cmbTipoBanda = null!;
        private TextBox _txtPrecio = null!;
        private TextBox _txtCosto = null!;
        private TextBox _txtStock = null!;
        private ComboBox _cmbCategoria = null!;
        private ComboBox _cmbProveedor = null!;

        public Producto Resultado { get; private set; } = new();

        public ProductoForm(Producto? productoExistente = null)
        {
            _productoExistente = productoExistente;

            Text = productoExistente == null ? "Agregar producto" : "Editar producto";
            Size = new Size(420, 730);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.BackgroundDark;
            Font = Theme.FontRegular;

            BuildForm();
            CargarCombos();

            if (_productoExistente != null)
                CargarValores(_productoExistente);
        }

        private void BuildForm()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                Padding = new Padding(24),
                AutoScroll = true,
                BackColor = Theme.BackgroundDark,
            };
            panel.RowStyles.Clear();

            _txtNombre = StyleHelpers.CreateTextBox();
            _txtCodigo = StyleHelpers.CreateTextBox();
            _txtDescripcion = StyleHelpers.CreateTextBox();
            _txtUbicacion = StyleHelpers.CreateTextBox();
            _txtAplicacion = StyleHelpers.CreateTextBox();
            _txtMarca = StyleHelpers.CreateTextBox();
            _cmbTipoBanda = StyleHelpers.CreateComboBox();
            _cmbTipoBanda.DataSource = new[] { "", "Canales", "Dentadas", "Lisas", "Distribución" };
            _txtPrecio = StyleHelpers.CreateTextBox();
            _txtCosto = StyleHelpers.CreateTextBox();
            _txtStock = StyleHelpers.CreateTextBox();
            _cmbCategoria = StyleHelpers.CreateComboBox();
            _cmbProveedor = StyleHelpers.CreateComboBox();

            AddField(panel, "Nombre *", _txtNombre);
            AddField(panel, "Código del repuesto *", _txtCodigo);
            AddField(panel, "Descripción", _txtDescripcion);
            AddField(panel, "Tipo de banda (solo Bandas)", _cmbTipoBanda);
            AddField(panel, "Aplicación / vehículo", _txtAplicacion);
            AddField(panel, "Marca", _txtMarca);
            AddField(panel, "Ubicación en percha", _txtUbicacion);
            AddField(panel, "Costo *", _txtCosto);
            AddField(panel, "Precio de venta *", _txtPrecio);
            AddField(panel, "Stock *", _txtStock);
            AddField(panel, "Categoría *", _cmbCategoria);
            AddField(panel, "Proveedor *", _cmbProveedor);


            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(24, 12, 24, 12),
                BackColor = Theme.BackgroundDark,
            };

            var btnGuardar = StyleHelpers.CreateButton("Guardar", Theme.AccentGreen);
            btnGuardar.Click += BtnGuardar_Click;

            var btnCancelar = StyleHelpers.CreateButton("Cancelar", Theme.InputBackground, Theme.TextPrimary);
            btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            buttonsPanel.Controls.Add(btnGuardar);
            buttonsPanel.Controls.Add(btnCancelar);

            Controls.Add(panel);
            Controls.Add(buttonsPanel);
        }

        private void AddField(TableLayoutPanel panel, string labelText, Control input)
        {
            var label = StyleHelpers.CreateFieldLabel(labelText);
            input.Width = 340;
            input.Margin = new Padding(0, 2, 0, 12);
            label.Margin = new Padding(0, 8, 0, 2);

            panel.Controls.Add(label);
            panel.Controls.Add(input);
        }

        private void CargarCombos()
        {
            try
            {
                var tiposRepuesto = new[] { "Bandas", "Orings", "Retenedores", "Rulimanes" };
                var categorias = _categoriaRepo.ObtenerTodas()
                    .Where(c => tiposRepuesto.Contains(c.Nombre, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                _cmbCategoria.DataSource = categorias;
                _cmbCategoria.DisplayMember = nameof(Categoria.Nombre);
                _cmbCategoria.ValueMember = nameof(Categoria.Id);

                var proveedores = _proveedorRepo.ObtenerTodos(soloActivos: true);
                _cmbProveedor.DataSource = proveedores;
                _cmbProveedor.DisplayMember = nameof(Proveedor.Nombre);
                _cmbProveedor.ValueMember = nameof(Proveedor.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar categorías/proveedores.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarValores(Producto p)
        {
            _txtNombre.Text = p.Nombre;
            _txtCodigo.Text = p.Codigo;
            _txtDescripcion.Text = p.Descripcion;
            _txtUbicacion.Text = p.UbicacionPercha;
            _txtAplicacion.Text = p.AplicacionVehiculo;
            _txtMarca.Text = p.Marca;
            _cmbTipoBanda.SelectedItem = p.TipoBanda ?? "";
            _txtPrecio.Text = p.Precio.ToString("0.00");
            _txtCosto.Text = p.Costo.ToString("0.00");
            _txtStock.Text = p.Stock.ToString();
            _cmbCategoria.SelectedValue = p.CategoriaId;
            _cmbProveedor.SelectedValue = p.ProveedorId;
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (!Validar(out string error))
            {
                MessageBox.Show(error, "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Resultado = new Producto
            {
                Id = _productoExistente?.Id ?? 0,
                Nombre = _txtNombre.Text.Trim(),
                Codigo = _txtCodigo.Text.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(_txtDescripcion.Text) ? null : _txtDescripcion.Text.Trim(),
                UbicacionPercha = string.IsNullOrWhiteSpace(_txtUbicacion.Text) ? null : _txtUbicacion.Text.Trim(),
                TipoBanda = string.IsNullOrWhiteSpace(_cmbTipoBanda.Text) ? null : _cmbTipoBanda.Text,
                AplicacionVehiculo = string.IsNullOrWhiteSpace(_txtAplicacion.Text) ? null : _txtAplicacion.Text.Trim(),
                Marca = string.IsNullOrWhiteSpace(_txtMarca.Text) ? null : _txtMarca.Text.Trim(),
                Precio = decimal.Parse(_txtPrecio.Text),
                Costo = decimal.Parse(_txtCosto.Text),
                Stock = int.Parse(_txtStock.Text),
                CategoriaId = (int)_cmbCategoria.SelectedValue!,
                ProveedorId = (int)_cmbProveedor.SelectedValue!,
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool Validar(out string error)
        {
            if (string.IsNullOrWhiteSpace(_txtNombre.Text))
            {
                error = "El nombre es obligatorio.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(_txtCodigo.Text))
            {
                error = "El código del repuesto es obligatorio.";
                return false;
            }
            if (!decimal.TryParse(_txtPrecio.Text, out decimal precio) || precio < 0)
            {
                error = "El precio debe ser un número válido mayor o igual a 0.";
                return false;
            }
            if (!decimal.TryParse(_txtCosto.Text, out decimal costo) || costo < 0)
            {
                error = "El costo debe ser un número válido mayor o igual a 0.";
                return false;
            }
            if (!int.TryParse(_txtStock.Text, out int stock) || stock < 0)
            {
                error = "El stock debe ser un número entero mayor o igual a 0.";
                return false;
            }
            if (_cmbCategoria.SelectedValue == null)
            {
                error = "Selecciona una categoría.";
                return false;
            }
            if (_cmbProveedor.SelectedValue == null)
            {
                error = "Selecciona un proveedor.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
