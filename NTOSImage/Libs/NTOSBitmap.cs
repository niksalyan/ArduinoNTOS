using NTOSImage.Models;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Imaging.Effects;

namespace NTOSImage.Libs
{
    public class NTOSBitmap
    {
        private readonly InputModel input;

        private byte[]? _bytes;

        public NTOSBitmap(string file)
        {
            _bytes = File.ReadAllBytes(file);
        }

        public NTOSBitmap(InputModel input)
        {
            this.input =
                input ?? throw new ArgumentNullException(nameof(input));

            if (input.Image == null)
                throw new ArgumentNullException(
                    nameof(input.Image));

            if (input.Resize.Width <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(input.Resize.Width));

            if (input.Resize.Height <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(input.Resize.Height));

            // NTI stores dimensions in a single byte.
            if (input.Resize.Width > byte.MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(input.Resize.Width),
                    "NTOS image width cannot exceed 255.");

            if (input.Resize.Height > byte.MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(input.Resize.Height),
                    "NTOS image height cannot exceed 255.");
        }

        // ============================================================
        // Public API
        // ============================================================


        public byte[] ToByteArray()
        {
            if (_bytes != null)
                return _bytes;

            using Bitmap resized =
                ResizeImage(
                    input.Image,
                    input.Resize,
                    input.Bicubix);

            if (input.Invert)
            {
                resized.ApplyEffect(new InvertEffect());
            }

            if (input.Grayscale)
            {
                resized.ApplyEffect(new GrayScaleEffect());
            }

            _bytes =
                EncodeBitmap(
                    resized,
                    input.Dithering);

            return _bytes;
        }

        public Bitmap ToBitmap()
        {
            byte[] bytes =
                ToByteArray();

            return DecodeBitmap(bytes);
        }

        // ============================================================
        // Resize
        // ============================================================

        private static Bitmap ResizeImage(
            Bitmap source,
            Size size,
            bool bicubic)
        {
            Bitmap result =
                new Bitmap(
                    size.Width,
                    size.Height,
                    PixelFormat.Format24bppRgb);

            using Graphics graphics =
                Graphics.FromImage(result);

            graphics.Clear(Color.Black);

            graphics.InterpolationMode =
                bicubic
                    ? InterpolationMode.HighQualityBicubic
                    : InterpolationMode.HighQualityBilinear;

            graphics.PixelOffsetMode =
                PixelOffsetMode.HighQuality;

            graphics.CompositingQuality =
                CompositingQuality.HighQuality;

            graphics.SmoothingMode =
                SmoothingMode.HighQuality;

            graphics.DrawImage(
                source,
                new Rectangle(
                    0,
                    0,
                    size.Width,
                    size.Height));

            return result;
        }

        // ============================================================
        // Encode
        // ============================================================

        private static byte[] EncodeBitmap(
            Bitmap bitmap,
            bool dithering)
        {
            int width =
                bitmap.Width;

            int height =
                bitmap.Height;

            int pixelCount =
                width * height;

            // --------------------------------------------------------
            // NTI FORMAT
            //
            // Byte 0 = width
            // Byte 1 = height
            // Byte 2+ = RGB332 pixels
            // --------------------------------------------------------

            byte[] result =
                new byte[2 + pixelCount];

            // --------------------------------------------------------
            // Header
            // --------------------------------------------------------

            result[0] =
                (byte)width;

            result[1] =
                (byte)height;

            // --------------------------------------------------------
            // Read source pixels
            // --------------------------------------------------------

            using Bitmap source =
                new Bitmap(
                    bitmap.Width,
                    bitmap.Height,
                    PixelFormat.Format24bppRgb);

            using (Graphics g =
                Graphics.FromImage(source))
            {
                g.DrawImageUnscaled(
                    bitmap,
                    0,
                    0);
            }

            Rectangle rect =
                new Rectangle(
                    0,
                    0,
                    width,
                    height);

            BitmapData data =
                source.LockBits(
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);

            try
            {
                if (dithering)
                {
                    EncodeDithered(
                        data,
                        result,
                        width,
                        height);
                }
                else
                {
                    EncodeNormal(
                        data,
                        result,
                        width,
                        height);
                }
            }
            finally
            {
                source.UnlockBits(data);
            }

            return result;
        }

