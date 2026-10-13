using MiTienda.Data;
using MiTienda.Forms;
using MiTienda.Models;
using MiTienda.UI;

namespace MiTienda.Controls
{
    public class ProductosControl : UserControl
    {
        private readonly ProductoRepository _productoRepo = new();
        private readonly CategoriaRepository _categoriaRepo = new();
        private readonly bool _modoGestion;

        private DataGridView _grid = null!;
        private TextBox _txtCodigo = null!;
        private TextBox _txtAplicacion = null!;
        private ComboBox _cmbTipoRepuesto = null!;
        private ComboBox _cmbSubtipoBanda = null!;
        private Label _lblSubtipo = null!;
        private ComboBox _cmbMarca = null!;
        private Panel _flujoPanel = null!;
        private Panel _tipoPanel = null!;
        private Panel _subtipoPanel = null!;
        private Panel _filtrosDetalle = null!;
        private Label _rutaLabel = null!;
        private Button _btnRegresar = null!;
        private List<Categoria> _categorias = new();

        public event EventHandler? DatosModificados;
        public event EventHandler<Producto>? ProductoParaVenta;
        public event EventHandler? GestionSolicitada;

        public ProductosControl(bool modoGestion = false)
        {
            _modoGestion = modoGestion;
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundDark;
            BuildLayout();
            CargarCategorias();
        }

        private void BuildLayout()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Theme.BackgroundDark };

