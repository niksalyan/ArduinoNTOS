using NTOSCompiler;
using NTOSCompiler.Compiler;

namespace NTOSEmulator.Libs
{
    internal class ScreenBuffer
    {
        private readonly Bitmap buffer = new Bitmap(480, 320, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        private TextRenderer textRenderer = new TextRenderer();

        public Action OnInvalidate;

        public readonly Dictionary<string, byte> colors = new Dictionary<string, byte>
        {
            ["EMPTY"] = 0x00,
            ["LOW"] = 0x00,
            ["HIGH"] = 0x1,


            ["BLACK"] = 0x00,
            ["WHITE"] = 0xFF,

            ["RED"] = 0xE0,
            ["GREEN"] = 0x1C,
            ["BLUE"] = 0x03,

            ["YELLOW"] = 0xFC,
            ["CYAN"] = 0x1F,
            ["MAGENTA"] = 0xE3,

            ["ORANGE"] = 0xE8,
            ["PURPLE"] = 0x83,
            ["PINK"] = 0xE6,

            ["GRAY"] = 0x92,
            ["DARKGRAY"] = 0x49,
            ["LIGHTGRAY"] = 0xDB,

            ["BROWN"] = 0xA0,
            ["DARKRED"] = 0x80,
            ["DARKGREEN"] = 0x10,
            ["DARKBLUE"] = 0x02,

            ["LIME"] = 0x3C,
            ["NAVY"] = 0x02,
            ["TEAL"] = 0x12,
            ["OLIVE"] = 0xB0
        };


        public ScreenBuffer(VMFunctions vmFunctions)
        {
            SetupFunctions(vmFunctions);
        }

        public Bitmap GetBuffer()
        {
            return buffer;
        }

        private void SetupFunctions(VMFunctions vmFunctions)
        {


            vmFunctions.AddFunction(9, "cls", 0, VariableType.None, async args =>
            {
                textRenderer.SetCursor(0, 0);
                textRenderer.SetTextSize(2);
                using var g = Graphics.FromImage(buffer);
                g.Clear(Color.Black);

                OnInvalidate?.Invoke();
                return null;
            });

            // ============================================================
            // GRAPHICS: 10 - 49
            // ============================================================

            vmFunctions.AddFunction(10, "drawBox", 5, VariableType.None, async args =>
            {
                if (args.Length < 5)
                    return null;

                using var g = Graphics.FromImage(buffer);
                using var pen = new Pen(
                    GetColor332((byte)args[4]));

                g.DrawRectangle(
                    pen,
                    new Rectangle(
                        Convert.ToInt32(args[0]),
                        Convert.ToInt32(args[1]),
                        Convert.ToInt32(args[2]),
                        Convert.ToInt32(args[3])));

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(11, "fillBox", 5, VariableType.None, async args =>
            {
                if (args.Length < 5)
                    return null;

                using var g = Graphics.FromImage(buffer);
                using var brush = new SolidBrush(
                    GetColor332((byte)args[4]));

                g.FillRectangle(
                    brush,
                    new Rectangle(
                        Convert.ToInt32(args[0]),
                        Convert.ToInt32(args[1]),
                        Convert.ToInt32(args[2]),
                        Convert.ToInt32(args[3])));

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(12, "drawRoundBox", 6, VariableType.None, async args =>
            {
                if (args.Length < 5)
                    return null;

                using var g = Graphics.FromImage(buffer);
                using var pen = new Pen(
                    GetColor332((byte)args[5]));

                g.DrawRoundedRectangle(
                    pen,
                    new Rectangle(
                        Convert.ToInt32(args[0]),
                        Convert.ToInt32(args[1]),
                        Convert.ToInt32(args[2]),
                        Convert.ToInt32(args[3])),
                    new Size(Convert.ToInt32(args[4]), Convert.ToInt32(args[4]))
                    );

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(13, "fillRoundBox", 6, VariableType.None, async args =>
            {
                if (args.Length < 5)
                    return null;

                using var g = Graphics.FromImage(buffer);
                using var brush = new SolidBrush(
                    GetColor332((byte)args[5]));

                g.FillRoundedRectangle(
                    brush,
                    new Rectangle(
                        Convert.ToInt32(args[0]),
                        Convert.ToInt32(args[1]),
                        Convert.ToInt32(args[2]),
                        Convert.ToInt32(args[3])),
                    new Size(Convert.ToInt32(args[4]), Convert.ToInt32(args[4])));

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(14, "drawPixel", 3, VariableType.None, async args =>
            {
                if (args.Length < 3)
                    return null;

                using var g = Graphics.FromImage(buffer);

                g.FillRectangle(
                    new SolidBrush(GetColor332((byte)args[2])),
                    Convert.ToInt32(args[0]),
                    Convert.ToInt32(args[1]),
                    1,
                    1);

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(15, "drawLine", 5, VariableType.None, async args =>
            {
                if (args.Length < 5)
                    return null;

                using var g = Graphics.FromImage(buffer);
                using var pen = new Pen(
                    GetColor332((byte)args[4]));

                g.DrawLine(
                    pen,
                    Convert.ToInt32(args[0]),
                    Convert.ToInt32(args[1]),
                    Convert.ToInt32(args[2]),
                    Convert.ToInt32(args[3]));

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(16, "fillCircle", 4, VariableType.None, async args =>
            {
                if (args.Length < 4)
                    return null;

                int x = Convert.ToInt32(args[0]);
                int y = Convert.ToInt32(args[1]);
                int radius = Convert.ToInt32(args[2]);

                using var g = Graphics.FromImage(buffer);
                using var brush = new SolidBrush(
                    GetColor332((byte)args[3]));

                g.FillEllipse(
                    brush,
                    x - radius,
                    y - radius,
                    radius * 2,
                    radius * 2);

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(17, "drawCircle", 4, VariableType.None, async args =>
            {
                if (args.Length < 4)
                    return null;

                int x = Convert.ToInt32(args[0]);
                int y = Convert.ToInt32(args[1]);
                int radius = Convert.ToInt32(args[2]);

                using var g = Graphics.FromImage(buffer);
                using var pen = new Pen(
                    GetColor332((byte)args[3]));

                g.DrawEllipse(
                    pen,
                    x - radius,
                    y - radius,
                    radius * 2,
                    radius * 2);

                OnInvalidate?.Invoke();
                return null;
            });


            vmFunctions.AddFunction(20, "cursor", 3, VariableType.None, async args =>
            {
                if (args.Length < 3) return null;
                textRenderer.SetCursor((int)args[0], (int)args[1]);
                textRenderer.SetTextSize((int)args[2]);
                return null;
            });

            vmFunctions.AddFunction(21, "print", 2, VariableType.None, async args =>
            {
                if (args.Length < 2) return null;

                string text = args[0] switch
                {
                    float f => f.ToString("0.00"),
                    double d => d.ToString("0.00"),
                    _ => args[0]?.ToString() ?? ""
                };

                using var g = Graphics.FromImage(buffer);

                textRenderer.SetGraphics(g);
                textRenderer.SetTextColor(GetColor332((byte)args[1]));
                textRenderer.Print(text);
                OnInvalidate?.Invoke();

                return null;
            });
            vmFunctions.AddFunction(22, "printCentered", 2, VariableType.None, async args =>
            {
                if (args.Length < 2) return null;

                string text = args[0] switch
                {
                    float f => f.ToString("0.00"),
                    double d => d.ToString("0.00"),
                    _ => args[0]?.ToString() ?? ""
                };

                using var g = Graphics.FromImage(buffer);

                textRenderer.SetGraphics(g);
                textRenderer.SetTextColor(GetColor332((byte)args[1]));
                textRenderer.SetCursor(textRenderer.CursorX - textRenderer.GetTextWidth(text) / 2, textRenderer.CursorY);
                textRenderer.Print(text);
                OnInvalidate?.Invoke();

                return null;
            });
            vmFunctions.AddFunction(23, "printRight", 2, VariableType.None, async args =>
            {
                if (args.Length < 2) return null;

                string text = args[0] switch
                {
                    float f => f.ToString("0.00"),
                    double d => d.ToString("0.00"),
                    _ => args[0]?.ToString() ?? ""
                };

                using var g = Graphics.FromImage(buffer);

                textRenderer.SetGraphics(g);
                textRenderer.SetTextColor(GetColor332((byte)args[1]));
                textRenderer.SetCursor(textRenderer.CursorX - textRenderer.GetTextWidth(text), textRenderer.CursorY);

                textRenderer.Print(text);
                OnInvalidate?.Invoke();

                return null;
            });

            vmFunctions.AddFunction(29, "drawSprite", 4, VariableType.None, async args =>
            {
                using var g = Graphics.FromImage(buffer);

                SpriteRenderer.DrawSprite(
                    g,
                    (int)args[0],
                    Convert.ToInt32(args[1]),
                    Convert.ToInt32(args[2]),
                    GetColor332((byte)args[3]));
                OnInvalidate?.Invoke();

                return null;
            });

            vmFunctions.AddFunction(30, "dialog", 1, VariableType.None, async args =>
            {

                string text = args[0]?.ToString() ?? "";

                using var g = Graphics.FromImage(buffer);

                g.Clear(Color.Black);
                g.FillRectangle(
                    new SolidBrush(Color.FromArgb(25, 28, 24)),
                    0,
                    0,
                    buffer.Width,
                    32);

                textRenderer.SetGraphics(g);

                textRenderer.SetTextSize(2);


                Color c = textRenderer.TextColor;
                textRenderer.SetTextColor(Color.Black);
                textRenderer.SetCursor(15, 14);
                textRenderer.Print(text);

                textRenderer.SetTextColor(Color.FromArgb(0, 154, 181));
                textRenderer.SetCursor(12, 10);
                textRenderer.Print(text);

                textRenderer.SetCursor(0, 32);
                textRenderer.SetTextSize(2);

                textRenderer.SetTextColor(c);

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(128, "collision", 8, VariableType.Bool, async args =>
            {
                float x1 = Convert.ToSingle(args[0]);
                float y1 = Convert.ToSingle(args[1]);
                float w1 = Convert.ToSingle(args[2]);
                float h1 = Convert.ToSingle(args[3]);

                float x2 = Convert.ToSingle(args[4]);
                float y2 = Convert.ToSingle(args[5]);
                float w2 = Convert.ToSingle(args[6]);
                float h2 = Convert.ToSingle(args[7]);

                bool result =
                    x1 - w1 / 2 < x2 + w2 / 2 &&
                    x1 + w1 / 2 > x2 - w2 / 2 &&
                    y1 - h1 / 2 < y2 + h2 / 2 &&
                    y1 + h1 / 2 > y2 - h2 / 2;

                return result;
            });




        }

        public static Color GetColor332(byte color)
        {
            int r = (color >> 5) & 0b111;
            int g = (color >> 2) & 0b111;
            int b = color & 0b11;

            int red = r * 255 / 7;
            int green = g * 255 / 7;
            int blue = b * 255 / 3;

            return Color.FromArgb(red, green, blue);
        }


    }
}
