namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Generates flair data
    /// </summary>
    public interface IFlairGenerator
    {
        /// <summary>
        /// Generate the flair data
        /// </summary>
        /// <returns></returns>
        Task Generate();
    }
}