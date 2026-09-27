
using System.Drawing;

namespace NTOSImage.Models
{
    internal class InputModel
    {
        public Bitmap Image { get; set; }

        public Size Resize { get; set; } =
            new Size(240, 160);

        public Boolean Bicubix { get; set; } = false;

        public Boolean Dithering { get; set; } = false;
        public Boolean Invert { get; set; } = false;

        public Boolean Grayscale { get; set; } = false;
    }

}

