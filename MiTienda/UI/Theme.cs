namespace MiTienda.UI
{
    /// <summary>
    /// Paleta de colores centralizada para mantener consistencia visual
    /// en toda la aplicación (estilo dashboard oscuro, como el mockup).
    /// </summary>
    public static class Theme
    {
        // Fondos
        public static readonly Color BackgroundDark = ColorTranslator.FromHtml("#0B1220");
        public static readonly Color SidebarBackground = ColorTranslator.FromHtml("#0F1729");
        public static readonly Color PanelBackground = ColorTranslator.FromHtml("#151F32");
        public static readonly Color CardBackground = ColorTranslator.FromHtml("#111A2B");
        public static readonly Color TopBarBackground = ColorTranslator.FromHtml("#0B1220");
        public static readonly Color InputBackground = ColorTranslator.FromHtml("#1B2740");
        public static readonly Color BorderColor = ColorTranslator.FromHtml("#243149");

        // Texto
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#94A3B8");
        public static readonly Color TextMuted = ColorTranslator.FromHtml("#64748B");

        // Acentos
        public static readonly Color AccentBlue = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color AccentBlueHover = ColorTranslator.FromHtml("#3B82F6");
        public static readonly Color AccentGreen = ColorTranslator.FromHtml("#16A34A");
        public static readonly Color AccentGreenHover = ColorTranslator.FromHtml("#22C55E");
        public static readonly Color AccentPurple = ColorTranslator.FromHtml("#7C3AED");
        public static readonly Color AccentOrange = ColorTranslator.FromHtml("#D97706");
        public static readonly Color AccentRed = ColorTranslator.FromHtml("#DC2626");

        // Estados
        public static readonly Color StatusOk = ColorTranslator.FromHtml("#16A34A");
        public static readonly Color StatusWarning = ColorTranslator.FromHtml("#D97706");
        public static readonly Color StatusError = ColorTranslator.FromHtml("#DC2626");

        // Fuentes
        public static readonly Font FontRegular = new("Segoe UI", 9.5f, FontStyle.Regular);
        public static readonly Font FontSemibold = new("Segoe UI Semibold", 9.5f, FontStyle.Regular);
        public static readonly Font FontTitle = new("Segoe UI Semibold", 12f, FontStyle.Regular);
        public static readonly Font FontHeading = new("Segoe UI Semibold", 15f, FontStyle.Regular);
        public static readonly Font FontCardValue = new("Segoe UI Semibold", 18f, FontStyle.Regular);
        public static readonly Font FontSmall = new("Segoe UI", 8.5f, FontStyle.Regular);
    }
}
