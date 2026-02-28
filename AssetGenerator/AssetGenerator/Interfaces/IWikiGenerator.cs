namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Generate relationship data between Wikibooks URLs and chess positions
    /// </summary>
    public interface IWikiGenerator
    {
        /// <summary>
        /// Generate the data
        /// </summary>
        /// <returns></returns>
        Task Generate();
    }
}