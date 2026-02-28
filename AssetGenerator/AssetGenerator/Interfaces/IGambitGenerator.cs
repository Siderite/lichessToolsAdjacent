namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Generates gambit data, including the gambit information and ranking.
    /// </summary>
    public interface IGambitGenerator
    {
        /// <summary>
        /// Generate the gabmit data for both white and black players from PGN files and save the results to a JSON file.
        /// </summary>
        /// <returns></returns>
        Task Generate();

        /// <summary>
        /// Ranks the gambits for both white and black players by processing the generated gambit data 
        /// and outputs the results to a CSV file.
        /// </summary>
        /// <returns></returns>
        Task RankGambits();
    }
}