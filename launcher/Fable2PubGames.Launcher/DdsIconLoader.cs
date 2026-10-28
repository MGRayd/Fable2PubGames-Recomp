using System.Runtime.InteropServices;

namespace Fable2PubGames.Launcher;

internal static class DdsIconLoader
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr handle);

    public static Icon? TryLoadIcon(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            byte[] data = File.ReadAllBytes(path);
            if (data.Length < 144 || data[0] != 'D' || data[1] != 'D' || data[2] != 'S' || data[3] != ' ') return null;
            int height = BitConverter.ToInt32(data, 12);
            int width = BitConverter.ToInt32(data, 16);
            if (width <= 0 || height <= 0 || width > 2048 || height > 2048 || System.Text.Encoding.ASCII.GetString(data, 84, 4) != "DXT5") return null;

            using var decoded = DecodeDxt5(data, width, height);
            using var iconBitmap = new Bitmap(decoded, new Size(32, 32));
            IntPtr handle = iconBitmap.GetHicon();
            try
            {
                using Icon temporary = Icon.FromHandle(handle);
                return (Icon)temporary.Clone();
            }
            finally { DestroyIcon(handle); }
        }
        catch { return null; }
    }

    private static Bitmap DecodeDxt5(byte[] data, int width, int height)
    {
        var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        int offset = 128;
        int blocksWide = (width + 3) / 4;
        int blocksHigh = (height + 3) / 4;
        for (int blockY = 0; blockY < blocksHigh; blockY++)
        {
            for (int blockX = 0; blockX < blocksWide; blockX++)
            {
                if (offset + 16 > data.Length) return bitmap;
                byte alpha0 = data[offset];
                byte alpha1 = data[offset + 1];
                byte[] alphas = new byte[8];
                alphas[0] = alpha0;
                alphas[1] = alpha1;
                if (alpha0 > alpha1)
                {
                    for (int i = 1; i <= 6; i++) alphas[i + 1] = (byte)(((7 - i) * alpha0 + i * alpha1) / 7);
                }
                else
                {
                    for (int i = 1; i <= 4; i++) alphas[i + 1] = (byte)(((5 - i) * alpha0 + i * alpha1) / 5);
                    alphas[6] = 0;
                    alphas[7] = 255;
                }

                ulong alphaBits = 0;
                for (int i = 0; i < 6; i++) alphaBits |= (ulong)data[offset + 2 + i] << (8 * i);
                ushort colour0 = BitConverter.ToUInt16(data, offset + 8);
                ushort colour1 = BitConverter.ToUInt16(data, offset + 10);
                Color c0 = FromRgb565(colour0);
                Color c1 = FromRgb565(colour1);
                Color[] colours = new Color[4];
                colours[0] = c0;
                colours[1] = c1;
                colours[2] = Blend(c0, c1, 2, 1);
                colours[3] = Blend(c0, c1, 1, 2);
                uint colourBits = BitConverter.ToUInt32(data, offset + 12);

                for (int pixel = 0; pixel < 16; pixel++)
                {
                    int x = blockX * 4 + pixel % 4;
                    int y = blockY * 4 + pixel / 4;
                    if (x >= width || y >= height) continue;
                    Color rgb = colours[(int)((colourBits >> (pixel * 2)) & 3)];
                    byte alpha = alphas[(int)((alphaBits >> (pixel * 3)) & 7)];
                    bitmap.SetPixel(x, y, Color.FromArgb(alpha, rgb.R, rgb.G, rgb.B));
                }
                offset += 16;
            }
        }
        return bitmap;
    }

    private static Color FromRgb565(ushort value)
    {
        int r = (value >> 11) & 31;
        int g = (value >> 5) & 63;
        int b = value & 31;
        return Color.FromArgb((r * 255 + 15) / 31, (g * 255 + 31) / 63, (b * 255 + 15) / 31);
    }

    private static Color Blend(Color first, Color second, int firstWeight, int secondWeight)
    {
        int total = firstWeight + secondWeight;
        return Color.FromArgb(
            (first.R * firstWeight + second.R * secondWeight) / total,
            (first.G * firstWeight + second.G * secondWeight) / total,
            (first.B * firstWeight + second.B * secondWeight) / total);
    }
}
