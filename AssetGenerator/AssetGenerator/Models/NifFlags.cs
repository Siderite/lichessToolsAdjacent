namespace AssetGenerator.Models
{
    /// <summary>
    /// The flags representing the options for the NIF (N-gram Index Format) file format.
    /// </summary>
    [Flags]
    public enum NifFlags
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// ID counters
        /// </summary>
        IdCounters = 1,

        /// <summary>
        /// N-gram counters
        /// </summary>
        NgramCounters = 2,


        /// <summary>
        /// N-grams are compressed
        /// </summary>
        NgramCompression = 4
    }
}
