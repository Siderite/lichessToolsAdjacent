using AssetGenerator.Interfaces;
using Chess;

namespace AssetGenerator.Implementations.GeraChessWrap
{
    /// <summary>
    /// Implementation of IChessManager that uses the GeraChess library to manage chess board states and moves.
    /// This class provides methods to load chess boards from FEN and PGN formats, as well as to create new 
    /// chess boards in the standard starting position. It serves as a bridge between the GeraChess library 
    /// and the IChessManager interface used in the asset generation process, allowing for seamless integration 
    /// of chess functionalities within the application.
    /// </summary>
    public class ChessManager : IChessManager
    {
        /// <summary>
        /// Gets the Forsyth-Edwards Notation (FEN) string that represents the standard starting position for a chess
        /// game.
        /// </summary>
        /// <remarks>The returned FEN string specifies the initial arrangement of all pieces, the active
        /// color, castling rights, en passant target square, halfmove clock, and fullmove number according to the FEN
        /// standard.</remarks>
        public static string StartingPositionFen => "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        /// <summary>
        /// Loads a chess board position from a specified FEN (Forsyth–Edwards Notation) string.
        /// </summary>
        /// <remarks>Throws an exception if the provided FEN string is invalid or cannot be
        /// parsed.</remarks>
        /// <param name="fen">The FEN string that represents the chess board state to load. Must be a valid FEN format.</param>
        /// <returns>An instance of <see cref="IChessBoard"/> that represents the chess board loaded from the provided FEN
        /// string.</returns>
        public IChessBoard LoadFromFen(string fen)
        {
            var board = ChessBoard.LoadFromFen(fen);
            return new ChessBoardWrapper(board);
        }

        /// <summary>
        /// Loads a chess board position from a specified PGN (Portable Game Notation) string.
        /// </summary>
        /// <param name="pgn"></param>
        /// <returns></returns>
        public IChessBoard LoadFromPgn(string pgn)
        {
            var board = ChessBoard.LoadFromPgn(pgn);
            return new ChessBoardWrapper(board);
        }

        /// <summary>
        /// Creates a new board
        /// </summary>
        /// <returns></returns>
        public IChessBoard NewBoard()
        {
            return LoadFromFen(StartingPositionFen);
        }
    }
}
