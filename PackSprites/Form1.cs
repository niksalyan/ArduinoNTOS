using System.Text;

namespace PackSprites
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


            string s = PackSpritesChunked(Directory.GetCurrentDirectory() + "\\monochrome_packed.png");
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\Sprites.h", s);
        }

        public static string PackSpritesChunked(
    string imagePath,
    string arrayName = "SPRITES")
        {
            using var bitmap = new Bitmap(imagePath);

            if (bitmap.Width % 16 != 0 || bitmap.Height % 16 != 0)
                throw new ArgumentException(
                    "Image dimensions must be divisible by 16.");

            const int SpriteSize = 16;
            const int SpriteBytes = 32;

            // 8 KB chunks = 256 sprites
            const int ChunkSize = 8 * 1024;
            const int SpritesPerChunk = ChunkSize / SpriteBytes;

            int spritesX = bitmap.Width / SpriteSize;
            int spritesY = bitmap.Height / SpriteSize;
            int spriteCount = spritesX * spritesY;

            byte[] data = new byte[spriteCount * SpriteBytes];

            int index = 0;

            for (int sy = 0; sy < spritesY; sy++)
            {
                for (int sx = 0; sx < spritesX; sx++)
                {
                    for (int y = 0; y < SpriteSize; y++)
                    {
                        byte row = 0;

                        for (int x = 0; x < SpriteSize; x++)
                        {
                            Color pixel = bitmap.GetPixel(
                                sx * SpriteSize + x,
                                sy * SpriteSize + y);

                            bool on = pixel.R > 160;

                            if (on)
                            {
                                row |= (byte)(
                                    1 << (7 - (x % 8)));
                            }

                            if (x % 8 == 7)
                            {
                                data[index++] = row;
                                row = 0;
                            }
                        }
                    }
                }
            }

            var sb = new StringBuilder();

            sb.AppendLine($@"
#pragma once
#include ""progmem_far.h""
#define SPRITE_SIZE 16
#define SPRITE_BYTES 32
#define SPRITE_SCALE 2
");

            int chunkCount =
                (spriteCount + SpritesPerChunk - 1)
                / SpritesPerChunk;

            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                int firstSprite =
                    chunk * SpritesPerChunk;

                int spritesInChunk =
                    Math.Min(
                        SpritesPerChunk,
                        spriteCount - firstSprite);

                int firstByte =
                    firstSprite * SpriteBytes;

                int bytesInChunk =
                    spritesInChunk * SpriteBytes;



                sb.AppendLine(
                    $"const uint8_t {arrayName}_{chunk}[] PROGMEM_FAR = {{");

                for (int i = 0; i < bytesInChunk; i++)
                {
                    if (i % 16 == 0)
                        sb.Append("    ");

                    sb.Append(
                        $"0x{data[firstByte + i]:X2}");

                    if (i < bytesInChunk - 1)
                        sb.Append(", ");

                    if (i % 16 == 15)
                        sb.AppendLine();
                }

                if (bytesInChunk % 16 != 0)
                    sb.AppendLine();

                sb.AppendLine("};");
                sb.AppendLine();
            }

            sb.AppendLine(
                $"const uint16_t {arrayName}_COUNT = {spriteCount};");

            sb.AppendLine(
                $"const uint16_t {arrayName}_CHUNK_COUNT = {chunkCount};");

            sb.AppendLine(
                $"const uint16_t {arrayName}_SPRITES_PER_CHUNK = {SpritesPerChunk};");

            return sb.ToString();
        }
        public static string PackSprites(string imagePath, string arrayName = "SPRITES")
        {
            using var bitmap = new Bitmap(imagePath);

            if (bitmap.Width % 16 != 0 || bitmap.Height % 16 != 0)
                throw new ArgumentException("Image dimensions must be divisible by 16.");

            int spritesX = bitmap.Width / 16;
            int spritesY = bitmap.Height / 16;
            int spriteCount = spritesX * spritesY;

            // 16x16 = 256 bits = 32 bytes
            byte[] data = new byte[spriteCount * 32];

            int index = 0;

            for (int sy = 0; sy < spritesY; sy++)
            {
                for (int sx = 0; sx < spritesX; sx++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        byte row = 0;

                        for (int x = 0; x < 16; x++)
                        {
                            Color pixel = bitmap.GetPixel(
                                sx * 16 + x,
                                sy * 16 + y);

                            // Treat dark pixels as 1
                            bool on = pixel.R > 160;

                            if (on)
                                row |= (byte)(1 << (7 - (x % 8)));

                            // Every 8 pixels produces one byte
                            if (x % 8 == 7)
                            {
                                data[index++] = row;
                                row = 0;
                            }
                        }
                    }
                }
            }

            var sb = new StringBuilder();

            sb.AppendLine($"const uint8_t {arrayName}[] PROGMEM_FAR = {{");

            for (int i = 0; i < data.Length; i++)
            {
                if (i % 16 == 0)
                    sb.Append("    ");

                sb.Append($"0x{data[i]:X2}");

                if (i < data.Length - 1)
                    sb.Append(", ");

                if (i % 16 == 15)
                    sb.AppendLine();
            }

            if (data.Length % 16 != 0)
                sb.AppendLine();

            sb.AppendLine("};");

            sb.AppendLine();
            sb.AppendLine($"const uint16_t {arrayName}_COUNT = {spriteCount};");

            return sb.ToString();
        }
    }
}
