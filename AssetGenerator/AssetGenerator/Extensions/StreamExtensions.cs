using System.Text;

namespace AssetGenerator.Extensions
{
    /// <summary>
    /// Extensions for the Stream class
    /// </summary>
    public static class StreamExtensions
    {
        extension(Stream stream)
        {
            /// <summary>
            /// Writes the specified string to the underlying stream as a sequence of bytes encoded in UTF-8.
            /// </summary>
            /// <remarks>This method converts the input string to a byte array using UTF-8 encoding
            /// before writing it to the stream. Ensure that the stream is open and writable before calling this
            /// method.</remarks>
            /// <param name="text">The string to write to the stream. This parameter cannot be null.</param>
            public void WriteString(string text)
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                stream.Write(bytes);
            }

            /// <summary>
            /// Reads a sequence of bytes from the underlying stream and decodes them as a UTF-8 encoded string.
            /// </summary>
            /// <remarks>An exception is thrown if the stream does not contain enough data to read the
            /// specified number of bytes.</remarks>
            /// <param name="length">The number of bytes to read from the stream. Must be greater than zero.</param>
            /// <returns>A string containing the UTF-8 decoded characters read from the stream.</returns>
            public string ReadString(int length)
            {
                var bytes = new byte[length];
                stream.ReadExactly(bytes);
                return Encoding.UTF8.GetString(bytes);
            }

            /// <summary>
            /// Writes the specified 32-bit unsigned integer to the underlying stream as a sequence of bytes.
            /// </summary>
            /// <remarks>The value is written using the endianness of the system on which the code is
            /// running. Ensure that the stream is properly initialized and writable before calling this
            /// method.</remarks>
            /// <param name="value">The 32-bit unsigned integer value to write to the stream.</param>
            public void WriteUint(uint value)
            {
                var bytes = BitConverter.GetBytes(value);
                stream.Write(bytes);
            }

            /// <summary>
            /// Writes a 32-bit unsigned integer value to the underlying stream.
            /// </summary>
            /// <remarks>If the specified value is negative, it will be cast to an unsigned integer,
            /// which may result in unexpected values being written to the stream. Ensure that the value is non-negative
            /// to avoid data corruption.</remarks>
            /// <param name="value">The non-negative signed integer value to write as an unsigned integer.</param>
            public void WriteUint(int value)
            {
                stream.WriteUint((uint)value);
            }

            /// <summary>
            /// Writes the specified integer value to the underlying stream as a sequence of bytes.
            /// </summary>
            /// <remarks>The method writes the bytes in little-endian order. Ensure that the stream is
            /// open and writable before calling this method.</remarks>
            /// <param name="value">The integer value to be written to the stream. This value is converted to an unsigned integer before
            /// writing.</param>
            /// <param name="byteCount">The number of bytes to write from the byte array representation of the integer value. Must be between 1
            /// and 4, inclusive.</param>
            public void WriteNumber(int value, int byteCount)
            {
                var bytes = BitConverter.GetBytes((uint)value);
                stream.Write(bytes, 0, byteCount);
            }

            /// <summary>
            /// Reads a 32-bit unsigned integer from the current position in the underlying stream.
            /// </summary>
            /// <remarks>This method reads exactly four bytes from the stream and interprets them as
            /// an unsigned integer in the system's endianness. Ensure that the stream is positioned correctly and
            /// contains at least four bytes to avoid an exception.</remarks>
            /// <returns>A 32-bit unsigned integer representing the value read from the stream.</returns>
            public uint ReadUint()
            {
                var bytes = new byte[4];
                stream.ReadExactly(bytes);
                return BitConverter.ToUInt32(bytes);
            }

            /// <summary>
            /// Reads an integer value from the stream using the specified number of bytes.
            /// </summary>
            /// <remarks>This method reads the specified number of bytes from the underlying stream
            /// and interprets them as an unsigned integer, which is then returned as a signed integer. The stream must
            /// be positioned such that enough bytes are available to read. The bytes are read in little-endian
            /// order.</remarks>
            /// <param name="byteCount">The number of bytes to read from the stream. Must be between 1 and 4, inclusive.</param>
            /// <returns>The integer value represented by the bytes read from the stream.</returns>
            public int ReadNumber(int byteCount)
            {
                var bytes = new byte[4];
                stream.ReadExactly(bytes, 0, byteCount);
                return (int)BitConverter.ToUInt32(bytes);
            }

            /// <summary>
            /// Writes the specified 16-bit unsigned integer to the underlying stream in binary format.
            /// </summary>
            /// <remarks>The value is written using the system's endianness. Ensure that the
            /// underlying stream is properly initialized and writable before calling this method.</remarks>
            /// <param name="value">The 16-bit unsigned integer value to write to the stream.</param>
            public void WriteUshort(ushort value)
            {
                var bytes = BitConverter.GetBytes(value);
                stream.Write(bytes);
            }

            /// <summary>
            /// Reads a 16-bit unsigned integer from the current stream.
            /// </summary>
            /// <remarks>This method reads exactly two bytes from the stream and converts them to an
            /// unsigned short. Ensure that the stream is positioned correctly and contains enough data to read the
            /// value.</remarks>
            /// <returns>The 16-bit unsigned integer read from the stream.</returns>
            public ushort ReadUshort()
            {
                var bytes = new byte[2];
                stream.ReadExactly(bytes);
                return BitConverter.ToUInt16(bytes);
            }
        }
    }
}