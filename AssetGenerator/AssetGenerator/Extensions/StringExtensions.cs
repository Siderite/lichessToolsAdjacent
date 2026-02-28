using System.IO.Compression;
using System.Text;

namespace AssetGenerator.Extensions
{
    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringExtensions
    {
        extension(string text)
        {
            /// <summary>
            /// Compress a string into another string
            /// </summary>
            /// <returns></returns>
            public string Compress()
            {
                if (string.IsNullOrEmpty(text)) return text;

                byte[] bytes = Encoding.UTF8.GetBytes(text);

                using var input = new MemoryStream(bytes);
                using var output = new MemoryStream();
                using (var brotli = new BrotliStream(output, CompressionLevel.Optimal))
                {
                    input.CopyTo(brotli);
                }

                return Convert.ToBase64String(output.ToArray());
            }

            /// <summary>
            /// Decompress a string into the original string
            /// </summary>
            /// <returns></returns>
            public string Decompress()
            {
                if (string.IsNullOrEmpty(text)) return text;

                byte[] bytes = Convert.FromBase64String(text);

                using var input = new MemoryStream(bytes);
                using var output = new MemoryStream();
                using (var brotli = new BrotliStream(input, CompressionMode.Decompress))
                {
                    brotli.CopyTo(output);
                }

                return Encoding.UTF8.GetString(output.ToArray());
            }
        }
    }
}