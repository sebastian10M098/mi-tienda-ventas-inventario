namespace MiTienda.UI
{
    public static class StyleHelpers
    {
        public static void ApplyDarkGrid(DataGridView grid)
        {
            grid.BackgroundColor = Theme.PanelBackground;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Theme.BorderColor;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 38;
            grid.RowTemplate.Height = 36;
            grid.EnableHeadersVisualStyles = false;
            grid.Font = Theme.FontRegular;

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Theme.SidebarBackground,
                ForeColor = Theme.TextSecondary,
                Font = Theme.FontSemibold,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = Theme.SidebarBackground,
                SelectionForeColor = Theme.TextSecondary,
                Padding = new Padding(8, 0, 0, 0),
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Theme.PanelBackground,
                ForeColor = Theme.TextPrimary,
                SelectionBackColor = Theme.AccentBlue,
                SelectionForeColor = Color.White,
                Padding = new Padding(8, 0, 0, 0),
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Theme.CardBackground,
            };
        }

        public static Button CreateButton(string text, Color backColor, Color? foreColor = null)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor ?? Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.FontSemibold,
                Cursor = Cursors.Hand,
                Height = 34,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(14, 4, 14, 4),
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backColor, 0.15f);
            return btn;
        }

        public static TextBox CreateTextBox()
        {
            return new TextBox
            {
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = Theme.FontRegular,
            };
        }

        public static ComboBox CreateComboBox()
        {
            return new ComboBox
            {
                BackColor = Theme.InputBackground,
                ForeColor = Theme.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Theme.FontRegular,
            };
        }

        public static Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = Theme.TextSecondary,
                Font = Theme.FontSmall,
                AutoSize = true,
            };
        }
    }
}
