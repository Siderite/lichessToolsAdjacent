using AssetGenerator.Extensions;
using AssetGenerator.Interfaces;
using Microsoft.Extensions.Logging;

//using Chess;
using Neondactyl.PgnParser.Net;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace AssetGenerator.Implementations
{
    /// <summary>
    /// Generates the opening list
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="chessManager"></param>
    public class OpeningGenerator(
        ILogger<OpeningGenerator> logger,
        IChessManager chessManager
        ) 
        : IOpeningGenerator
    {

        /// <summary>
        /// Asynchronously generates a consolidated JSON file of chess openings by aggregating data from multiple
        /// sources.
        /// </summary>
        /// <remarks>The method reads initial openings from a local JSON file and supplements them with
        /// additional data fetched from several remote and local sources. The consolidated list is serialized and saved
        /// to a file named 'openings.json'. Ensure that all required data files are accessible and that network
        /// connectivity is available for remote sources. The resulting file can be used as a comprehensive reference
        /// for chess openings.</remarks>
        /// <returns>This method does not return a value.</returns>
        public async Task Generate()
        {
            logger.LogInformation("Generating openings...");
            var openings = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(File.ReadAllText("Data/initialOpenings.json"));
            //var openings = new ConcurrentDictionary<string, string>();

            foreach (var url in new[]
            {
                "https://raw.githubusercontent.com/lichess-org/chess-openings/master/a.tsv",
                "https://raw.githubusercontent.com/lichess-org/chess-openings/master/b.tsv",
                "https://raw.githubusercontent.com/lichess-org/chess-openings/master/c.tsv",
                "https://raw.githubusercontent.com/lichess-org/chess-openings/master/d.tsv",
                "https://raw.githubusercontent.com/lichess-org/chess-openings/master/e.tsv"
            })
            {
                using (var client = new HttpClient())
                {
                    using (var stream = client.GetStreamAsync(url).GetAwaiter().GetResult())
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            var line = sr.ReadLine(); // ignore header
                            line = sr.ReadLine();
                            while (line != null)
                            {
                                var splits = line.Split('\t');
                                addToOpenings(openings, splits[1].Trim(), splits[2].Trim());
                                line = sr.ReadLine();
                            }
                        }
                    }
                }
            }

            addToOpenings(openings, "https://github.com/kentdjb/pgn-extract/raw/refs/heads/main/eco.pgn");

            addToOpenings(openings, "Data/additionalOpenings.pgn");
            addToOpenings(openings, "Data/GambitsForBlack.pgn");
            addToOpenings(openings, "Data/GambitsForWhite.pgn");

            var dict = openings.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);

            var result = JsonConvert.SerializeObject(dict, Formatting.Indented);
            Directory.CreateDirectory("Output");
            File.WriteAllText("Output/openings.json", result);
            logger.LogInformation("{Count} openings generated successfully.", dict.Count);
        }

        private void addToOpenings(ConcurrentDictionary<string, string>? openings, string path)
        {
            logger.LogInformation(" openings from {path}...", path);
            var parser = new PgnParser();
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                using (var client = new HttpClient())
                {
                    var pgnText = client.GetStringAsync(path).GetAwaiter().GetResult();
                    pgnText = Regex.Replace(pgnText, @"^\s*1\.[^\[]+", m => Regex.Replace(m.Value, @"[\r\n]+|\s*\*\s*$", " ") + "\r\n", RegexOptions.Multiline);
                    parser.ParseFromString(pgnText);
                }
            }
            else
            {
                parser.ParseFromFile(path);
            }
            var pgns = parser.GetGames();
            foreach (var pgn in pgns.Where(p => p.GetMoves() is not null))
            {
                string gamePgnText = pgn.GetPgn();
                var board = chessManager.LoadFromPgn(gamePgnText);
                var fen = board.ToFen().Split(" ");
                board.Previous();
                var lastMove = pgn.GetMovesArray().LastOrDefault(m => m?.Trim() != "*")?.Replace("*", "");
                if (string.IsNullOrWhiteSpace(lastMove)) continue;
                var key = string.Join("", fen.Take(2).Select(s => s.Replace("/", "")));
                if (!openings.ContainsKey(key))
                {
                    string name = pgn.GetEvent()?.Trim('"')?.Trim();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = pgn.GetOpeningAndVariation();
                    }
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        throw new Exception("Opening name missing for " + gamePgnText);
                    }
                    openings[key] = name;
                }
            }
        }

        private void addToOpenings(ConcurrentDictionary<string, string>? openings, string description, string pgnText)
        {
            var parser = new PgnParser();
            parser.ParseFromString(pgnText);
            var pgns = parser.GetGames();
            foreach (var pgn in pgns.Where(p => p.GetMoves() is not null))
            {
                var board = chessManager.LoadFromPgn(pgn.GetPgn());
                var fen = board.ToFen().Split(" ");
                board.Previous();
                var key = string.Join("", fen.Take(2).Select(s => s.Replace("/", "")));
                if (!openings.ContainsKey(key))
                {
                    openings[key] = description;
                }
            }
        }


    }
}
