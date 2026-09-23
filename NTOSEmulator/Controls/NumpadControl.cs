using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace NTOSEmulator.Controls
{
    public class NumpadControl : UserControl
    {
        private readonly char[,] _keys =
        {
        { '1', '2', '3', 'A' },
        { '4', '5', '6', 'B' },
        { '7', '8', '9', 'C' },
        { '*', '0', '#', 'D' }
    };

        private int _hoverRow = -1;
        private int _hoverColumn = -1;

        private int _pressedRow = -1;
        private int _pressedColumn = -1;

        private char _lastKey;

        public event EventHandler<char>? KeyPressed;

        public char LastKey => _lastKey;

        public int KeySpacing { get; set; } = 8;

        public Color BackgroundColor { get; set; } =
            Color.FromArgb(24, 27, 32);

        public Color KeyColor { get; set; } =
            Color.FromArgb(45, 49, 58);

        public Color KeyHoverColor { get; set; } =
            Color.FromArgb(58, 64, 75);

        public Color KeyPressedColor { get; set; } =
            Color.FromArgb(70, 125, 175);

        public Color KeyTextColor { get; set; } =
            Color.FromArgb(235, 238, 242);

        public Color SecondaryTextColor { get; set; } =
            Color.FromArgb(145, 153, 165);

        public NumpadControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);

            TabStop = true;
            DoubleBuffered = true;

            BackColor = BackgroundColor;
            ForeColor = KeyTextColor;

            MinimumSize = new Size(180, 180);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            DrawBackground(g);

            Rectangle[,] rectangles = CalculateKeyRectangles();

            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    DrawKey(
                        g,
                        rectangles[row, column],
                        _keys[row, column],
                        row,
                        column);
                }
            }
        }

        private void DrawBackground(Graphics g)
        {
            using var backgroundBrush = new SolidBrush(BackgroundColor);

            g.FillRectangle(
                backgroundBrush,
                ClientRectangle);

            // Very subtle inner border.
            using var borderPen = new Pen(
                Color.FromArgb(55, 60, 70),
                1);

            Rectangle border = ClientRectangle;

            border.Width -= 1;
            border.Height -= 1;

            g.DrawRectangle(borderPen, border);
        }

        private void DrawKey(
            Graphics g,
            Rectangle rectangle,
            char key,
            int row,
            int column)
        {
            bool hovered =
                row == _hoverRow &&
                column == _hoverColumn;

            bool pressed =
                row == _pressedRow &&
                column == _pressedColumn;

            Color keyColor = KeyColor;

            if (pressed)
                keyColor = KeyPressedColor;
            else if (hovered)
                keyColor = KeyHoverColor;

            // Shadow
            Rectangle shadowRect = rectangle;
            shadowRect.Y += pressed ? 2 : 4;

            using (GraphicsPath shadowPath =
                   CreateRoundedRectangle(shadowRect, 10))
            using (SolidBrush shadowBrush =
                   new(Color.FromArgb(70, 0, 0, 0)))
            {
                g.FillPath(shadowBrush, shadowPath);
            }

            // Main key
            Rectangle keyRect = rectangle;

            if (pressed)
                keyRect.Y += 2;

            using (GraphicsPath path =
                   CreateRoundedRectangle(keyRect, 10))
            using (SolidBrush brush =
                   new(keyColor))
            {
                g.FillPath(brush, path);

                // Subtle border
                using var borderPen = new Pen(
                    hovered || pressed
                        ? Color.FromArgb(100, 130, 160)
                        : Color.FromArgb(65, 70, 80),
                    1);

                g.DrawPath(borderPen, path);
            }

            // Highlight line at the top.
            if (!pressed)
            {
                using var highlightPen = new Pen(
                    Color.FromArgb(35, 255, 255, 255),
                    1);

                int left = keyRect.Left + 7;
                int right = keyRect.Right - 7;
                int y = keyRect.Top + 1;

                g.DrawLine(
                    highlightPen,
                    left,
                    y,
                    right,
                    y);
            }

            DrawKeyText(
                g,
                keyRect,
                key,
                hovered,
                pressed);
        }

        private void DrawKeyText(
            Graphics g,
            Rectangle rectangle,
            char key,
            bool hovered,
            bool pressed)
        {
            string text = key.ToString();

            using var font = new Font(
                Font.FontFamily,
                Math.Max(10, rectangle.Height * 0.30f),
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            Color color = KeyTextColor;

            if (pressed)
                color = Color.White;
            else if (hovered)
                color = Color.FromArgb(245, 248, 252);

            using var brush = new SolidBrush(color);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            Rectangle textRect = rectangle;

            if (pressed)
                textRect.Y += 2;

            g.DrawString(
                text,
                font,
                brush,
                textRect,
                format);
        }

        private Rectangle[,] CalculateKeyRectangles()
        {
            Rectangle[,] result = new Rectangle[4, 4];

            int availableWidth =
                ClientSize.Width -
                Padding.Left -
                Padding.Right -
                KeySpacing * 3;

            int availableHeight =
                ClientSize.Height -
                Padding.Top -
                Padding.Bottom -
                KeySpacing * 3;

            int keyWidth = availableWidth / 4;
            int keyHeight = availableHeight / 4;

            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    int x =
                        Padding.Left +
                        column * (keyWidth + KeySpacing);

                    int y =
                        Padding.Top +
                        row * (keyHeight + KeySpacing);

                    result[row, column] =
                        new Rectangle(
                            x,
                            y,
                            keyWidth,
                            keyHeight);
                }
            }

            return result;
        }

        private bool TryGetKey(
            Point location,
            out int row,
            out int column)
        {
            Rectangle[,] rectangles =
                CalculateKeyRectangles();

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (rectangles[r, c].Contains(location))
                    {
                        row = r;
                        column = c;
                        return true;
                    }
                }
            }

            row = -1;
            column = -1;

            return false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (TryGetKey(
                e.Location,
                out int row,
                out int column))
            {
                if (_hoverRow != row ||
                    _hoverColumn != column)
                {
                    _hoverRow = row;
                    _hoverColumn = column;

                    Invalidate();
                }
            }
            else
            {
                if (_hoverRow != -1 ||
                    _hoverColumn != -1)
                {
                    _hoverRow = -1;
                    _hoverColumn = -1;

                    Invalidate();
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _hoverRow = -1;
            _hoverColumn = -1;

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            Focus();

            if (TryGetKey(
                e.Location,
                out int row,
                out int column))
            {
                _pressedRow = row;
                _pressedColumn = column;

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left)
                return;

            int row = _pressedRow;
            int column = _pressedColumn;

            _pressedRow = -1;
            _pressedColumn = -1;

            Invalidate();

            if (row < 0 || column < 0)
                return;

            if (TryGetKey(
                e.Location,
                out int currentRow,
                out int currentColumn) &&
                currentRow == row &&
                currentColumn == column)
            {
                EmitKey(_keys[row, column]);
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            char? key = ConvertKeyboardKey(e.KeyCode);

            if (key.HasValue)
            {
                EmitKey(key.Value);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private char? ConvertKeyboardKey(Keys key)
        {
            return key switch
            {
                Keys.D0 or Keys.NumPad0 => '0',
                Keys.D1 or Keys.NumPad1 => '1',
                Keys.D2 or Keys.NumPad2 => '2',
                Keys.D3 or Keys.NumPad3 => '3',
                Keys.D4 or Keys.NumPad4 => '4',
                Keys.D5 or Keys.NumPad5 => '5',
                Keys.D6 or Keys.NumPad6 => '6',
                Keys.D7 or Keys.NumPad7 => '7',
                Keys.D8 or Keys.NumPad8 => '8',
                Keys.D9 or Keys.NumPad9 => '9',

                Keys.A => 'A',
                Keys.B => 'B',
                Keys.C => 'C',
                Keys.D => 'D',

                Keys.Multiply => '*',

                Keys.Enter => '#',
                Keys.Oem1 => '#',

                Keys.Escape => '*',

                _ => null
            };
        }

        private void EmitKey(char key)
        {
            _lastKey = key;

            KeyPressed?.Invoke(this, key);

            Invalidate();
        }

        private static GraphicsPath CreateRoundedRectangle(
            Rectangle rectangle,
            int radius)
        {
            GraphicsPath path = new();

            int diameter = radius * 2;

            Rectangle arc = new(
                rectangle.X,
                rectangle.Y,
                diameter,
                diameter);

            path.AddArc(
                arc,
                180,
                90);

            arc.X =
                rectangle.Right -
                diameter;

            path.AddArc(
                arc,
                270,
                90);

            arc.Y =
                rectangle.Bottom -
                diameter;

            path.AddArc(
                arc,
                0,
                90);

            arc.X =
                rectangle.Left;

            path.AddArc(
                arc,
                90,
                90);

            path.CloseFigure();

            return path;
        }
    }
}
