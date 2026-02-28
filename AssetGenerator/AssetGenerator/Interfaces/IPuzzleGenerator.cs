namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Generates puzzle data
    /// </summary>
    public interface IPuzzleGenerator
    {
        /// <summary>
        /// Generates a NIF index file that can be used by LiChess Tools 
        /// to find the puzzles reaching a specific position
        /// </summary>
        /// <returns></returns>
        Task GenerateNif();

        /// <summary>
        /// Redownload the puzzle file from the LiChess server if it's newer than the local one.
        /// </summary>
        /// <returns></returns>
        Task<bool> RefreshPuzzleFile();
    }
}