        // ============================================================
        // Normal RGB332 conversion
        // ============================================================

        private static void EncodeNormal(
            BitmapData data,
            byte[] output,
            int width,
            int height)
        {
            int stride =
                data.Stride;

            IntPtr scan0 =
                data.Scan0;

            unsafe
            {
                byte* basePtr =
                    (byte*)scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* row =
                        basePtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        // Format24bppRgb is BGR.
                        byte b =
                            row[x * 3 + 0];

                        byte g =
                            row[x * 3 + 1];

                        byte r =
                            row[x * 3 + 2];

                        output[
                            2 + y * width + x] =
                            ToRGB332(
                                r,
                                g,
                                b);
                    }
                }
            }
        }

        // ============================================================
        // Floyd-Steinberg dithering
        // ============================================================

        private static void EncodeDithered(
            BitmapData data,
            byte[] output,
            int width,
            int height)
        {
            int stride =
                data.Stride;

            IntPtr scan0 =
                data.Scan0;

            // Only keep errors for the current
            // and next row.
            float[] errorR =
                new float[width + 2];

            float[] errorG =
                new float[width + 2];

            float[] errorB =
                new float[width + 2];

            float[] nextErrorR =
                new float[width + 2];

            float[] nextErrorG =
                new float[width + 2];

            float[] nextErrorB =
                new float[width + 2];

            unsafe
            {
                byte* basePtr =
                    (byte*)scan0;

                for (int y = 0; y < height; y++)
                {
                    Array.Clear(
                        nextErrorR,
                        0,
                        nextErrorR.Length);

                    Array.Clear(
                        nextErrorG,
                        0,
                        nextErrorG.Length);

                    Array.Clear(
                        nextErrorB,
                        0,
                        nextErrorB.Length);

                    byte* row =
                        basePtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int errorIndex =
                            x + 1;

                        float b =
                            row[x * 3 + 0] +
                            errorB[errorIndex];

                        float g =
                            row[x * 3 + 1] +
                            errorG[errorIndex];

                        float r =
                            row[x * 3 + 2] +
                            errorR[errorIndex];

                        r = Clamp(
                            r,
                            0,
                            255);

                        g = Clamp(
                            g,
                            0,
                            255);

                        b = Clamp(
                            b,
                            0,
                            255);

                        byte packed =
                            ToRGB332(
                                (byte)r,
                                (byte)g,
                                (byte)b);

                        // 2-byte NTI header.
                        output[
                            2 + y * width + x] =
                            packed;

                        // Reconstruct quantized RGB
                        // to calculate the error.
                        FromRGB332(
                            packed,
                            out int quantizedR,
                            out int quantizedG,
                            out int quantizedB);

                        float diffR =
                            r - quantizedR;

                        float diffG =
                            g - quantizedG;

                        float diffB =
                            b - quantizedB;

                        // --------------------------------------------
                        // Floyd-Steinberg
                        //
                        //             current    7/16
                        //       3/16  5/16       1/16
                        // --------------------------------------------

                        AddError(
                            errorR,
                            errorG,
                            errorB,
                            errorIndex + 1,
                            diffR,
                            diffG,
                            diffB,
                            7.0f / 16.0f);

                        AddError(
                            nextErrorR,
                            nextErrorG,
                            nextErrorB,
                            errorIndex - 1,
                            diffR,
                            diffG,
                            diffB,
                            3.0f / 16.0f);

                        AddError(
                            nextErrorR,
                            nextErrorG,
                            nextErrorB,
                            errorIndex,
                            diffR,
                            diffG,
                            diffB,
                            5.0f / 16.0f);

                        AddError(
                            nextErrorR,
                            nextErrorG,
                            nextErrorB,
                            errorIndex + 1,
                            diffR,
                            diffG,
                            diffB,
                            1.0f / 16.0f);
                    }

                    // Swap current and next buffers.
                    Swap(
                        ref errorR,
                        ref nextErrorR);

                    Swap(
                        ref errorG,
                        ref nextErrorG);

                    Swap(
                        ref errorB,
                        ref nextErrorB);
                }
            }
        }

        // ============================================================
        // RGB888 -> RGB332
        // ============================================================

        private static byte ToRGB332(
            byte r,
            byte g,
            byte b)
        {
            byte rr =
                (byte)(r >> 5);

            byte gg =
                (byte)(g >> 5);

            byte bb =
                (byte)(b >> 6);

            return (byte)(
                (rr << 5) |
                (gg << 2) |
                bb);
        }

        // ============================================================
        // RGB332 -> RGB888
        // ============================================================

        private static void FromRGB332(
            byte value,
            out int r,
            out int g,
            out int b)
        {
            int rr =
                (value >> 5) & 0x07;

            int gg =
                (value >> 2) & 0x07;

            int bb =
                value & 0x03;

            r =
                (rr * 255) / 7;

            g =
                (gg * 255) / 7;

            b =
                (bb * 255) / 3;
        }

        // ============================================================
        // Decode
        // ============================================================

        private static Bitmap DecodeBitmap(
            byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(
                    nameof(bytes));

            // --------------------------------------------------------
            // Minimum:
            //
            // 1 byte width
            // 1 byte height
            // --------------------------------------------------------

            if (bytes.Length < 2)
                throw new ArgumentException(
                    "Invalid NTOS image.");

            int width =
                bytes[0];

            int height =
                bytes[1];

            if (width <= 0 ||
                height <= 0)
            {
                throw new ArgumentException(
                    "Invalid NTOS image dimensions.");
            }

            int expectedSize =
                2 + width * height;

            if (bytes.Length < expectedSize)
            {
                throw new ArgumentException(
                    "NTOS image data is incomplete.");
            }

            Bitmap bitmap =
                new Bitmap(
                    width,
                    height,
                    PixelFormat.Format24bppRgb);

            Rectangle rect =
                new Rectangle(
                    0,
                    0,
                    width,
                    height);

            BitmapData data =
                bitmap.LockBits(
                    rect,
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);

            try
            {
                int stride =
                    data.Stride;

                unsafe
                {
                    byte* basePtr =
                        (byte*)data.Scan0;

                    for (int y = 0; y < height; y++)
                    {
                        byte* row =
                            basePtr + y * stride;

                        for (int x = 0; x < width; x++)
                        {
                            byte packed =
                                bytes[
                                    2 +
                                    y * width +
                                    x];

                            FromRGB332(
                                packed,
                                out int r,
                                out int g,
                                out int b);

                            // Format24bppRgb is BGR.
                            row[x * 3 + 0] =
                                (byte)b;

                            row[x * 3 + 1] =
                                (byte)g;

                            row[x * 3 + 2] =
                                (byte)r;
                        }
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        // ============================================================
        // Helpers
        // ============================================================

        private static float Clamp(
            float value,
            float min,
            float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        private static void AddError(
            float[] errorR,
            float[] errorG,
            float[] errorB,
            int index,
            float r,
            float g,
            float b,
            float factor)
        {
            if (index < 0 ||
                index >= errorR.Length)
            {
                return;
            }

            errorR[index] +=
                r * factor;

            errorG[index] +=
                g * factor;

            errorB[index] +=
                b * factor;
        }

        private static void Swap<T>(
            ref T first,
            ref T second)
        {
            T temp =
                first;

            first =
                second;

            second =
                temp;
        }
    }
}