using AssetGenerator.Interfaces;
using Chess;
using System.Reflection;

namespace AssetGenerator.Implementations.GeraChessWrap
{
    /// <summary>
    /// Implements the IChessBoard interface by wrapping an instance of the ChessBoard class from the GeraChess library.
    /// This wrapper provides methods to move pieces using SAN and UCI notations, normalize SAN moves, 
    /// navigate to previous moves, and convert the board state to FEN and PGN formats. 
    /// It also handles resetting the end game state before making a move to ensure proper game progression.
    /// </summary>
    /// <param name="board"></param>
    public class ChessBoardWrapper(ChessBoard board) : IChessBoard
    {
        private static PropertyInfo endGameProp;

        /// <summary>
        /// Make a SAN move
        /// </summary>
        /// <param name="san"></param>
        public void MoveSan(string san)
        {
            var move = board.ParseFromSan(san);
            endGameProp ??= board.GetType().GetProperty(nameof(ChessBoard.EndGame));
            endGameProp.SetValue(board, null);
            board.Move(move);
        }

        /// <summary>
        /// Make a UCI move
        /// </summary>
        /// <param name="uci"></param>
        public void MoveUci(string uci)
        {
            var moveStr = "{" + uci.Substring(0, 2) + " - " + uci.Substring(2, 2);
            if (uci.Length > 4 && uci[4] != 'q')
            {
                moveStr += " - =" + uci.Substring(4, 1).ToLower();
            }
            moveStr += "}";
            var move = new Move(moveStr);
            endGameProp ??= board.GetType().GetProperty(nameof(ChessBoard.EndGame));
            endGameProp.SetValue(board, null);
            board.Move(move);
        }

        /// <summary>
        /// Retrieve the normalized SAN and corresponding UCI move for a given SAN move string. 
        /// </summary>
        /// <param name="san"></param>
        /// <returns></returns>
        public (string san, string uci) NormalizeSan(string san)
        {
            var move = board.ParseFromSan(san);
            san = board.ParseToSan(move);
            var uci = $"{move.OriginalPosition}{move.NewPosition}";
            if (move.IsPromotion) uci+= move.Promotion.Type.AsChar;
            return (san, uci);
        }

        /// <summary>
        /// Go to previous move if possible. This method checks if the current move index is greater than or equal to zero,
        /// </summary>
        public void Previous()
        {
            if (board.MoveIndex >= 0)
            {
                board.Previous();
            }
        }

        /// <summary>
        /// Return the FEN
        /// </summary>
        /// <returns></returns>
        public string ToFen()
        {
            return board.ToFen();
        }

        /// <summary>
        /// Return the PGN text
        /// </summary>
        /// <returns></returns>
        public string ToPgn()
        {
            return board.ToPgn();
        }
    }
}
