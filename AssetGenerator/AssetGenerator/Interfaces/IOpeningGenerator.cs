namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Generates opening information
    /// </summary>
    public interface IOpeningGenerator
    {
        /// <summary>
        /// Generate the data
        /// </summary>
        /// <returns></returns>
        Task Generate();
    }
}