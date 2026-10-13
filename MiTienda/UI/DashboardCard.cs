namespace MiTienda.UI
{
    /// <summary>
    /// Tarjeta de resumen tipo "Productos registrados", "Ventas del día", etc.
    /// Incluye un icono con fondo de color, un título pequeño y un valor grande.
    /// </summary>
    public class DashboardCard : Panel
    {
        private readonly Panel _iconBadge;
        private readonly Label _iconLabel;
        private readonly Label _titleLabel;
        private readonly Label _valueLabel;

        public string Value
        {
            get => _valueLabel.Text;
            set => _valueLabel.Text = value;
        }

        public DashboardCard(string icon, string title, string value, Color accentColor)
        {
            Size = new Size(240, 105);
            BackColor = Theme.CardBackground;
            Padding = new Padding(16);
            Margin = new Padding(0, 0, 16, 0);

            _iconBadge = new Panel
            {
                Size = new Size(38, 38),
                Location = new Point(14, 15),
                BackColor = accentColor
            };
            _iconBadge.Paint += (_, e) => DrawRoundedBackground(e.Graphics, _iconBadge, accentColor, 10);

            _iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.White,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _iconBadge.Controls.Add(_iconLabel);

            _titleLabel = new Label
            {
                Text = title,
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                AutoSize = false,
                Location = new Point(64, 15),
                Size = new Size(Width - 76, 20),
                BackColor = Color.Transparent
            };

            _valueLabel = new Label
            {
                Text = value,
                Font = Theme.FontCardValue,
                ForeColor = Theme.TextPrimary,
                AutoSize = false,
                Location = new Point(64, 37),
                Size = new Size(Width - 76, 42),
                BackColor = Color.Transparent
            };

            Controls.Add(_iconBadge);
            Controls.Add(_titleLabel);
            Controls.Add(_valueLabel);

            SizeChanged += (_, _) =>
            {
                int textWidth = Math.Max(90, Width - 76);
                _titleLabel.Size = new Size(textWidth, 20);
                _valueLabel.Size = new Size(textWidth, Math.Max(30, Height - 52));
            };

            Paint += (_, e) => DrawRoundedBorder(e.Graphics);
        }

        private static void DrawRoundedBackground(Graphics g, Control control, Color color, int radius)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = RoundedRect(new Rectangle(0, 0, control.Width, control.Height), radius);
            using var brush = new SolidBrush(color);
            g.FillPath(brush, path);
        }

        private void DrawRoundedBorder(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 10);
            using var pen = new Pen(Theme.BorderColor, 1);
            g.DrawPath(pen, path);
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
