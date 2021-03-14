﻿using System;
using System.IO;
using System.Text;

namespace VanityAddrGen
{
    /// <summary>
    /// Behold the almighty HexUtils with several ridiculous optimization tricks.
    /// </summary>
    public static class HexUtils
    {
        private static readonly char[] hexMap8 =
        {
            '0', '1', '2', '3', '4',
            '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F',
        };

        private static readonly string[] hexMap16 =
        {
            "00", "01", "02", "03", "04", "05", "06", "07",
            "08", "09", "0A", "0B", "0C", "0D", "0E", "0F",
            "10", "11", "12", "13", "14", "15", "16", "17",
            "18", "19", "1A", "1B", "1C", "1D", "1E", "1F",
            "20", "21", "22", "23", "24", "25", "26", "27",
            "28", "29", "2A", "2B", "2C", "2D", "2E", "2F",
            "30", "31", "32", "33", "34", "35", "36", "37",
            "38", "39", "3A", "3B", "3C", "3D", "3E", "3F",
            "40", "41", "42", "43", "44", "45", "46", "47",
            "48", "49", "4A", "4B", "4C", "4D", "4E", "4F",
            "50", "51", "52", "53", "54", "55", "56", "57",
            "58", "59", "5A", "5B", "5C", "5D", "5E", "5F",
            "60", "61", "62", "63", "64", "65", "66", "67",
            "68", "69", "6A", "6B", "6C", "6D", "6E", "6F",
            "70", "71", "72", "73", "74", "75", "76", "77",
            "78", "79", "7A", "7B", "7C", "7D", "7E", "7F",
            "80", "81", "82", "83", "84", "85", "86", "87",
            "88", "89", "8A", "8B", "8C", "8D", "8E", "8F",
            "90", "91", "92", "93", "94", "95", "96", "97",
            "98", "99", "9A", "9B", "9C", "9D", "9E", "9F",
            "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7",
            "A8", "A9", "AA", "AB", "AC", "AD", "AE", "AF",
            "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7",
            "B8", "B9", "BA", "BB", "BC", "BD", "BE", "BF",
            "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7",
            "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF",
            "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7",
            "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF",
            "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7",
            "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF",
            "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7",
            "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF",
        };

        private static readonly string[] binMap16 =
        {
            "00000000", "00000001", "00000010", "00000011", "00000100", "00000101", "00000110", "00000111",
            "00001000", "00001001", "00001010", "00001011", "00001100", "00001101", "00001110", "00001111",
            "00010000", "00010001", "00010010", "00010011", "00010100", "00010101", "00010110", "00010111",
            "00011000", "00011001", "00011010", "00011011", "00011100", "00011101", "00011110", "00011111",
            "00100000", "00100001", "00100010", "00100011", "00100100", "00100101", "00100110", "00100111",
            "00101000", "00101001", "00101010", "00101011", "00101100", "00101101", "00101110", "00101111",
            "00110000", "00110001", "00110010", "00110011", "00110100", "00110101", "00110110", "00110111",
            "00111000", "00111001", "00111010", "00111011", "00111100", "00111101", "00111110", "00111111",
            "01000000", "01000001", "01000010", "01000011", "01000100", "01000101", "01000110", "01000111",
            "01001000", "01001001", "01001010", "01001011", "01001100", "01001101", "01001110", "01001111",
            "01010000", "01010001", "01010010", "01010011", "01010100", "01010101", "01010110", "01010111",
            "01011000", "01011001", "01011010", "01011011", "01011100", "01011101", "01011110", "01011111",
            "01100000", "01100001", "01100010", "01100011", "01100100", "01100101", "01100110", "01100111",
            "01101000", "01101001", "01101010", "01101011", "01101100", "01101101", "01101110", "01101111",
            "01110000", "01110001", "01110010", "01110011", "01110100", "01110101", "01110110", "01110111",
            "01111000", "01111001", "01111010", "01111011", "01111100", "01111101", "01111110", "01111111",
            "10000000", "10000001", "10000010", "10000011", "10000100", "10000101", "10000110", "10000111",
            "10001000", "10001001", "10001010", "10001011", "10001100", "10001101", "10001110", "10001111",
            "10010000", "10010001", "10010010", "10010011", "10010100", "10010101", "10010110", "10010111",
            "10011000", "10011001", "10011010", "10011011", "10011100", "10011101", "10011110", "10011111",
            "10100000", "10100001", "10100010", "10100011", "10100100", "10100101", "10100110", "10100111",
            "10101000", "10101001", "10101010", "10101011", "10101100", "10101101", "10101110", "10101111",
            "10110000", "10110001", "10110010", "10110011", "10110100", "10110101", "10110110", "10110111",
            "10111000", "10111001", "10111010", "10111011", "10111100", "10111101", "10111110", "10111111",
            "11000000", "11000001", "11000010", "11000011", "11000100", "11000101", "11000110", "11000111",
            "11001000", "11001001", "11001010", "11001011", "11001100", "11001101", "11001110", "11001111",
            "11010000", "11010001", "11010010", "11010011", "11010100", "11010101", "11010110", "11010111",
            "11011000", "11011001", "11011010", "11011011", "11011100", "11011101", "11011110", "11011111",
            "11100000", "11100001", "11100010", "11100011", "11100100", "11100101", "11100110", "11100111",
            "11101000", "11101001", "11101010", "11101011", "11101100", "11101101", "11101110", "11101111",
            "11110000", "11110001", "11110010", "11110011", "11110100", "11110101", "11110110", "11110111",
            "11111000", "11111001", "11111010", "11111011", "11111100", "11111101", "11111110", "11111111",
        };

