using NTOSEmulator.Libs;
using WeifenLuo.WinFormsUI.Docking;

namespace NTOSDev.Components
{
    public partial class SpriteViewer : DockContent
    {
        private const int SpriteSize = 32;
        private const int Columns = 16;

        private const int CellWidth = 48;
        private const int CellHeight = 52;

        private const int SpriteCount = 1078;

        private int selectedSprite = -1;

        public int SelectedSprite => selectedSprite;

        public SpriteViewer()
        {
            InitializeComponent();

            AutoScroll = true;
            BackColor = Color.FromArgb(30, 30, 30);

            AutoScrollMinSize = new Size(
                Columns * CellWidth,
                GetRowCount() * CellHeight);
        }

        private int GetRowCount()
        {
            return (SpriteCount + Columns - 1) / Columns;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.TranslateTransform(
                AutoScrollPosition.X,
                AutoScrollPosition.Y);

            int visibleWidth = ClientSize.Width;
            int visibleHeight = ClientSize.Height;

            for (int spriteIndex = 0; spriteIndex < SpriteCount; spriteIndex++)
            {
                int column = spriteIndex % Columns;
                int row = spriteIndex / Columns;

                int x = column * CellWidth;
                int y = row * CellHeight;

                // Skip sprites outside the visible area
                if (x + CellWidth < -AutoScrollPosition.X ||
                    x > -AutoScrollPosition.X + visibleWidth ||
                    y + CellHeight < -AutoScrollPosition.Y ||
                    y > -AutoScrollPosition.Y + visibleHeight)
                {
                    continue;
                }

                DrawSpriteCell(e.Graphics, spriteIndex, x, y);
            }
        }

        private void DrawSpriteCell(
            Graphics graphics,
            int spriteIndex,
            int x,
            int y)
        {
            bool selected = spriteIndex == selectedSprite;

            // Cell background
            using (Brush background =
                new SolidBrush(
                    selected
                        ? Color.FromArgb(60, 90, 130)
                        : Color.FromArgb(40, 40, 40)))
            {
                graphics.FillRectangle(
                    background,
                    x,
                    y,
                    CellWidth - 1,
                    CellHeight - 1);
            }

            // Sprite
            SpriteRenderer.DrawSprite(
                graphics,
                spriteIndex,
                x + 8,
                y + 2,
                Color.White);

            // Index
            string text = spriteIndex.ToString();

            using (Brush textBrush =
                new SolidBrush(Color.LightGray))
            {
                SizeF size = graphics.MeasureString(
                    text,
                    Font);

                graphics.DrawString(
                    text,
                    Font,
                    textBrush,
                    x + (CellWidth - size.Width) / 2,
                    y + SpriteSize + 2);
            }

            // Selection border
            if (selected)
            {
                using Pen pen = new Pen(Color.DeepSkyBlue, 2);

                graphics.DrawRectangle(
                    pen,
                    x,
                    y,
                    CellWidth - 2,
                    CellHeight - 2);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            Point point = e.Location;

            point.Offset(
                -AutoScrollPosition.X,
                -AutoScrollPosition.Y);

            int column = point.X / CellWidth;
            int row = point.Y / CellHeight;

            if (column < 0 || column >= Columns ||
                row < 0)
            {
                return;
            }

            int spriteIndex = row * Columns + column;

            if (spriteIndex >= SpriteCount)
                return;

            selectedSprite = spriteIndex;

            Invalidate();
        }
    }
}