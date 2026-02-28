namespace AssetGenerator.Models
{
    /// <summary>
    /// Info about an n-gram
    /// </summary>
    public class NgramInfo
    {
        /// <summary>
        /// The index of the n-gram in the list of n-grams. This is used to reference the n-gram in the move tree.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The count
        /// </summary>
        public int IndexCount { get; set; }

        /// <summary>
        /// Position in the file
        /// </summary>
        public long Position { get; set; }
    }
}