        /// <summary>
        /// Returns a hex string from <paramref name="value"/>, in big-endian order.
        /// </summary>
        public static string HexFromInt(int value)
        {
            ReadOnlySpan<char> buffer = stackalloc char[8]
            {
                hexMap8[(value & 0xf0000000) >> 28],
                hexMap8[(value & 0x0f000000) >> 24],
                hexMap8[(value & 0x00f00000) >> 20],
                hexMap8[(value & 0x000f0000) >> 16],

                hexMap8[(value & 0x0000f000) >> 12],
                hexMap8[(value & 0x00000f00) >> 8],
                hexMap8[(value & 0x000000f0) >> 4],
                hexMap8[(value & 0x0000000f)],
            };
            return buffer.ToString();
        }

        /// <summary>
        /// Appends a hex string from <paramref name="value"/> to <paramref name="sb"/>, in big-endian order.
        /// </summary>
        public static void AppendHexFromInt(StringBuilder sb, int value)
        {
            sb.Append(hexMap8[(value & 0xf0000000) >> 28]);
            sb.Append(hexMap8[(value & 0x0f000000) >> 24]);
            sb.Append(hexMap8[(value & 0x00f00000) >> 20]);
            sb.Append(hexMap8[(value & 0x000f0000) >> 16]);

            sb.Append(hexMap8[(value & 0x0000f000) >> 12]);
            sb.Append(hexMap8[(value & 0x00000f00) >> 8]);
            sb.Append(hexMap8[(value & 0x000000f0) >> 4]);
            sb.Append(hexMap8[(value & 0x0000000f)]);
        }

        /// <summary>
        /// Writes a hex string from <paramref name="value"/> to <paramref name="writer"/>, in big-endian order.
        /// </summary>
        public static void AppendHexFromInt(TextWriter writer, int value)
        {
            writer.Write(hexMap8[(value & 0xf0000000) >> 28]);
            writer.Write(hexMap8[(value & 0x0f000000) >> 24]);
            writer.Write(hexMap8[(value & 0x00f00000) >> 20]);
            writer.Write(hexMap8[(value & 0x000f0000) >> 16]);

            writer.Write(hexMap8[(value & 0x0000f000) >> 12]);
            writer.Write(hexMap8[(value & 0x00000f00) >> 8]);
            writer.Write(hexMap8[(value & 0x000000f0) >> 4]);
            writer.Write(hexMap8[(value & 0x0000000f)]);
        }

