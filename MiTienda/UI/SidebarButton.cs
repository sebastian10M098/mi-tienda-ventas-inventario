namespace MiTienda.UI
{
    /// <summary>
    /// Botón estilizado para el menú lateral, con ícono (emoji/símbolo) + texto,
    /// y resaltado cuando está activo (sección seleccionada).
    /// </summary>
    public class SidebarButton : Panel
    {
        private readonly Label _iconLabel;
        private readonly Label _textLabel;
        private bool _isActive;

        public event EventHandler? Activated;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                UpdateVisualState();
            }
        }

        public SidebarButton(string icon, string text)
        {
            Height = 44;
            Dock = DockStyle.Top;
            Cursor = Cursors.Hand;
            Margin = new Padding(0, 2, 0, 2);
            BackColor = Theme.SidebarBackground;

            _iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 11f),
                ForeColor = Theme.TextSecondary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(14, 0),
                Size = new Size(28, Height),
                BackColor = Color.Transparent
            };

            _textLabel = new Label
            {
                Text = text,
                Font = Theme.FontSemibold,
                ForeColor = Theme.TextSecondary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(48, 0),
                Size = new Size(150, Height),
                BackColor = Color.Transparent
            };

            Controls.Add(_iconLabel);
            Controls.Add(_textLabel);

            foreach (Control c in new Control[] { this, _iconLabel, _textLabel })
            {
                c.Click += (_, _) => Activated?.Invoke(this, EventArgs.Empty);
                c.MouseEnter += (_, _) => { if (!_isActive) BackColor = Theme.InputBackground; };
                c.MouseLeave += (_, _) => { if (!_isActive) BackColor = Theme.SidebarBackground; };
            }
        }

        private void UpdateVisualState()
        {
            if (_isActive)
            {
                BackColor = Theme.AccentBlue;
                _iconLabel.ForeColor = Color.White;
                _textLabel.ForeColor = Color.White;
            }
            else
            {
                BackColor = Theme.SidebarBackground;
                _iconLabel.ForeColor = Theme.TextSecondary;
                _textLabel.ForeColor = Theme.TextSecondary;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_isActive)
            {
                // Barrita de acento a la izquierda para reforzar el estado activo
                using var brush = new SolidBrush(Color.White);
                e.Graphics.FillRectangle(brush, 0, 0, 3, Height);
            }
        }
    }
}
