namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Interface for managing chess board states and operations
    /// </summary>
    public interface IChessManager
    {
        /// <summary>
        /// Load a board from a FEN string
        /// </summary>
        /// <param name="fen"></param>
        /// <returns></returns>
        IChessBoard LoadFromFen(string fen);

        /// <summary>
        /// Load a board from a PGN string
        /// </summary>
        /// <param name="pgn"></param>
        /// <returns></returns>
        IChessBoard LoadFromPgn(string pgn);

        /// <summary>
        /// Create a new board
        /// </summary>
        /// <returns></returns>
        IChessBoard NewBoard();
    }
}