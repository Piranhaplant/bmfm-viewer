using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BMFM
{
    class BMFMCompress
    {
        private static Stream s;
        private static byte[] dictionary;
        private static int dictPos;
        private static List<byte> result;
        private static bool flag1;
        private static bool flag2;

        /// <summary>
        /// Decompress data from a stream. The stream must be positioned at a pointer to the data
        /// </summary>
        /// <param name="strm">The stream</param>
        /// <returns>The decompressed data</returns>
        public static byte[] Decompress(Stream strm)
        {
            // Load the pointer
            int graphicsPointer = strm.ReadByte() | strm.ReadByte() << 8;
            // And the bank
            int graphicsBank = strm.ReadByte();
            // We are going to want to return here
            long position = strm.Position;
            // The second most significant bit of the pointer indicates flag1
            bool flag1 = (graphicsBank & 0x40) > 0;
            // Remove the upper two bits to get the actually pointer bank
            graphicsBank = graphicsBank & 0xBF;
            // Convert to a usable PC address
            graphicsPointer = (graphicsBank - 0x80) * 0x8000 + graphicsPointer - 0x8000;
            strm.Seek(graphicsPointer, SeekOrigin.Begin);
            // Decompress it
            byte[] data = Decompress(strm, flag1);
            strm.Seek(position, SeekOrigin.Begin);
            return data;
        }

        /// <summary>
        /// Decompresses data from a stream. The stream must be positioned at the data itself
        /// </summary>
        /// <param name="strm">The stream</param>
        /// <param name="f1">flag1. This is the flag that indicated interleaved data</param>
        /// <returns>The decompressed data</returns>
        public static byte[] Decompress(Stream strm, bool f1)
        {
            s = strm;
            dictionary = new byte[0x400];
            dictPos = 0;
            result = new List<byte>();
            flag1 = f1;

            // The first two bytes are the length of compressed data
            int length = s.ReadByte() | s.ReadByte() << 8;
            // flag2 is specified in the high bit of the length
            flag2 = (length & 0x8000) > 0;
            // Remove that high bit from the length
            length = length & 0x7FFF;
            // Calculate the ending of the compressed data
            int end = (int)s.Position + length;

            while (s.Position < end)
            {
                // Load the byte that indicates what to do
                byte cur = (byte)s.ReadByte();
                // If the high bit is clear, do a copy from dictionary
                if (cur < 0x80) {
                    int numBytes = (cur >> 2) + 2;
                    int srcPos = ((s.ReadByte() | cur << 8) - 0x3DF) & 0x3FF;
                    copy(numBytes, srcPos);
                } else if (cur < 0xA0) {
                    // If the high bits are 100, load some bytes from the data
                    int numBytes = cur & 0x1F;
                    load(numBytes, false);
                } else if (cur < 0xC0) {
                    // If the high bits are 101, load some bytes alternating with zero bytes
                    int numBytes = (cur & 0x1F) + 2;
                    load(numBytes, true);
                } else if (cur < 0xE0) {
                    // If the high bits are 110, fill with a copy of a byte
                    int numBytes = (cur & 0x1F) + 2;
                    byte fillByte = (byte)s.ReadByte();
                    fill(numBytes, fillByte);
                } else if (cur < 0xFF) {
                    // If the high bits are 111, but the byte is not 0xff, then fill with some zero bytes
                    int numBytes = (cur & 0x1F) + 2;
                    fill(numBytes, 0);
                } else {
                    // If the byte is 0xff then fill with a lot of zero bytes
                    int numBytes = (byte)s.ReadByte() + 2;
                    fill(numBytes, 0);
                }
            }
            // Move the remaining data in the dictionary to the result
            moveDictionary();
            return result.ToArray();
        }

        private static void copy(int numBytes, int srcPos)
        {
            for (int i = 0; i < numBytes; i++)
                writeByte(dictionary[srcPos++ % 0x400]);
        }

        private static void load(int numBytes, bool zeros)
        {
            for (int i = 0; i < numBytes; i++) {
                if (zeros)
                    writeByte((byte)0);
                writeByte((byte)s.ReadByte());
            }
        }

        private static void fill(int numBytes, byte fillByte)
        {
            for (int i = 0; i < numBytes; i++)
                writeByte(fillByte);
        }

        private static void writeByte(byte b)
        {
            // Write a byte into the dictionary
            dictionary[dictPos++] = b;
            // When the dictionary is full, move it into the result
            if (dictPos == 0x400)
                moveDictionary();
        }

        private static void moveDictionary()
        {
            // flag1 specifies that the data is interleaved.
            // Here we alternate it with zero bytes, but another decompressed file will fill these bytes
            if (flag1)
            {
                for (int i = 0; i < dictPos; i++)
                {
                    result.Add(dictionary[i]);
                    result.Add((byte)0);
                }
            }
            else if (flag2)
            {
                // flag2 indicates some weird copy order for the bytes
                for (int i = 0; i < dictPos; i += 16)
                {
                    result.Add(dictionary[i]);
                    result.Add(dictionary[i + 8]);
                    result.Add(dictionary[i + 1]);
                    result.Add(dictionary[i + 9]);
                    result.Add(dictionary[i + 2]);
                    result.Add(dictionary[i + 10]);
                    result.Add(dictionary[i + 3]);
                    result.Add(dictionary[i + 11]);
                    result.Add(dictionary[i + 4]);
                    result.Add(dictionary[i + 12]);
                    result.Add(dictionary[i + 5]);
                    result.Add(dictionary[i + 13]);
                    result.Add(dictionary[i + 6]);
                    result.Add(dictionary[i + 14]);
                    result.Add(dictionary[i + 7]);
                    result.Add(dictionary[i + 15]);
                }
            }
            else
            {
                // Otherwise do a regular copy
                for (int i = 0; i < dictPos; i++)
                    result.Add(dictionary[i]);
            }
            dictPos = 0;
        }

        /// <summary>
        /// data2 is copied into data1. Both must have been decompressed with "flag1" set.
        /// </summary>
        /// <param name="data1">This array is the destination/first source</param>
        /// <param name="data2">This is the second source</param>
        public static void Interleave(byte[] data1, byte[] data2)
        {
            // Avoid going past the end of either array
            int len = Math.Min(data1.Length, data2.Length);
            // Copy
            for (int i = 0; i < len; i += 2)
                data1[i + 1] = data2[i];
        }
    }
}
