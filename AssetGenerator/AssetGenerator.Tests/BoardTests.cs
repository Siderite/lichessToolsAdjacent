using AssetGenerator.Implementations.GeraChessWrap;
using Chess;

namespace AssetGenerator.Tests
{
    [TestClass]
    public sealed class BoardTests
    {
        [TestMethod]
        public void EnPassantWorks_ChessBoard()
        {
            var board = ChessBoard.LoadFromFen("rnbqkbnr/1ppppppp/p7/4P3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2");
            board.Move(new Move(new Position(3, 6), new Position(3, 4)));
            var fen = board.ToFen();
            Assert.AreEqual("rnbqkbnr/1pp1pppp/p7/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3", fen);
        }

        [TestMethod]
        public void EnPassantWorks_Wrapper()
        {
            var chessManager = new ChessManager();
            var board = chessManager.LoadFromFen("rnbqkbnr/1ppppppp/p7/4P3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2");
            board.MoveSan("d5");
            var fen = board.ToFen();
            Assert.AreEqual("rnbqkbnr/1pp1pppp/p7/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3", fen);
        }
    }
}