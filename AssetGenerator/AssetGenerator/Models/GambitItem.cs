using Newtonsoft.Json;

namespace AssetGenerator.Models
{
    /// <summary>
    /// Data object representing a chess gambit, including its total occurrences, 
    /// list of moves, and various statistics such as wins, draws, losses, and average ELO. 
    /// The class also includes properties for the gambit's name, color, PGN representation, and FEN position,
    /// which are ignored during JSON serialization. The ToString method provides a string representation of 
    /// the gambit for easy debugging and logging purposes.
    /// </summary>
    public class GambitItem
    {
        /// <summary>
        /// Total number of games played with this gambit, calculated as the sum of wins, draws, and losses.
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// Moves in the gambit, represented as a list of GambitMove objects.
        /// Each move includes details such as the SAN notation and the resulting FEN position.
        /// </summary>
        public List<GambitMove> moves { get; set; } = [];

        /// <summary>
        /// Name of the gambit, which is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public string Name { get; set; }

        /// <summary>
        /// Color of the player using the gambit (e.g., "white" or "black"), which is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public string Color { get; set; }

        /// <summary>
        /// The PGN (Portable Game Notation) representation of the gambit, which is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public string Pgn { get; set; }

        /// <summary>
        /// The FEN (Forsyth-Edwards Notation) string representing the position 
        /// starting the gambit moves, which is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public string Fen { get; set; }

        /// <summary>
        /// Total wins achieved with this gambit, which is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public long Wins { get; set; }

        /// <summary>
        /// Total draws achieved with this gambit, which is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public long Draws { get; set; }

        /// <summary>
        /// Total losses incurred with this gambit, which is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public long Losses { get; set; }

        /// <summary>
        /// Total number of games played with this gambit, calculated as the sum of wins, draws, and losses. 
        /// This property is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public long Total
        {
            get
            {
                return this.Wins + this.Draws + this.Losses;
            }
        }

        /// <summary>
        /// Win ratio for this gambit, calculated as the number of wins divided by the total number of games played.
        /// </summary>
        [JsonIgnore]
        public double WinRatio
        {
            get
            {
                if (this.Total == 0) return 0;
                return (double)this.Wins / this.Total;
            }
        }

        /// <summary>
        /// Average ELO rating of players using this gambit, which is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public int AverageElo { get; set; }

        /// <summary>
        /// Represents the gambit as a string, showing the total number of games and the SAN notation of the moves.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[" + total + "] " + string.Join(",", moves.Select(m => m.san));
        }
    }
}
