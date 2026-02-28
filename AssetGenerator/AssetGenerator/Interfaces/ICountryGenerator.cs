namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Generates country data
    /// </summary>
    public interface ICountryGenerator
    {
        /// <summary>
        /// Generate the data
        /// </summary>
        /// <returns></returns>
        Task Generate();
    }
}