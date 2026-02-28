namespace AssetGenerator.Models
{
    /// <summary>
    /// Used to deserialize the data from the explorer API, 
    /// which contains the number of games played with white and black pieces, 
    /// the average rating of those games, and the moves that can be played from that position.
    /// </summary>
    public class ExplorerItem
    {
        /// <summary>
        /// Number of white wins
        /// </summary>
        public long white { get; set; }

        /// <summary>
        /// Number of draws
        /// </summary>
        public long draws { get; set; }

        /// <summary>
        /// Number of black wins
        /// </summary>
        public long black { get; set; }

        /// <summary>
        /// The average rating of players who played the move
        /// </summary>
        public int averageRating { get; set; }

        /// <summary>
        /// Moves that have been played from this position
        /// </summary>
        public ExplorerItem[] moves { get; set; }
    }
}