        /// <summary>
        /// Returns a hex string from <paramref name="value"/>, in big-endian order.
        /// </summary>
        public static string HexFromLong(long value)
        {
            ReadOnlySpan<char> buffer = stackalloc char[16]
            {
                hexMap8[((ulong)value & 0xf0000000_00000000) >> 60],
                hexMap8[(value & 0x0f000000_00000000) >> 56],
                hexMap8[(value & 0x00f00000_00000000) >> 52],
                hexMap8[(value & 0x000f0000_00000000) >> 48],

                hexMap8[(value & 0x0000f000_00000000) >> 44],
                hexMap8[(value & 0x00000f00_00000000) >> 40],
                hexMap8[(value & 0x000000f0_00000000) >> 36],
                hexMap8[(value & 0x0000000f_00000000) >> 32],

                hexMap8[(value & 0x00000000_f0000000) >> 28],
                hexMap8[(value & 0x00000000_0f000000) >> 24],
                hexMap8[(value & 0x00000000_00f00000) >> 20],
                hexMap8[(value & 0x00000000_000f0000) >> 16],

                hexMap8[(value & 0x00000000_0000f000) >> 12],
                hexMap8[(value & 0x00000000_00000f00) >> 8],
                hexMap8[(value & 0x00000000_000000f0) >> 4],
                hexMap8[(value & 0x00000000_0000000f)],
            };
            return buffer.ToString();
        }

        /// <summary>
        /// Appends a hex string from <paramref name="value"/> to <paramref name="sb"/>, in big-endian order.
        /// </summary>
        public static void AppendHexFromLong(StringBuilder sb, long value)
        {
            sb.Append(hexMap8[((ulong)value & 0xf0000000_00000000) >> 60]);
            sb.Append(hexMap8[(value & 0x0f000000_00000000) >> 56]);
            sb.Append(hexMap8[(value & 0x00f00000_00000000) >> 52]);
            sb.Append(hexMap8[(value & 0x000f0000_00000000) >> 48]);

            sb.Append(hexMap8[(value & 0x0000f000_00000000) >> 44]);
            sb.Append(hexMap8[(value & 0x00000f00_00000000) >> 40]);
            sb.Append(hexMap8[(value & 0x000000f0_00000000) >> 36]);
            sb.Append(hexMap8[(value & 0x0000000f_00000000) >> 32]);

            sb.Append(hexMap8[(value & 0x00000000_f0000000) >> 28]);
            sb.Append(hexMap8[(value & 0x00000000_0f000000) >> 24]);
            sb.Append(hexMap8[(value & 0x00000000_00f00000) >> 20]);
            sb.Append(hexMap8[(value & 0x00000000_000f0000) >> 16]);

            sb.Append(hexMap8[(value & 0x00000000_0000f000) >> 12]);
            sb.Append(hexMap8[(value & 0x00000000_00000f00) >> 8]);
            sb.Append(hexMap8[(value & 0x00000000_000000f0) >> 4]);
            sb.Append(hexMap8[(value & 0x00000000_0000000f)]);
        }

        /// <summary>
        /// Writes a hex string from <paramref name="value"/> to <paramref name="writer"/>, in big-endian order.
        /// </summary>
        public static void AppendHexFromLong(TextWriter writer, long value)
        {
            writer.Write(hexMap8[((ulong)value & 0xf0000000_00000000) >> 60]);
            writer.Write(hexMap8[(value & 0x0f000000_00000000) >> 56]);
            writer.Write(hexMap8[(value & 0x00f00000_00000000) >> 52]);
            writer.Write(hexMap8[(value & 0x000f0000_00000000) >> 48]);

            writer.Write(hexMap8[(value & 0x0000f000_00000000) >> 44]);
            writer.Write(hexMap8[(value & 0x00000f00_00000000) >> 40]);
            writer.Write(hexMap8[(value & 0x000000f0_00000000) >> 36]);
            writer.Write(hexMap8[(value & 0x0000000f_00000000) >> 32]);

            writer.Write(hexMap8[(value & 0x00000000_f0000000) >> 28]);
            writer.Write(hexMap8[(value & 0x00000000_0f000000) >> 24]);
            writer.Write(hexMap8[(value & 0x00000000_00f00000) >> 20]);
            writer.Write(hexMap8[(value & 0x00000000_000f0000) >> 16]);

            writer.Write(hexMap8[(value & 0x00000000_0000f000) >> 12]);
            writer.Write(hexMap8[(value & 0x00000000_00000f00) >> 8]);
            writer.Write(hexMap8[(value & 0x00000000_000000f0) >> 4]);
            writer.Write(hexMap8[(value & 0x00000000_0000000f)]);
        }

