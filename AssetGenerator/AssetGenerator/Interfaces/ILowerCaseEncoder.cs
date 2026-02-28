namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Encodes a puzzle ID into a lowercase string format, which can be used for consistent file naming or URL generation.
    /// </summary>
    public interface ILowerCaseEncoder
    {
        /// <summary>
        /// Encodes the given puzzle ID into a lowercase string format.
        /// </summary>
        /// <param name="puzzleId"></param>
        /// <returns></returns>
        string Encode(string puzzleId);
    }
}