            var titleLabel = new Label
            {
                Text = _modoGestion ? "📦  Gestión de productos" : "📦  Productos",
                Font = Theme.FontHeading,
                ForeColor = Theme.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 12),
            };

            var btnAgregar = StyleHelpers.CreateButton("Gestionar Inventario", Theme.AccentBlue);
            btnAgregar.Click += (_, _) =>
            {
                if (_modoGestion) AgregarProducto();
                else GestionSolicitada?.Invoke(this, EventArgs.Empty);
            };

            header.Resize += (_, _) => btnAgregar.Location = new Point(header.Width - btnAgregar.Width, 10);
            btnAgregar.Location = new Point(header.Width - btnAgregar.Width, 10);
            btnAgregar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            header.Controls.Add(titleLabel);
            header.Controls.Add(btnAgregar);

            _flujoPanel = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Theme.BackgroundDark };
            _rutaLabel = new Label
            {
                Text = string.Empty,
                ForeColor = Theme.TextSecondary,
                Font = Theme.FontSemibold,
                AutoSize = true,
                Location = new Point(0, 0),
                Visible = false,
            };
            _btnRegresar = StyleHelpers.CreateButton("← Regresar", Theme.InputBackground, Theme.TextPrimary);
            _btnRegresar.Location = new Point(610, 0);
            _btnRegresar.Visible = false;
            _btnRegresar.Click += (_, _) => RegresarPaso();

            _tipoPanel = new Panel { Location = new Point(0, 20), Size = new Size(760, 52), BackColor = Theme.BackgroundDark };
            var tipoLabel = StyleHelpers.CreateFieldLabel("¿Qué repuesto buscas?");
            tipoLabel.Location = new Point(0, 4);
            _cmbTipoRepuesto = StyleHelpers.CreateComboBox();
            _cmbTipoRepuesto.Width = 190;
            _cmbTipoRepuesto.Location = new Point(2, 23);
            var btnContinuar = StyleHelpers.CreateButton("Continuar", Theme.AccentBlue);
            btnContinuar.Location = new Point(204, 19);
            btnContinuar.Click += (_, _) => SeleccionarTipo();
            _tipoPanel.Controls.Add(tipoLabel);
            _tipoPanel.Controls.Add(_cmbTipoRepuesto);
            _tipoPanel.Controls.Add(btnContinuar);

            _subtipoPanel = new Panel { Location = new Point(0, 20), Size = new Size(760, 52), BackColor = Theme.BackgroundDark, Visible = false };
            _lblSubtipo = StyleHelpers.CreateFieldLabel("Tipo de banda *");
            _lblSubtipo.Location = new Point(0, 4);
            _cmbSubtipoBanda = StyleHelpers.CreateComboBox();
            _cmbSubtipoBanda.Width = 190;
            _cmbSubtipoBanda.Location = new Point(0, 23);
            _cmbSubtipoBanda.DataSource = new[] { "Canales", "Dentadas", "Lisas", "Distribución" };
            var btnContinuarBanda = StyleHelpers.CreateButton("Continuar", Theme.AccentBlue);
            btnContinuarBanda.Location = new Point(204, 19);
            btnContinuarBanda.Click += (_, _) => AbrirFiltros();
            _subtipoPanel.Controls.AddRange(new Control[] { _lblSubtipo, _cmbSubtipoBanda, btnContinuarBanda });

            _filtrosDetalle = new Panel { Location = new Point(0, 20), Size = new Size(760, 56), BackColor = Theme.BackgroundDark, Visible = false };
            var filtroLabel = StyleHelpers.CreateFieldLabel("Ingresa uno o varios datos; la lista se actualiza al escribir");
            filtroLabel.Location = new Point(0, 2);
            _txtCodigo = CrearFiltro("Código del repuesto", 0, 210);
            _cmbMarca = StyleHelpers.CreateComboBox();
            _cmbMarca.Width = 170;
            _cmbMarca.Location = new Point(222, 23);
            _cmbMarca.SelectedIndexChanged += (_, _) => CargarProductos();
            _txtAplicacion = CrearFiltro("Aplicación / vehículo", 404, 185);
            _filtrosDetalle.Controls.Add(filtroLabel);
            _filtrosDetalle.Controls.AddRange(new Control[] { _txtCodigo, _cmbMarca, _txtAplicacion });
            _flujoPanel.Controls.AddRange(new Control[] { _rutaLabel, _btnRegresar, _tipoPanel, _subtipoPanel, _filtrosDetalle });

            _grid = new DataGridView { Dock = DockStyle.Fill };
            StyleHelpers.ApplyDarkGrid(_grid);
            ConfigurarColumnas();

            Panel? accionesPanel = null;
            if (_modoGestion)
            {
                accionesPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Theme.BackgroundDark };
                var btnActualizarStock = StyleHelpers.CreateButton("Actualizar stock del producto seleccionado", Theme.AccentGreen);
                btnActualizarStock.Location = new Point(0, 8);
                btnActualizarStock.Click += (_, _) => ActualizarStock();
                var btnEditar = StyleHelpers.CreateButton("Editar producto", Theme.AccentBlue);
                btnEditar.Location = new Point(290, 8);
                btnEditar.Click += (_, _) => EditarSeleccionado();
                var btnEliminar = StyleHelpers.CreateButton("Eliminar producto", Theme.AccentRed);
                btnEliminar.Location = new Point(460, 8);
                btnEliminar.Click += (_, _) => EliminarSeleccionado();
                accionesPanel.Controls.AddRange(new Control[] { btnActualizarStock, btnEditar, btnEliminar });
            }

            Controls.Add(_grid);
            if (accionesPanel != null) Controls.Add(accionesPanel);
            Controls.Add(_flujoPanel);
            Controls.Add(header);

            _grid.CellContentClick += Grid_CellContentClick;
            if (_modoGestion) _grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is Producto producto)
                    AbrirFormulario(producto);
            };
        }

        private TextBox CrearFiltro(string placeholder, int left, int width)
        {
            var filtro = StyleHelpers.CreateTextBox();
            filtro.Width = width;
            filtro.Location = new Point(left, 23);
            filtro.PlaceholderText = placeholder;
            filtro.TextChanged += (_, _) => CargarProductos();
            return filtro;
        }

        private void ConfigurarColumnas()
        {
            _grid.AutoGenerateColumns = false;
            _grid.Columns.Clear();
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Nombre del producto", DataPropertyName = "Nombre", FillWeight = 190 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ubicacion", HeaderText = "Ubicación en percha", DataPropertyName = "UbicacionPercha", FillWeight = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Costo", HeaderText = "Costo", DataPropertyName = "Costo", FillWeight = 75, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio de venta", DataPropertyName = "Precio", FillWeight = 85, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stock", HeaderText = "Stock", DataPropertyName = "Stock", FillWeight = 60 });
            if (!_modoGestion)
                _grid.Columns.Add(new DataGridViewButtonColumn { Name = "AgregarVenta", HeaderText = "Venta", Text = "Agregar a venta", UseColumnTextForButtonValue = true, FillWeight = 105, FlatStyle = FlatStyle.Flat });
        }

        private void CargarCategorias()
        {
            try
            {
                _categorias = _categoriaRepo.ObtenerTodas();
                _cmbTipoRepuesto.DataSource = new List<string> { "Bandas", "Orings", "Retenedores", "Rulimanes" };
                _cmbTipoRepuesto.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar las categorías.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SeleccionarTipo()
        {
            if (_cmbTipoRepuesto.SelectedItem == null)
            {
                MessageBox.Show("Selecciona primero el tipo de repuesto.", "Falta un dato", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string tipoSeleccionado = _cmbTipoRepuesto.SelectedItem?.ToString() ?? string.Empty;
            bool requiereSubtipo = tipoSeleccionado is "Bandas" or "Orings";
            bool esBanda = tipoSeleccionado == "Bandas";
            _tipoPanel.Visible = false;
            _subtipoPanel.Visible = requiereSubtipo;
            _filtrosDetalle.Visible = !requiereSubtipo;
            if (tipoSeleccionado == "Orings")
            {
                _lblSubtipo.Text = "Tipo de oring *";
                _cmbSubtipoBanda.DataSource = new[] { "Viton", "Nitrilo" };
            }
            else
            {
                _lblSubtipo.Text = "Tipo de banda *";
                _cmbSubtipoBanda.DataSource = new[] { "Canales", "Dentadas", "Lisas", "Distribución" };
            }
            MostrarRuta(_cmbTipoRepuesto.SelectedItem + " >");
            if (!requiereSubtipo) AbrirFiltros();
        }

        private void AbrirFiltros()
        {
            if (_cmbTipoRepuesto.SelectedItem?.ToString() is "Bandas" or "Orings" && _cmbSubtipoBanda.SelectedItem == null)
            {
                MessageBox.Show("Selecciona el tipo de banda.", "Falta un dato", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _tipoPanel.Visible = false;
            _subtipoPanel.Visible = false;
            _filtrosDetalle.Visible = true;
            string tipo = _cmbTipoRepuesto.SelectedItem?.ToString() ?? string.Empty;
            string ruta = tipo is "Bandas" or "Orings"
                ? $"{tipo} > {_cmbSubtipoBanda.SelectedItem} >"
                : $"{tipo} >";
            MostrarRuta(ruta);
            CargarMarcas();
            CargarProductos();
        }

        private void MostrarRuta(string ruta)
        {
            _rutaLabel.Text = ruta;
            _rutaLabel.Visible = true;
            _btnRegresar.Visible = true;
        }

        private void RegresarPaso()
        {
            string? tipo = _cmbTipoRepuesto.SelectedItem?.ToString();
            _grid.DataSource = null;
            _filtrosDetalle.Visible = false;
            if (_subtipoPanel.Visible)
            {
                _subtipoPanel.Visible = false;
                _tipoPanel.Visible = true;
                _rutaLabel.Visible = false;
                _btnRegresar.Visible = false;
                return;
            }
            if (tipo is "Bandas" or "Orings")
            {
                _subtipoPanel.Visible = true;
                MostrarRuta("Bandas >");
                return;
            }

            _tipoPanel.Visible = true;
            _subtipoPanel.Visible = false;
            _rutaLabel.Visible = false;
            _btnRegresar.Visible = false;
        }

        private void CargarMarcas()
        {
            string tipo = _cmbTipoRepuesto.SelectedItem?.ToString() ?? string.Empty;
            var marcas = _productoRepo.ObtenerTodos(filtroCategoriaId: _categorias.FirstOrDefault(c => c.Nombre == tipo)?.Id)
                .Select(p => p.Marca)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Cast<string>()
                .Concat(tipo == "Bandas" ? new[] { "BANDO", "BETTERLINE", "DONGIL", "GATES" } : Array.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(m => m)
                .ToList();
            marcas.Insert(0, "Todas las marcas");
            _cmbMarca.DataSource = marcas;
            string? subtipo = _cmbSubtipoBanda.SelectedItem?.ToString();
            bool esDistribucion = tipo == "Bandas" && subtipo == "Distribución";
            _cmbMarca.Visible = tipo == "Rulimanes" || (tipo == "Bandas" && !esDistribucion);
            _txtAplicacion.Visible = esDistribucion || (tipo == "Bandas" && subtipo == "Canales");
            _txtAplicacion.PlaceholderText = esDistribucion
                ? "Aplicación / descripción"
                : "Aplicación / vehículo";
            ConfigurarColumnaDetalle(tipo, subtipo);
        }

        public void CargarProductos()
        {
            try
            {
                if (!_filtrosDetalle.Visible || _cmbTipoRepuesto.SelectedItem is not string tipo) return;
                int? categoriaId = _categorias.FirstOrDefault(c => string.Equals(c.Nombre, tipo, StringComparison.OrdinalIgnoreCase))?.Id;
                string? marca = _cmbMarca.Visible && _cmbMarca.SelectedItem?.ToString() != "Todas las marcas"
                    ? _cmbMarca.SelectedItem?.ToString() : null;
                string? subtipo = tipo is "Bandas" or "Orings" ? _cmbSubtipoBanda.SelectedItem?.ToString() : null;
                var productos = _productoRepo.ObtenerTodos(filtroCategoriaId: categoriaId,
                    filtroCodigo: _txtCodigo.Text, filtroMarca: marca, filtroTipoBanda: tipo == "Bandas" ? subtipo : null,
                    filtroTipoOring: tipo == "Orings" ? subtipo : null,
                    filtroAplicacion: tipo == "Bandas" && subtipo == "Canales" ? _txtAplicacion.Text : null,
                    filtroDescripcion: tipo == "Bandas" && subtipo == "Distribución" ? _txtAplicacion.Text : null);
                _grid.DataSource = productos;

                // Resaltar únicamente los productos agotados.
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.DataBoundItem is not Producto p) continue;
                    if (p.Stock <= 0)
                        row.DefaultCellStyle.ForeColor = Theme.StatusError;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los productos.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RestablecerFiltros()
        {
            _txtCodigo.Clear();
            _txtAplicacion.Clear();
            _cmbTipoRepuesto.SelectedIndex = -1;
            _cmbSubtipoBanda.SelectedIndex = 0;
            _tipoPanel.Visible = true;
            _subtipoPanel.Visible = false;
            _filtrosDetalle.Visible = false;
            _rutaLabel.Visible = false;
            _btnRegresar.Visible = false;
            _grid.DataSource = null;
        }

        private void ConfigurarColumnaDetalle(string tipo, string? subtipo)
        {
            var columna = _grid.Columns["Ubicacion"];
            bool mostrarAplicacion = tipo == "Bandas" && (subtipo == "Canales" || subtipo == "Distribución");
            columna.HeaderText = mostrarAplicacion ? "Aplicación / descripción" : "Ubicación en percha";
            columna.DataPropertyName = subtipo == "Distribución"
                ? nameof(Producto.Descripcion)
                : mostrarAplicacion
                    ? nameof(Producto.AplicacionVehiculo)
                    : nameof(Producto.UbicacionPercha);
        }

        private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _modoGestion || _grid.Columns[e.ColumnIndex].Name != "AgregarVenta") return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is not Producto producto) return;
            if (producto.Stock <= 0)
            {
                MessageBox.Show("Este producto no tiene stock disponible.", "Sin stock", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ProductoParaVenta?.Invoke(this, producto);
        }

        private void AgregarProducto()
        {
            AbrirFormulario(null);
        }

        private void ActualizarStock()
        {
            var producto = ObtenerSeleccionado();
            if (producto == null) return;

            using var form = new ReposicionStockForm(producto);
            if (form.ShowDialog(FindForm()) != DialogResult.OK) return;

            try
            {
                producto.Stock += form.CantidadRecibida;
                _productoRepo.Actualizar(producto);
                CargarProductos();
                DatosModificados?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo actualizar el stock.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool SolicitarClave()
        {
            using var dialogo = new Form
            {
                Text = "Acceso de administrador",
                Size = new Size(360, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Theme.BackgroundDark,
            };
            var etiqueta = StyleHelpers.CreateFieldLabel("Contraseña de un administrador");
            etiqueta.Location = new Point(22, 18);
            var clave = StyleHelpers.CreateTextBox();
            clave.Location = new Point(22, 42);
            clave.Width = 300;
            clave.PasswordChar = '●';
            var aceptar = StyleHelpers.CreateButton("Ingresar", Theme.AccentGreen);
            aceptar.Location = new Point(178, 82);
            aceptar.DialogResult = DialogResult.OK;
            var cancelar = StyleHelpers.CreateButton("Cancelar", Theme.InputBackground, Theme.TextPrimary);
            cancelar.Location = new Point(70, 82);
            cancelar.DialogResult = DialogResult.Cancel;
            dialogo.Controls.AddRange(new Control[] { etiqueta, clave, aceptar, cancelar });
            dialogo.AcceptButton = aceptar;
            dialogo.CancelButton = cancelar;

            if (dialogo.ShowDialog() != DialogResult.OK) return false;
            try
            {
                if (new UsuarioRepository().ValidarClaveAdministrador(clave.Text)) return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo validar la cuenta de administrador.\n\n{ex.Message}", "Error de acceso", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            MessageBox.Show("La contraseña no es correcta.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void EditarSeleccionado()
        {
            var producto = ObtenerSeleccionado();
            if (producto != null) AbrirFormulario(producto);
        }

        private void EliminarSeleccionado()
        {
            var producto = ObtenerSeleccionado();
            if (producto == null) return;
            if (MessageBox.Show($"¿Eliminar el producto «{producto.Nombre}»?", "Confirmar eliminación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                if (!_productoRepo.Eliminar(producto.Id))
                {
                    MessageBox.Show("Este producto tiene ventas o movimientos asociados y no se puede eliminar sin afectar el historial.",
                        "Producto con historial", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                CargarProductos();
                DatosModificados?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo eliminar el producto.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormulario(Producto? productoExistente)
        {
            using var form = new ProductoForm(productoExistente);
            if (form.ShowDialog(FindForm()) != DialogResult.OK) return;

            try
            {
                if (productoExistente == null)
                    _productoRepo.Insertar(form.Resultado);
                else
                    _productoRepo.Actualizar(form.Resultado);

                CargarProductos();
                DatosModificados?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar el producto.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Producto? ObtenerSeleccionado()
        {
            if (_grid.CurrentRow?.DataBoundItem is Producto p) return p;
            MessageBox.Show("Selecciona un producto de la lista primero.", "Ningún producto seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

    }
}