        public static string HexFromByteArray(ReadOnlySpan<byte> data)
        {
            return HexFromByteArray(data, 0, data.Length);
        }

        public static string HexFromByteArray(ReadOnlySpan<byte> data, int index, int length)
        {
            if (index < 0 || index >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (length < 0 || (index + length) > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            var sb = StringBuilderPool.Acquire(length * 2);
            for (int i = index, c = index + length; i < c; ++i)
            {
                sb.Append(hexMap16[data[i]]);
            }
            return StringBuilderPool.GetStringAndRelease(sb);
        }

        public static void AppendHexFromByteArray(StringBuilder sb, ReadOnlySpan<byte> data)
        {
            AppendHexFromByteArray(sb, data, 0, data.Length);
        }

        public static void AppendHexFromByteArray(StringBuilder sb, ReadOnlySpan<byte> data, int index, int length)
        {
            if (index < 0 || index >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (length < 0 || (index + length) > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            sb.EnsureCapacity(sb.Length + length * 2);
            for (int i = index, c = index + length; i < c; ++i)
            {
                sb.Append(hexMap16[data[i]]);
            }
        }

        public static void AppendHexFromByteArray(TextWriter writer, ReadOnlySpan<byte> data)
        {
            AppendHexFromByteArray(writer, data, 0, data.Length);
        }

        public static void AppendHexFromByteArray(TextWriter writer, ReadOnlySpan<byte> data, int index, int length)
        {
            if (index < 0 || index >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (length < 0 || (index + length) > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            for (int i = index, c = index + length; i < c; ++i)
            {
                writer.Write(hexMap16[data[i]]);
            }
        }

        /// <summary>
        /// Parses <paramref name="str"/> in big-endian order and returns a 32-bit integer.
        /// </summary>
        public static int IntFromHex(string str)
        {
            return (byteInHex(str, 0) << 24)
                | (byteInHex(str, 2) << 16)
                | (byteInHex(str, 4) << 8)
                | byteInHex(str, 6);
        }

        /// <summary>
        /// Parses <paramref name="str"/> in big-endian order and returns a 64-bit integer.
        /// </summary>
        public static long LongFromHex(string str)
        {
            return (byteInHex(str, 0) << 56)
                | (byteInHex(str, 2) << 48)
                | (byteInHex(str, 4) << 40)
                | (byteInHex(str, 6) << 32)
                | (byteInHex(str, 8) << 24)
                | (byteInHex(str, 10) << 16)
                | (byteInHex(str, 12) << 8)
                | byteInHex(str, 14);
        }

        public static byte[] ByteArrayFromHex(string str)
        {
            return ByteArrayFromHex(str, 0, str.Length);
        }

        public static byte[] ByteArrayFromHex(string str, int index, int length)
        {
            if (index < 0 || index >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (length < 0 || (index + length) > str.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            byte[] arr = new byte[length / 2];
            for (int i = index, c = index + length; i < c; i += 2)
            {
                arr[(i - index) / 2] = byteInHex(str, i);
            }

            return arr;
        }

        private static byte byteInHex(string str, int index)
        {
            char high = str[index];
            char low = str[index + 1];
            return (byte)((intFromHexDigit(high) << 4) | intFromHexDigit(low));
        }

        private static int intFromHexDigit(char digit) => digit switch
        {
            '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' => digit - '0',
            'a' or 'b' or 'c' or 'd' or 'e' or 'f' => digit - 'a' + 10,
            'A' or 'B' or 'C' or 'D' or 'E' or 'F' => digit - 'A' + 10,
            _ => 0,
        };
    }
}