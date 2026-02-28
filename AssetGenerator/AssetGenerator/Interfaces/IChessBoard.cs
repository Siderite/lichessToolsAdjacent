namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Interface abstracting a chess board and its various operations.
    /// </summary>
    public interface IChessBoard
    {
        /// <summary>
        /// Move in UCI format (e.g. e2e4, e7e8q)
        /// </summary>
        /// <param name="uci"></param>
        void MoveUci(string uci);

        /// <summary>
        /// Move in SAN format (e.g. e4, Nf3, O-O)
        /// </summary>
        /// <param name="san"></param>
        void MoveSan(string san);

        /// <summary>
        /// Go to previous position
        /// </summary>
        void Previous();

        /// <summary>
        /// Retrieve the FEN of the current position
        /// </summary>
        /// <returns></returns>
        string ToFen();

        /// <summary>
        /// Retrieve the PGN of the current game
        /// </summary>
        /// <returns></returns>
        string ToPgn();

        /// <summary>
        /// Normalize a SAN move string to its canonical form and retrieve the corresponding UCI move string.
        /// </summary>
        /// <param name="san"></param>
        /// <returns></returns>
        (string san, string uci) NormalizeSan(string san);
    }
}