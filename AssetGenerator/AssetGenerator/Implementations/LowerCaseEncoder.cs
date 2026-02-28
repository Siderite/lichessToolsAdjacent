using AssetGenerator.Interfaces;
using System.Text;

namespace AssetGenerator.Implementations
{
    /// <summary>
    /// Encodes a puzzle ID into a lowercase string format, which can be used for consistent file naming or URL generation.
    /// </summary>
    public class LowerCaseEncoder : ILowerCaseEncoder
    {

        /// <summary>
        /// Encodes the given puzzle ID into a lowercase string format.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string Encode(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";

            var sb = new StringBuilder(id.Length);
            ulong bits = 0;

            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                sb.Append(char.ToLowerInvariant(c));

                if (char.IsUpper(c))
                {
                    // Leftmost char = highest bit (bit position = length-1-i)
                    int bitPosition = id.Length - 1 - i;
                    bits |= (1UL << bitPosition);
                }
            }
            sb.Append(bits);
            return sb.ToString();
        }

    }
}
