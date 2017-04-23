using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace BMFM
{
    public class Palette
    {
        public Color[] colors;

        /// <summary>
        /// Loads a new palette from a stream. Must be positioned at the palette data
        /// </summary>
        /// <param name="s">The stream</param>
        public Palette(Stream s)
        {
            // The palette needs space for 0x200 bytes or 0x100 colors
            byte[] data = new byte[0x200];
            // First two bytes indicate a pointer to the actual colors
            int pointer = s.ReadByte() | s.ReadByte() << 8;
            // When this is zero, all the colors have been loaded
            while (pointer != 0)
            {
                // Next byte is where this palette goes in the overall palette
                int index = s.ReadByte() * 0x10;
                // It will be necessary to return here after each loop
                long position = s.Position;
                // Go to the color data
                s.Seek(SNES.toPC(pointer, 0x8A), SeekOrigin.Begin);
                // Two bytes for how many total bytes will be in the output (not how many are in the input)
                int numBytes = s.ReadByte() | s.ReadByte() << 8;
                while (numBytes > 0)
                {
                    // Two bytes are automatically skipped (remain 0) every 0x20 bytes, but not every 0x200 bytes
                    if ((index & 0x1FF) != 0 && (index & 0x1F) == 0)
                        index += 2;
                    // Load the next color or fill
                    int next = s.ReadByte() | s.ReadByte() << 8;
                    numBytes -= 2;
                    // If the high bit is set, then it indicates a fill with 0s
                    if ((next & 0x8000) > 0)
                    {
                        // The number of zero bytes is the low byte * 2
                        int numZeros = (next & 0xFF) * 2;
                        // Make sure not to exceed the number of bytes
                        if (numZeros > numBytes)
                            numZeros = numBytes;
                        // Copy them
                        for (int i = 0; i < numZeros; i++)
                            data[index++] = 0;
                        numBytes -= numZeros;
                    }
                    else
                    {
                        // Otherwise, simply copy those two bytes into the palette
                        data[index++] = (byte)(next & 0xFF);
                        data[index++] = (byte)((next & 0xFF00) >> 8);
                    }
                }
                // Return to the next color block
                s.Seek(position, SeekOrigin.Begin);
                // Load the pointer for the next check
                pointer = s.ReadByte() | s.ReadByte() << 8;
            }

            // Convert all of this into colors that can actually be used
            colors = new Color[0x100];
            for (int i = 0; i < data.Length; i += 2)
                colors[i / 2] = SNES.toRGB(data[i], data[i + 1]);
        }
    }
}
