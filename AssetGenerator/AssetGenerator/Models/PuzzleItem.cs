namespace AssetGenerator.Models
{
    /// <summary>
    /// Represents a chess puzzle item
    /// </summary>
    public class PuzzleItem
    {
        /// <summary>
        /// Puzzle ID
        /// </summary>
        public string PuzzleId { get; set; }

        /// <summary>
        /// The FENs of the positions reached by the puzzle
        /// </summary>
        public List<string> Fens { get; set; }

        /// <summary>
        /// The moves in the puzzle
        /// </summary>
        public string[] Moves { get; set; }

        /// <summary>
        /// Puzzle rating
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Puzzle rating deviation
        /// </summary>
        public int RatingDeviation { get; set; }

        /// <summary>
        /// Popularity of the puzzle
        /// </summary>
        public int Popularity { get; set; }

        /// <summary>
        /// Number of times the puzzle has been played
        /// </summary>
        public int NbPlays { get; set; }

        /// <summary>
        /// Themes associated with the puzzle, such as "mate in 2", "fork", "pin", etc.
        /// </summary>
        public string[] Themes { get; set; }

        /// <summary>
        /// Game URL where the puzzle was extracted from, if applicable.
        /// </summary>
        public string GameUrl { get; set; }

        /// <summary>
        /// Opening tags associated with the puzzle, such as "Sicilian Defense", "Ruy Lopez", etc.
        /// </summary>
        public string[] OpeningTags { get; set; }

        /// <summary>
        /// PGNs associated with the puzzle
        /// </summary>
        public List<string> Pgns { get; set; }
    }
}
