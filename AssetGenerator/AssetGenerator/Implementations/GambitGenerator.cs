using AssetGenerator.Interfaces;
using AssetGenerator.Models;
using Microsoft.Extensions.Logging;

//using Chess;
using Neondactyl.PgnParser.Net;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace AssetGenerator
{
    /// <summary>
    /// Provides functionality to generate and rank chess gambits based on PGN files for both white and black pieces.
    /// </summary>
    /// <remarks>This class implements the IGambitGenerator interface and offers methods to extract gambit
    /// data from PGN files, serialize the results to JSON and CSV formats, and rank gambits based on game outcomes. It
    /// handles logging and file operations to support analysis workflows.</remarks>
    /// <param name="logger">The logger used to record informational messages and errors during gambit generation and ranking operations.</param>
    /// <param name="chessManager">The chess manager responsible for creating and managing chess board states during gambit analysis.</param>
    public partial class GambitGenerator(
        ILogger<GambitGenerator> logger,
        IChessManager chessManager
        )
        :IGambitGenerator
    {
        [GeneratedRegex("\\[.*?\\]")]
        private partial Regex regPgnTag();

        /// <summary>
        /// Generates gambit data for both white and black players from predefined PGN files and writes the results to a
        /// JSON file.
        /// </summary>
        /// <remarks>This method reads gambit information from 'Data/GambitsForWhite.pgn' and
        /// 'Data/GambitsForBlack.pgn', serializes the combined data into JSON format, and saves it as 'gambits.json'.
        /// Ensure that the required PGN files exist and are accessible before calling this method. The method logs the
        /// number of gambits generated for each player upon completion.</remarks>
        /// <returns></returns>
        public async Task Generate()
        {
            logger.LogInformation("Generating gambits...");
            var gambits = new
            {
                white = getGambits("Data/GambitsForWhite.pgn"),
                black = getGambits("Data/GambitsForBlack.pgn")
            };
            var result = JsonConvert.SerializeObject(gambits, Formatting.Indented);
            Directory.CreateDirectory("Output");
            File.WriteAllText("Output/gambits.json", result);
            logger.LogInformation("Gambits generated successfully. White: {wcount} Black: {bcount}", gambits.white.Count, gambits.black.Count);
        }

        /// <summary>
        /// Asynchronously ranks chess gambits for both white and black players by processing data from PGN files and
        /// outputs the results to a CSV file.
        /// </summary>
        /// <remarks>This method retrieves gambit data from predefined PGN files, aggregates the results,
        /// and logs the ranking process. The output CSV file contains detailed statistics for each gambit, including
        /// total games played, wins, draws, losses, win percentage, and average Elo rating. The method is intended for
        /// batch processing and writes the results to a file named 'gambits.csv' in the application's working
        /// directory.</remarks>
        /// <returns></returns>
        public async Task RankGambits()
        {
            logger.LogInformation("Ranking gambits...");
            var gambits = await rankGambits("Data/GambitsForWhite.pgn", "white");
            gambits.AddRange(await rankGambits("Data/GambitsForBlack.pgn", "black"));
            var sb = new StringBuilder();
            sb.AppendLine("Name,Color,Pgn,Fen,Total,Wins,Draws,Losses,Win%,AvgElo");
            foreach (var gambit in gambits)
            {
                sb.AppendLine("\"" + string.Join("\",\"", new[]
                {
                    gambit.Name,
                    gambit.Color,
                    gambit.Pgn,
                    gambit.Fen,
                    gambit.Total.ToString(),
                    gambit.Wins.ToString(),
                    gambit.Draws.ToString(),
                    gambit.Losses.ToString(),
                    gambit.WinRatio.ToString("P2"),
                    gambit.AverageElo.ToString()
                }) + "\"");
            }
            Directory.CreateDirectory("Output");
            File.WriteAllText("Output/gambits.csv", sb.ToString());
            logger.LogInformation("{count} gambits ranked successfully.", gambits.Count);
        }

        private Dictionary<string, GambitItem> getGambits(string path)
        {
            logger.LogInformation(" gambits from {path}...", path);
            var gambits = new ConcurrentDictionary<string, GambitItem>();
            var parser = new PgnParser();
            parser.ParseFromFile(path);
            var pgns = parser.GetGames();
            foreach (var pgn in pgns)
            {
                var moves = pgn.GetMovesArray();
                var board = chessManager.NewBoard();
                for (int i = 0; i < moves.Length; i++)
                {
                    var fen = board.ToFen().Split(" ");
                    var key = string.Join("", fen.Take(2).Select(s => s.Replace("/", "")));
                    var item = gambits.GetOrAdd(key, new GambitItem());
                    item.total++;
                    var (san,uci) = board.NormalizeSan(moves[i]);
                    var nextMove = item.moves.FirstOrDefault(m => m.san == san);
                    if (nextMove == null)
                    {
                        nextMove = new GambitMove
                        {
                            san = san,
                            uci = uci,
                            nr = 0
                        };
                        item.moves.Add(nextMove);
                    }
                    nextMove.nr++;
                    board.MoveUci(uci);
                }
            }
            return gambits.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        }

        private async Task<List<GambitItem>> rankGambits(string path, string color)
        {
            var gambits = new List<GambitItem>();
            var parser = new PgnParser();
            parser.ParseFromFile(path);
            var pgns = parser.GetGames();
            foreach (var pgn in pgns)
            {
                var moves = pgn.GetMovesArray();
                var board = chessManager.NewBoard();
                string[] fen;
                for (int i = 0; i < moves.Length; i++)
                {
                    board.MoveSan(moves[i]);
                }
                fen = board.ToFen().Split(" ");
                var item = new GambitItem
                {
                    Name = pgn.GetEvent().Replace("\"", ""),
                    Color = color,
                    Pgn = regPgnTag().Replace(pgn.GetPgn(), "").Trim(),
                    Fen = string.Join(" ", fen)
                };
                await populateWins(item);
                gambits.Add(item);
                logger.LogInformation("{count} {color} gambits",gambits.Count,color);
            }
            return gambits;
        }

        private async Task populateWins(GambitItem item)
        {
            var url = "https://explorer.lichess.org/lichess?source=analysis&fen=" + HttpUtility.UrlEncode(item.Fen);
            using (var client = new HttpClient())
            {
                string json;
                try
                {
                    await Task.Delay(500);
                    json = await client.GetStringAsync(url);
                }
                catch (HttpRequestException ex)
                    when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    logger.LogWarning("Got 429 error code. waiting 10s...");
                    await Task.Delay(10000);
                    json = await client.GetStringAsync(url);
                }
                var games = JsonConvert.DeserializeObject<ExplorerItem>(json);
                item.Wins = item.Color == "white" ? games.white : games.black;
                item.Draws = games.draws;
                item.Losses = item.Color == "white" ? games.black : games.white;
                if (games.moves?.Length > 0)
                {
                    var stats = games.moves
                        .Select(g => new { total = g.white + g.draws + g.black, rating = g.averageRating / 100.0 });
                    item.AverageElo = (int)(stats.Sum(s => s.total * s.rating) / stats.Sum(s => s.total) * 100);
                }
            }
        }


    }
}
