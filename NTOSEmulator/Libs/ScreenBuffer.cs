using NTOSCompiler;
using NTOSCompiler.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NTOSEmulator.Libs
{
    internal class ScreenBuffer
    {
        private readonly Bitmap buffer = new Bitmap(480, 320, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        private TextRenderer textRenderer = new TextRenderer();

        public Action OnInvalidate;

        public readonly Dictionary<string, byte> colors = new Dictionary<string, byte>
        {
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
            

            vmFunctions.AddFunction(9, "cls", VariableType.None, async args =>
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

            vmFunctions.AddFunction(10, "drawBox", VariableType.None, async args =>
            {
                if (args.Length < 5)
                    return null;

                using var g = Graphics.FromImage(buffer);
                using var pen = new Pen(
                    GetColor332((byte)args[4]));

                g.DrawRectangle(
                    pen,
                    new Rectangle(
                        (int)args[0],
                        (int)args[1],
                        (int)args[2],
                        (int)args[3]));

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(11, "fillBox", VariableType.None, async args =>
            {
                if (args.Length < 5)
                    return null;

                using var g = Graphics.FromImage(buffer);
                using var brush = new SolidBrush(
                    GetColor332((byte)args[4]));

                g.FillRectangle(
                    brush,
                    new Rectangle(
                        (int)args[0],
                        (int)args[1],
                        (int)args[2],
                        (int)args[3]));

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(12, "drawPixel", VariableType.None, async args =>
            {
                if (args.Length < 3)
                    return null;

                using var g = Graphics.FromImage(buffer);

                g.FillRectangle(
                    new SolidBrush(GetColor332((byte)args[2])),
                    (int)args[0],
                    (int)args[1],
                    1,
                    1);

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(13, "drawLine", VariableType.None, async args =>
            {
                if (args.Length < 5)
                    return null;

                using var g = Graphics.FromImage(buffer);
                using var pen = new Pen(
                    GetColor332((byte)args[4]));

                g.DrawLine(
                    pen,
                    (int)args[0],
                    (int)args[1],
                    (int)args[2],
                    (int)args[3]);

                OnInvalidate?.Invoke();
                return null;
            });

            vmFunctions.AddFunction(15, "fillCircle", VariableType.None, async args =>
            {
                if (args.Length < 4)
                    return null;

                int x = (int)args[0];
                int y = (int)args[1];
                int radius = (int)args[2];

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

            vmFunctions.AddFunction(14, "drawCircle", VariableType.None, async args =>
            {
                if (args.Length < 4)
                    return null;

                int x = (int)args[0];
                int y = (int)args[1];
                int radius = (int)args[2];

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


            vmFunctions.AddFunction(20, "cursor", VariableType.None, async args =>
            {
                if (args.Length < 3) return null;
                textRenderer.SetCursor((int)args[0], (int)args[1]);
                textRenderer.SetTextSize((int)args[2]);
                return null;
            });

            vmFunctions.AddFunction(21, "print", VariableType.None, async args =>
            {
                if (args.Length < 2) return null;

                string text = args[0]?.ToString() ?? "";

                using var g = Graphics.FromImage(buffer);
                
                textRenderer.SetGraphics(g);
                textRenderer.SetTextColor(GetColor332((byte)args[1]));
                textRenderer.Print(text);
                OnInvalidate?.Invoke();

                return null;
            });
            vmFunctions.AddFunction(22, "printCentered", VariableType.None, async args =>
            {
                if (args.Length < 2) return null;

                string text = args[0]?.ToString() ?? "";

                using var g = Graphics.FromImage(buffer);

                textRenderer.SetGraphics(g);
                textRenderer.SetTextColor(GetColor332((byte)args[1]));
                textRenderer.SetCursor(textRenderer.CursorX - textRenderer.GetTextWidth(text) / 2, textRenderer.CursorY);
                textRenderer.Print(text);
                OnInvalidate?.Invoke();

                return null;
            });
            vmFunctions.AddFunction(23, "printRight", VariableType.None, async args =>
            {
                if (args.Length < 2) return null;

                string text = args[0]?.ToString() ?? "";

                using var g = Graphics.FromImage(buffer);

                textRenderer.SetGraphics(g);
                textRenderer.SetTextColor(GetColor332((byte)args[1]));
                textRenderer.SetCursor(textRenderer.CursorX - textRenderer.GetTextWidth(text), textRenderer.CursorY);

                textRenderer.Print(text);
                OnInvalidate?.Invoke();

                return null;
            });

            vmFunctions.AddFunction(30, "dialog", VariableType.None, async args =>
            {
                

                if (args.Length < 1)
                    return null;

                string text = args[0]?.ToString() ?? "";

                using var g = Graphics.FromImage(buffer);
                
                g.Clear(Color.Black);
                g.FillRectangle(
                    new SolidBrush(GetColor332(colors["BLUE"])),
                    0,
                    0,
                    buffer.Width,
                    25);

                textRenderer.SetGraphics(g);
                
                textRenderer.SetTextSize(2);


                Color c = textRenderer.TextColor;
                textRenderer.SetTextColor(Color.Black);
                textRenderer.SetCursor(243 - textRenderer.GetTextWidth(text) / 2, 9);
                textRenderer.Print(text);

                textRenderer.SetTextColor(Color.White);
                textRenderer.SetCursor(240 - textRenderer.GetTextWidth(text) / 2, 5);
                textRenderer.Print(text);

                textRenderer.SetCursor(0, 26);
                textRenderer.SetTextSize(2);

                textRenderer.SetTextColor(c);

                OnInvalidate?.Invoke();
                return null;
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
