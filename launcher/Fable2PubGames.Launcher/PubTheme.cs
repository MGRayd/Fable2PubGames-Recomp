using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Fable2PubGames.Launcher;

internal static class PubTheme
{
    private static readonly PrivateFontCollection GameFonts = new();
    private static FontFamily? gameFont;

    public static readonly Color Background = Color.FromArgb(10, 19, 18);
    public static readonly Color Surface = Color.FromArgb(16, 29, 27);
    public static readonly Color SurfaceRaised = Color.FromArgb(20, 36, 33);
    public static readonly Color Border = Color.FromArgb(55, 75, 70);
    public static readonly Color Text = Color.FromArgb(239, 235, 218);
    public static readonly Color Muted = Color.FromArgb(157, 181, 174);
    public static readonly Color Gold = Color.FromArgb(221, 169, 69);
    public static readonly Color Success = Color.FromArgb(117, 166, 105);
    public static readonly Color Ink = Color.FromArgb(36, 24, 15);
    public static readonly Color Parchment = Color.FromArgb(232, 210, 160);

    public static void Initialise(string mediaRoot)
    {
        if (gameFont is not null) return;
        string fontPath = Path.Combine(mediaRoot, "font_en.ttf");
        if (!File.Exists(fontPath)) return;
        try
        {
            GameFonts.AddFontFile(fontPath);
            gameFont = GameFonts.Families.FirstOrDefault();
        }
        catch { gameFont = null; }
    }

    public static Font Heading(float size, FontStyle style = FontStyle.Regular) =>
        new(gameFont ?? FontFamily.GenericSerif, size, style, GraphicsUnit.Point);

    public static void StyleButton(Button button, bool primary = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = primary ? Gold : Border;
        button.BackColor = primary ? Gold : SurfaceRaised;
        button.ForeColor = primary ? Ink : Text;
        button.Font = new Font("Segoe UI Semibold", 9.5F);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
        ApplyRoundedRegion(button);
        button.Resize += (_, _) => ApplyRoundedRegion(button);
    }

    private static void ApplyRoundedRegion(Control control)
    {
        if (control.Width <= 0 || control.Height <= 0) return;
        const int radius = 8;
        using var path = new GraphicsPath();
        int diameter = radius * 2;
        var arc = new Rectangle(0, 0, diameter, diameter);
        path.AddArc(arc, 180, 90);
        arc.X = control.Width - diameter - 1;
        path.AddArc(arc, 270, 90);
        arc.Y = control.Height - diameter - 1;
        path.AddArc(arc, 0, 90);
        arc.X = 0;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        Region? oldRegion = control.Region;
        control.Region = new Region(path);
        oldRegion?.Dispose();
    }
}

internal sealed class LauncherCard : Panel
{
    public LauncherCard()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = PubTheme.Surface;
        Padding = new Padding(22);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var border = new Pen(PubTheme.Border);
        e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
    }
}

internal sealed class GameArtworkPanel : Panel
{
    private Image? artwork;

    public GameArtworkPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = PubTheme.Surface;
    }

    public void LoadFrom(string path)
    {
        artwork?.Dispose();
        artwork = null;
        if (File.Exists(path))
        {
            try
            {
                using var source = Image.FromFile(path);
                artwork = new Bitmap(source);
            }
            catch { }
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        if (artwork is not null)
        {
            float scale = Math.Max((float)Width / artwork.Width, (float)Height / artwork.Height);
            int width = (int)(artwork.Width * scale);
            int height = (int)(artwork.Height * scale);
            e.Graphics.DrawImage(artwork, (Width - width) / 2, (Height - height) / 2, width, height);
            using var shade = new LinearGradientBrush(ClientRectangle, Color.FromArgb(8, 10, 15, 13), Color.FromArgb(210, 7, 13, 12), 90F);
            e.Graphics.FillRectangle(shade, ClientRectangle);
        }
        else
        {
            using var wash = new LinearGradientBrush(ClientRectangle, PubTheme.SurfaceRaised, PubTheme.Background, 45F);
            e.Graphics.FillRectangle(wash, ClientRectangle);
        }

        using var border = new Pen(PubTheme.Border);
        e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        using var title = PubTheme.Heading(23F);
        TextRenderer.DrawText(e.Graphics, "PUB GAMES", title, new Rectangle(24, Height - 104, Width - 48, 42), PubTheme.Gold,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        using var caption = new Font("Segoe UI", 9F);
        TextRenderer.DrawText(e.Graphics, artwork is null ? "Artwork appears after setup" : "Welcome back to Albion",
            caption, new Rectangle(26, Height - 58, Width - 52, 28), PubTheme.Muted,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) artwork?.Dispose();
        base.Dispose(disposing);
    }
}
