using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BMFM
{
    public class Level
    {
        public static int width = 4096;
        public static int height = 2048;

        public Tileset tileset;
        public LevelBG BG1;
        public LevelBG BG2;

        /// <summary>
        /// Load a level from a stream. The stream should be positioned at the beginning of the level data
        /// </summary>
        /// <param name="s">The stream</param>
        public Level(Stream s)
        {
            // Saved because tileset loading won't return the stream to its original position
            long pos = s.Position;
            this.tileset = new Tileset(s);
            s.Seek(pos, SeekOrigin.Begin);
            // Go to the pointer specified by the first two bytes (The level background data)
            s.Seek(SNES.toPC(s.ReadByte() | s.ReadByte() << 8, 0x84), SeekOrigin.Begin);

            // Move past the first two bytes, which specify nothing as far as I know
            s.Seek(0x2, SeekOrigin.Current);
            // BG 1 is interleaved across two blocks of compressed data
            byte[] BG1_1 = BMFMCompress.Decompress(s);
            byte[] BG1_3 = BMFMCompress.Decompress(s);
            BMFMCompress.Interleave(BG1_1, BG1_3);

            // Skip the tileset pointer and go to the BG 2 stuff
            s.Seek(6, SeekOrigin.Current);
            // BG 2 is not interleaved (becuase it uses 1 byte instead of 2 for tile indexes)
            byte[] BG2_1 = BMFMCompress.Decompress(s);

            // Get the large BG tile arrangement data
            byte[] BG1_2 = BMFMCompress.Decompress(s);
            byte[] BG2_2 = BMFMCompress.Decompress(s);

            // Create the backgrounds
            BG1 = new LevelBG(BG1_1, BG1_2);
            BG2 = new LevelBG(BG2_1, BG2_2, true);
        }
    }
}
