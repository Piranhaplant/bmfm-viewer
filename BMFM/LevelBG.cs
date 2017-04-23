using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BMFM
{
    public class LevelBG
    {
        public int[] tiles;
        public byte[] arrangment;

        /// <summary>
        /// Create a new level background
        /// </summary>
        /// <param name="tiles">The BG's 32x32 tile data</param>
        /// <param name="arrangment">The larger 256x256 tile arrangement</param>
        /// <param name="oneByteTiles">Are the tiles one or two bytes each?</param>
        public LevelBG(byte[] tiles, byte[] arrangment, bool oneByteTiles = false)
        {
            this.arrangment = arrangment;

            if (oneByteTiles)
            {
                this.tiles = new int[tiles.Length];
                for (int i = 0; i < this.tiles.Length; i++)
                    this.tiles[i] = tiles[i];
            }
            else
            {
                this.tiles = new int[tiles.Length / 2];
                for (int i = 0; i < this.tiles.Length; i++)
                    this.tiles[i] = tiles[i * 2] | tiles[i * 2 + 1] << 8;
            }
        }

        /// <summary>
        /// Render the entire background
        /// </summary>
        /// <param name="g">The graphics</param>
        /// <param name="t">The tileset</param>
        public void Render(Graphics g, Tileset t)
        {
            Render(g, t, new Rectangle(0, 0, Level.width, Level.height));
        }

        /// <summary>
        /// Render a selected rectangle of the background
        /// </summary>
        /// <param name="g">The graphics</param>
        /// <param name="t">The tileset</param>
        /// <param name="r">The rectangle that is to be rendered. This is in pixels</param>
        public void Render(Graphics g, Tileset t, Rectangle r)
        {
            try
            {
                for (int i = 0; i < arrangment.Length; i++)
                {
                    byte tile = arrangment[i];
                    if (tile > 0)
                    {
                        int x = (i % 16) * 256;
                        int y = (i / 16) * 256;
                        int position = (tile - 1) * 64;
                        for (int j = 0; j < 64; j++)
                        {
                            int x2 = x + (j % 8) * 32;
                            int y2 = y + (j / 8) * 32;
                            if (x2 >= r.X - 32 && y2 >= r.Y - 32 && x2 <= r.Right && y2 <= r.Bottom)
                                g.DrawImage(t.images[tiles[position + j]], x2, y2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
