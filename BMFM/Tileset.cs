using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace BMFM
{
    public class Tileset
    {
        public Bitmap[] images;
        public Palette p;

        /// <summary>
        /// Loads a tileset from a stream. The stream must be positioned at the beginning of the level data
        /// </summary>
        /// <param name="s">The stream that the tileset is loaded from.</param>
        public Tileset(Stream s)
        {
            // It will be necessary to return to the level data
            long pos = s.Position;
            // Skip past the level data to the palette
            s.Seek(2, SeekOrigin.Current);
            // Go to the palette
            int palPtr = s.ReadByte() | s.ReadByte() << 8;
            s.Seek(SNES.toPC(palPtr, 0x81), SeekOrigin.Begin);
            // Load the palette
            p = new Palette(s);
            SNES.MakePltTransparent(p.colors);

            // Return to the beginning of the level data
            s.Seek(pos, SeekOrigin.Begin);
            // Go to the level data
            int BGPointer = s.ReadByte() | s.ReadByte() << 8;
            s.Seek(SNES.toPC(BGPointer, 0x84), SeekOrigin.Begin);

            // Skip to the first page of graphics
            s.Seek(0x1e, SeekOrigin.Current);
            byte[] GFX = BMFMCompress.Decompress(s);
            // And second, etc.
            s.Seek(3, SeekOrigin.Current);
            byte[] GFX2 = BMFMCompress.Decompress(s);
            s.Seek(3, SeekOrigin.Current);
            byte[] GFX3 = BMFMCompress.Decompress(s);

            // Return to the background data
            s.Seek(SNES.toPC(BGPointer, 0x84), SeekOrigin.Begin);
            // Go to the map16 data
            s.Seek(8, SeekOrigin.Current);
            // Load it (interleaved again)
            byte[] tileset = BMFMCompress.Decompress(s);
            byte[] tileset2 = BMFMCompress.Decompress(s);
            BMFMCompress.Interleave(tileset, tileset2);

            // Convert the graphics to linear form and combine them all into one array
            byte[][,] linGFX = new byte[0x300][,];
            for (int i = 0; i < 256; i++)
                linGFX[i] = SNES.PlanarToLinear(GFX, i * 0x20);
            for (int i = 0; i < 256; i++)
                linGFX[i + 0x100] = SNES.PlanarToLinear(GFX2, i * 0x20);
            for (int i = 0; i < 256; i++)
                linGFX[i + 0x200] = SNES.PlanarToLinear(GFX3, i * 0x20);

            // allocate the tileset images, there are 0x20 bytes per tile
            int numImages = tileset.Length / 0x20;
            images = new Bitmap[numImages];
            // Now it's finally time to load the images
            for (int block = 0; block < numImages; block++)
            {
                // Each image is 32x32
                Bitmap img = new Bitmap(32, 32, PixelFormat.Format8bppIndexed);
                // Add the palette to this image
                SNES.FillPalette(img, p.colors);
                // Lock the image. Without this, loading would be really slow
                BitmapData d = img.LockBits(new Rectangle(0, 0, 32, 32), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                // Read the first two bytes of the current block
                int blockStart = tileset[block * 0x20] | tileset[block * 0x20 + 1] << 8;
                // If this is >= 0xFDB0, then this block is a horizontally flipped copy of another tile
                if (blockStart >= 0xFDB0)
                {
                    // The number of the tile to flip is this
                    blockStart -= 0xFDB0;
                    // Copy the graphics for the flipped tile
                    for (int tile = 0; tile < 16; tile++)
                    {
                        int x = (tile % 4) * 8;
                        int y = (tile / 4) * 8;
                        int tile2 = (tile / 4) * 4 + 3 - (tile % 4);
                        SNES.DrawTile(d, x, y, (byte)(tileset[blockStart * 0x20 + tile2 * 2 + 1] ^ 0x40), tileset[blockStart * 0x20 + tile2 * 2], linGFX);
                    }
                }
                else
                {
                    // Copy the graphics regular
                    for (int tile = 0; tile < 16; tile++)
                    {
                        int x = (tile % 4) * 8;
                        int y = (tile / 4) * 8;
                        SNES.DrawTile(d, x, y, tileset[block * 0x20 + tile * 2 + 1], tileset[block * 0x20 + tile * 2], linGFX);
                    }
                }
                img.UnlockBits(d);
                images[block] = img;
            }
        }
    }
}
