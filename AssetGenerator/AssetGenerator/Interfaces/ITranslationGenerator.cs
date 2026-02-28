namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Generates translation data
    /// </summary>
    public interface ITranslationGenerator
    {
        /// <summary>
        /// Generate the data
        /// </summary>
        /// <returns></returns>
        Task Generate();
    }
}