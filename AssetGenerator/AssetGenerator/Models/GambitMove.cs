namespace AssetGenerator.Models
{
    /// <summary>
    /// Represents a chess move in both UCI (Universal Chess Interface) and SAN (Standard Algebraic Notation) formats,
    /// along with the number of gambits that follow this move in the game sequence. 
    /// This class is used to encapsulate the details of a move
    /// </summary>
    public class GambitMove
    {
        /// <summary>
        /// UCI
        /// </summary>
        public string uci { get; set; }

        /// <summary>
        /// SAN
        /// </summary>
        public string san { get; set; }

        /// <summary>
        /// Number of gambits that follow this move in the game sequence.
        /// </summary>
        public int nr { get; set; }
    }
}
