// The MIT License (MIT)
//
// Copyright (c) .NET Foundation and Contributors
//
// All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Security.Cryptography;
using System.Text;

namespace Tnc1997.AspNetCore.Identity.Bulk
{
    // See http://tools.ietf.org/html/rfc3548#section-5
    internal static class Base32
    {
        private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string GenerateBase32()
        {
            const int length = 20;
            // base32 takes 5 bytes and converts them into 8 characters, which would be (byte length / 5) * 8
            // except that it also pads ('=') for the last processed chunk if it's less than 5 bytes.
            // So in order to handle the padding we add 1 less than the chunk size to our byte length
            // which will either be removed due to integer division truncation if the length was already a multiple of 5
            // or it will increase the divided length by 1 meaning that a 1-4 byte length chunk will be 1 instead of 0
            // so the padding is now included in our string length calculation
            return string.Create(((length + 4) / 5) * 8, 0, static (buffer, _) =>
            {
                Span<byte> bytes = stackalloc byte[length];
                RandomNumberGenerator.Fill(bytes);

                var index = 0;
                for (int offset = 0; offset < bytes.Length;)
                {
                    int numCharsToOutput = GetNextGroup(bytes, ref offset, out var a, out var b, out var c, out var d, out var e, out var f, out var g, out var h);

                    buffer[index + 7] = ((numCharsToOutput >= 8) ? Base32Chars[h] : '=');
                    buffer[index + 6] = ((numCharsToOutput >= 7) ? Base32Chars[g] : '=');
                    buffer[index + 5] = ((numCharsToOutput >= 6) ? Base32Chars[f] : '=');
                    buffer[index + 4] = ((numCharsToOutput >= 5) ? Base32Chars[e] : '=');
                    buffer[index + 3] = ((numCharsToOutput >= 4) ? Base32Chars[d] : '=');
                    buffer[index + 2] = (numCharsToOutput >= 3) ? Base32Chars[c] : '=';
                    buffer[index + 1] = (numCharsToOutput >= 2) ? Base32Chars[b] : '=';
                    buffer[index] = (numCharsToOutput >= 1) ? Base32Chars[a] : '=';
                    index += 8;
                }
            });
        }

        public static string ToBase32(byte[] input)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));

            StringBuilder sb = new StringBuilder();
            for (int offset = 0; offset < input.Length;)
            {
                int numCharsToOutput = GetNextGroup(input, ref offset, out var a, out var b, out var c, out var d, out var e, out var f, out var g, out var h);

                sb.Append((numCharsToOutput >= 1) ? Base32Chars[a] : '=');
                sb.Append((numCharsToOutput >= 2) ? Base32Chars[b] : '=');
                sb.Append((numCharsToOutput >= 3) ? Base32Chars[c] : '=');
                sb.Append((numCharsToOutput >= 4) ? Base32Chars[d] : '=');
                sb.Append((numCharsToOutput >= 5) ? Base32Chars[e] : '=');
                sb.Append((numCharsToOutput >= 6) ? Base32Chars[f] : '=');
                sb.Append((numCharsToOutput >= 7) ? Base32Chars[g] : '=');
                sb.Append((numCharsToOutput >= 8) ? Base32Chars[h] : '=');
            }

            return sb.ToString();
        }

        public static byte[] FromBase32(string input)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));

            var trimmedInput = input.AsSpan().TrimEnd('=');
            if (trimmedInput.Length == 0)
            {
                return Array.Empty<byte>();
            }

            var output = new byte[trimmedInput.Length * 5 / 8];
            var bitIndex = 0;
            var inputIndex = 0;
            var outputBits = 0;
            var outputIndex = 0;
            while (outputIndex < output.Length)
            {
                var byteIndex = Base32Chars.IndexOf(char.ToUpperInvariant(trimmedInput[inputIndex]));
                if (byteIndex < 0)
                {
                    throw new FormatException();
                }

                var bits = Math.Min(5 - bitIndex, 8 - outputBits);
                output[outputIndex] <<= bits;
                output[outputIndex] |= (byte)(byteIndex >> (5 - (bitIndex + bits)));

                bitIndex += bits;
                if (bitIndex >= 5)
                {
                    inputIndex++;
                    bitIndex = 0;
                }

                outputBits += bits;
                if (outputBits >= 8)
                {
                    outputIndex++;
                    outputBits = 0;
                }
            }
            return output;
        }

        // returns the number of bytes that were output
        private static int GetNextGroup(Span<byte> input, ref int offset, out byte a, out byte b, out byte c, out byte d, out byte e, out byte f, out byte g, out byte h)
        {
            int retVal = (input.Length - offset) switch
            {
                1 => 2,
                2 => 4,
                3 => 5,
                4 => 7,
                _ => 8
            };

            var b1 = (offset < input.Length) ? input[offset++] : 0U;
            var b2 = (offset < input.Length) ? input[offset++] : 0U;
            var b3 = (offset < input.Length) ? input[offset++] : 0U;
            var b4 = (offset < input.Length) ? input[offset++] : 0U;
            var b5 = (offset < input.Length) ? input[offset++] : 0U;

            a = (byte)(b1 >> 3);
            b = (byte)(((b1 & 0x07) << 2) | (b2 >> 6));
            c = (byte)((b2 >> 1) & 0x1f);
            d = (byte)(((b2 & 0x01) << 4) | (b3 >> 4));
            e = (byte)(((b3 & 0x0f) << 1) | (b4 >> 7));
            f = (byte)((b4 >> 2) & 0x1f);
            g = (byte)(((b4 & 0x3) << 3) | (b5 >> 5));
            h = (byte)(b5 & 0x1f);

            return retVal;
        }
    }
}
