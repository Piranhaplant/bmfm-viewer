using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BMFM
{
    class SNES
    {
        /// <summary>
        /// Convert a SNES address to a PC one
        /// </summary>
        /// <param name="address">SNES lower 2 bytes of address</param>
        /// <param name="bank">SNES address bank (high byte)</param>
        /// <returns>The PC address</returns>
        public static int toPC(int address, int bank)
        {
            return (bank - 0x80) * 0x8000 + address - 0x8000;
        }

        /// <summary>
        /// Convert a SNES color to a standard 32-bit RGB color
        /// </summary>
        /// <param name="low">The low byte. Since the SNES is little endian this will be the first byte in memory</param>
        /// <param name="high">The high byte. Acutally the second byte in memory</param>
        /// <returns>The color</returns>
        public static Color toRGB(byte low, byte high)
        {
            int v = low | high << 8;
            return Color.FromArgb((v % 0x20) * 8, ((v / 0x20) % 0x20) * 8, ((v / 0x400) % 0x20) * 8);
        }

        /// <summary>
        /// Converts an 8x8 tile of SNES planar graphics to linear format
        /// </summary>
        /// <param name="bytes">The planar graphics</param>
        /// <param name="index">The index into "bytes" to convert from</param>
        /// <returns>The linear graphics</returns>
        public static byte[,] PlanarToLinear(byte[] bytes, int index)
        {
            byte[,] result = new byte[8, 8];
            int line = 0;
            int bit = 0;
            for (int l = index; l <= index + 0x1f; l += 2)
            {
                for (int m = 0; m <= 7; m++)
                {
                    if ((bytes[l] & (1 << m)) != 0)
                    {
                        result[line, 7 - m] = (byte)(result[line, 7 - m] | (1 << bit));
                    }
                    if ((bytes[l + 1] & (1 << m)) != 0)
                    {
                        result[line, 7 - m] = (byte)(result[line, 7 - m] | (1 << bit + 1));
                    }
                }
                line += 1;
                if (line == 8)
                {
                    line = 0;
                    bit += 2;
                }
            }
            return result;
        }

        /// <summary>
        /// Renders a tile onto a bitmap. Much slower than the one using BitmapData
        /// </summary>
        /// <param name="bmp">The image to render the graphics on. Must be 8bpp indexed</param>
        /// <param name="x">The x coordinate of bmp to render onto</param>
        /// <param name="y">The y coordinate of bmp to render onto</param>
        /// <param name="gfx">The SNES graphics data. These are planar</param>
        /// <param name="gfxindex">The index into gfx to load from</param>
        /// <param name="palette">The graphics palette</param>
        /// <param name="palIndex">The index into palette to load from</param>
        /// <param name="xFlip">Flip the tile horizontally</param>
        /// <param name="yFlip">Flip the tile vertically</param>
        public static void DrawTile(Bitmap bmp, int x, int y, byte[] gfx, int gfxindex, Color[] palette, int palIndex, bool xFlip, bool yFlip)
        {
            byte[,] tile = PlanarToLinear(gfx, gfxindex);
            int xStep = 1;
            int yStep = 1;
            if (xFlip)
            {
                x += 7;
                xStep = -1;
            }
            int xOrig = x;
            if (yFlip)
            {
                y += 7;
                yStep = -1;
            }
            for (int l = 0; l <= 7; l++)
            {
                for (int m = 0; m <= 7; m++)
                {
                    if (palette[palIndex + tile[l, m]].A > 0)
                    {
                        bmp.SetPixel(x, y, palette[palIndex + tile[l, m]]);
                    }
                    x += xStep;
                }
                y += yStep;
                x = xOrig;
            }
        }

        /// <summary>
        /// Renders a tile onto a bitmap where the grahpics come from a stream. Much slower than the one using BitmapData
        /// </summary>
        /// <param name="bmp">The image to render the grahpics on. Must be 8bpp indexed</param>
        /// <param name="x">The x coordinate of bmp to render onto</param>
        /// <param name="y">The y coordinate of bmp to render onto</param>
        /// <param name="s">The stream to load the graphics from</param>
        /// <param name="palette">The graphics palette</param>
        /// <param name="palIndex">The index into palette to load from</param>
        /// <param name="xFlip">Flip the tile horizontally</param>
        /// <param name="yFlip">Flip the tile vertically</param>
        public static void DrawTile(Bitmap bmp, int x, int y, System.IO.Stream s, Color[] palette, int palIndex, bool xFlip, bool yFlip)
        {
            byte[] gfx = new byte[32];
            s.Read(gfx, 0, 32);
            DrawTile(bmp, x, y, gfx, 0, palette, palIndex, xFlip, yFlip);
        }

        /// <summary>
        /// Renders a tile onto a bitmap using the fast BitmapData method.
        /// </summary>
        /// <param name="bmp">The bitmap to render to. Must be 8bpp indexed</param>
        /// <param name="x">The x coordinate of bmp to render onto</param>
        /// <param name="y">The y coordinate of bmp to render onto</param>
        /// <param name="data">The tile information in yx0pppgg format</param>
        /// <param name="tileIndex">The tile to draw</param>
        /// <param name="tiles">All of the grahpics tiles in linear format</param>
        public static void DrawTile(BitmapData bmp, int x, int y, byte data, int tileIndex, byte[][,] tiles)
        {
            byte palIndex =(byte)(0x10 * ((data / 4) & 7));
            if ((data & 1) > 0)
                tileIndex += 0x100;
            else if ((data & 2) > 0)
                tileIndex += 0x200;
            bool xFlip = (data & 0x40) > 0;
            bool yFlip = (data & 0x80) > 0;
            int xStep = 1;
            int yStep = 1;
            if (xFlip)
            {
                x += 7;
                xStep = -1;
            }
            int xOrig = x;
            if (yFlip)
            {
                y += 7;
                yStep = -1;
            }
            for (int l = 0; l <= 7; l++)
            {
                for (int m = 0; m <= 7; m++)
                {
                    Marshal.WriteByte(bmp.Scan0, y * bmp.Stride + x, (byte)(palIndex + tiles[tileIndex][l, m]));
                    x += xStep;
                }
                y += yStep;
                x = xOrig;
            }
        }

        /// <summary>
        /// Sets the palette of bmp to colors
        /// </summary>
        /// <param name="bmp">The image</param>
        /// <param name="colors">The palette</param>
        public static void FillPalette(Bitmap bmp, Color[] colors)
        {
            ColorPalette pal = bmp.Palette;
            for (int l = 0; l <= colors.Length - 1; l++)
            {
                pal.Entries[l] = colors[l];
            }
            bmp.Palette = pal;
        }

        /// <summary>
        /// Makes ever 16th color transparent
        /// </summary>
        /// <param name="plt">The colors</param>
        public static void MakePltTransparent(Color[] plt)
        {
            for (int i = 0; i < plt.Length; i += 16)
            {
                plt[i] = Color.Transparent;
            }
        }

        /// <summary>
        /// Draws the specified bitmap with the specified palette
        /// </summary>
        /// <param name="g">The graphics used to draw</param>
        /// <param name="x">The x coordinate to draw onto</param>
        /// <param name="y">The y coordinate to draw onto</param>
        /// <param name="bmp">The bitmap</param>
        /// <param name="plt">The palette</param>
        /// <param name="colorIdx">The index into the palette to load from</param>
        /// <param name="colorCount">How many colors to load</param>
        public static void DrawWithPlt(Graphics g, int x, int y, Bitmap bmp, Color[] plt, int colorIdx, int colorCount)
        {
            ColorPalette pal = bmp.Palette;
            for (int l = 0; l <= colorCount - 1; l++)
            {
                pal.Entries[l] = plt[(l + colorIdx) % plt.Length];
            }
            bmp.Palette = pal;
            g.DrawImage(bmp, x, y);
        }
    }
}
