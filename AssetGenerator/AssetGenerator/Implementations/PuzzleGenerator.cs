using AssetGenerator.Extensions;
using AssetGenerator.Interfaces;
using AssetGenerator.Models;
using LiteDB;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;
using ZstdSharp;

namespace AssetGenerator.Implementations
{
    /// <summary>
    /// Handles puzzles from the lichess database
    /// </summary>
    /// <param name="chess"></param>
    /// <param name="lowerCaseEncoder"></param>
    /// <param name="logger"></param>
    public partial class PuzzleGenerator(
        IChessManager chess,
        ILowerCaseEncoder lowerCaseEncoder,
        ILogger<PuzzleGenerator> logger
        )
        : IPuzzleGenerator
    {
        /// <summary>
        /// Generates a NIF file containing puzzle data by processing chess puzzles from a CSV file and writing the
        /// results to disk asynchronously.
        /// </summary>
        /// <remarks>This method reads chess puzzle data from the 'lichess_db_puzzle.csv' file, processes
        /// the data to extract n-grams and their associated puzzle identifiers, and writes the output to a file named
        /// 'puzzle.nif'. Progress is logged at various stages, including reading, processing, and writing data. Ensure
        /// that the input file exists and is formatted correctly to avoid errors during execution.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task GenerateNif()
        {
            var start = DateTime.Now;
            const int ngramSize = 3;
            var lineNr = 0;

            logger.LogInformation("Initializing puzzle cache db...");
            using var db = new LiteDatabase("puzzleCache.db");
            var col = db.GetCollection<DbPuzzleItem>("puzzles");

            logger.LogInformation("Starting to read lichess_db_puzzle.csv...");

            var puzzles = File.ReadAllLines("Data/lichess_db_puzzle.csv")
                .Skip(1)
                //.Where(l=>l.StartsWith("0EbLC"))
                //.Take(10000)
                .AsParallel().WithDegreeOfParallelism(16)
                .Select(line =>
                {
                    lineNr++;
                    if (lineNr % 10000 == 0)
                    {
                        logger.LogInformation("reading: {LineNr}", lineNr);
                        db.Checkpoint();
                    }
                    return getPuzzle(line, col);
                })
                .OrderBy(p => p.PuzzleId)
                .ToList();
            logger.LogInformation("read: {LineNr}", lineNr);

            logger.LogInformation("Removing obsolete puzzles from cache...");
            var keysToKeep = new HashSet<string>(puzzles.Select(p => lowerCaseEncoder.Encode(p.PuzzleId)));
            var existingKeys = col
                .FindAll()
                .Select(doc => doc.Key)
                .ToHashSet();
            var keysToDelete = new BsonArray(existingKeys.Except(keysToKeep).Select(s => new BsonValue(s)));
            var deletedCount = col.DeleteMany(Query.In("_id", keysToDelete));
            Console.WriteLine($"Deleted {deletedCount} obsolete puzzles.");


            var puzzleData = new ConcurrentDictionary<string, List<string>>();
            var ngramString = new StringBuilder();
            var ngramDict = new Dictionary<string, int>();
            lineNr = 0;
            foreach (var puzzleItem in puzzles)
            {
                lineNr++;
                if (lineNr % 10000 == 0)
                {
                    logger.LogInformation("processing: {LineNr}", lineNr);
                }
                processPuzzle(ngramSize, puzzleData, ngramString, ngramDict, puzzleItem);
            }
            var puzzleIds = new HashSet<string>();
            var newPuzzleData = new ConcurrentDictionary<string, HashSet<string>>();
            logger.LogInformation("Filtering to most effective N-grams from {count}...", puzzleData.Count);
            lineNr = 0;
            var startTime = DateTime.Now.Ticks;
            var minimizeThreshold = puzzles.Count / 100;
            foreach (var pair in puzzleData.OrderBy(p => p.Value.Count))
            {
                lineNr++;
                if (DateTime.Now.Ticks - startTime > 5000)
                {
                    logger.LogInformation(" processing: {LineNr}", lineNr);
                }
                HashSet<string> set = [.. pair.Value];
                newPuzzleData[pair.Key] = set;
                puzzleIds.UnionWith(set);
                if (pair.Value.Count > minimizeThreshold && puzzleIds.Count == puzzles.Count)
                    break;
            }
            logger.LogInformation(" processed: {LineNr}", lineNr);

            logger.LogInformation("Puzzles: {Count}", puzzleIds.Count);
            logger.LogInformation("N-grams: {Count}", puzzleData.Count);
            logger.LogInformation("Minimal N-grams: {Count}", newPuzzleData.Count);
            logger.LogInformation("Compressed N-grams size: {Length}", ngramString.Length);
            puzzleData = null;
            logger.LogInformation("Creating puzzle.nif...");
            var idSize = (byte)puzzleIds.Max(id => id.Length);
            var idIndexSize = sizeInBytes(puzzleIds.Count);
            var idCountSize = sizeInBytes(newPuzzleData.Values.Max(v => v.Count));
            //var ngramIndexSize = SizeInBytes(newPuzzleData.Keys.Max(k => ngramDict[k]));
            var crcCountSize = sizeInBytes(puzzles.Max(p => p.Fens.Count));
            var crcSize = (byte)3;
            Directory.CreateDirectory("Output");
            using (var stream = File.Create("Output/puzzle.nif", 10000000))
            {
                stream.WriteString("NIF");
                stream.WriteByte(2); // version
                stream.WriteByte(idSize); // resource identifier size
                stream.WriteByte(idCountSize);
                stream.WriteUint(puzzleIds.Count);
                stream.WriteByte(ngramSize);
                stream.WriteUint(ngramString.Length);
                stream.WriteByte(crcSize);
                stream.WriteString(ngramString.ToString());
                logger.LogInformation("Writing {Count} puzzle ids", puzzles.Count);
                foreach (var puzzle in puzzles)
                {
                    stream.WriteString(puzzle.PuzzleId);
                }
                var pos = (int)stream.Position
                    + (ngramString.Length - ngramSize + 1) * (4 + idCountSize)
                    + puzzleIds.Count * (4 + 1);
                logger.LogInformation("Writing {Count} ngrams position and id counts", ngramString.Length - ngramSize + 1);
                for (var i = 0; i < ngramString.Length - ngramSize + 1; i++)
                {
                    stream.WriteUint(pos);
                    var ngram = ngramString.ToString(i, ngramSize);
                    var ngramIdCount = newPuzzleData.TryGetValue(ngram, out HashSet<string>? value) ? value.Count : 0;
                    stream.WriteNumber(ngramIdCount, idCountSize);
                    pos += ngramIdCount * idIndexSize;
                }
                logger.LogInformation("Writing position and CRC counts for {Count} puzzles", puzzles.Count);
                foreach (var puzzle in puzzles)
                {
                    stream.WriteUint(pos);
                    var idCrcCount = (byte)puzzle.Fens.Count;
                    stream.WriteByte(idCrcCount);
                    pos += idCrcCount * crcSize;
                }
                logger.LogInformation("Writing {Count} ngrams idIndexes", ngramString.Length - ngramSize + 1);
                var idDict = puzzleIds
                    .OrderBy(p => p)
                    .Select((id, index) => (id, index))
                    .ToDictionary(x => x.id, x => x.index);
                for (var i = 0; i < ngramString.Length - ngramSize + 1; i++)
                {
                    var ngram = ngramString.ToString(i, ngramSize);
                    if (newPuzzleData.TryGetValue(ngram, out HashSet<string> ids))
                    {
                        foreach (var id in ids)
                        {
                            stream.WriteNumber(idDict[id], idIndexSize);
                        }
                    }

                }
                logger.LogInformation("Writing {Count} puzzle CRCs", puzzles.Count);
                foreach (var puzzle in puzzles)
                {
                    foreach (var fullFen in puzzle.Fens)
                    {
                        var fen = String.Join(" ", fullFen.Split(' ').Take(2));
                        stream.WriteNumber(crc24(fen), crcSize);
                    }
                }
                logger.LogInformation("File created.");
                logger.LogInformation("Time taken: {TimeTaken} seconds", (DateTime.Now - start).TotalSeconds);
            }
        }

        private byte sizeInBytes(int nr)
        {
            return (byte)Math.Max(1, Math.Ceiling(Math.Log(nr, 256)));
        }

        private int crc24(string data)
        {
            var polynomial = 0x864CFB;
            var crc = 0xFFFFFF;
            for (var i = 0; i < data.Length; i++)
            {
                crc ^= (byte)data[i];
                for (var j = 0; j < 8; j++)
                {
                    crc = (crc >>> 1) ^ ((crc & 1) > 0 ? polynomial : 0);
                }
            }
            return crc ^ 0xFFFFFF;
        }


        private void processPuzzle(int ngramSize, ConcurrentDictionary<string, List<string>> puzzleData, StringBuilder ngramString, Dictionary<string, int> ngramDict, PuzzleItem puzzleItem)
        {
            var fenIndex = 0;
            foreach (var fullFen in puzzleItem.Fens)
            {
                var fen = String.Join(" ", fullFen.Split(' ').Take(2));
                var puzzleId = puzzleItem.PuzzleId;
                for (var i = 0; i <= fen.Length - ngramSize; i++)
                {
                    var ngram = fen.Substring(i, ngramSize);
                    if (!ngramDict.TryGetValue(ngram, out int index))
                    {
                        int j = ngramSize - 1;
                        while (j > 0)
                        {
                            if (ngramString.Length >= j && ngramString.ToString(ngramString.Length - j, j) == ngram[..j])
                            {
                                break;
                            }
                            j--;
                        }
                        index = ngramString.Length - ngramSize;
                        ngramString.Append(ngram[j..]);
                        for (var k = 0; k < ngramSize - j; k++)
                        {
                            index++;
                            if (index >= 0)
                            {
                                var ngram2 = ngramString.ToString(index, ngramSize);
                                if (!ngramDict.TryGetValue(ngram2, out int _))
                                {
                                    ngramDict[ngram2] = index;
                                }
                            }
                        }
                    }
                    var list = puzzleData.GetOrAdd(ngram, []);
                    list.Add(puzzleId);
                }
                fenIndex++;
            }
        }

        private PuzzleItem getPuzzle(string line, ILiteCollection<DbPuzzleItem> col)
        {
            var data = line.Split(',');
            var obj = new PuzzleItem
            {
                PuzzleId = data[0],
                Fens = [data[1]],
                Moves = data[2].Split(' '),
                Rating = int.Parse(data[3]),
                RatingDeviation = int.Parse(data[4]),
                Popularity = int.Parse(data[5]),
                NbPlays = int.Parse(data[6]),
                Themes = data[7].Split(' '),
                GameUrl = data[8],
                OpeningTags = data.Length > 9 ? data[9].Split(' ') : [],
                Pgns = []
            };


            var key = lowerCaseEncoder.Encode(obj.PuzzleId);
            var existing = col.FindOne(x => x.Key == key);
            if (existing != null)
            {
                return existing.ToPuzzleItem();
            }
            var board = chess.LoadFromFen(obj.Fens[0]);
            for (int i = 0; i < obj.Moves.Length; i++)
            {
                var uci = obj.Moves[i];
                board.MoveUci(uci);
                obj.Fens.Add(board.ToFen());
            }
            obj.Pgns.Add(board.ToPgn());
            col.Insert(DbPuzzleItem.FromPuzzleItem(key,obj));
            return obj;
        }

        /// <summary>
        /// Redownload the puzzle file from the LiChess server if it's newer than the local one.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshPuzzleFile()
        {
            var updated = await UpdateFileIfNewerAsync("https://database.lichess.org/lichess_db_puzzle.csv.zst", "Data/lichess_db_puzzle.csv.zst");
            using var input = File.OpenRead("Data/lichess_db_puzzle.csv.zst");
            using var output = File.OpenWrite("Data/lichess_db_puzzle.csv");
            using var decompressionStream = new DecompressionStream(input);
            decompressionStream.CopyTo(output);
            return updated;
        }

        private async Task<bool> UpdateFileIfNewerAsync(string url, string localPath)
        {
            logger.LogInformation("Downloading from {url} to {path}", url, localPath);
            using var client = new HttpClient();
            if (File.Exists(localPath))
            {
                var lastWrite = File.GetLastWriteTimeUtc(localPath);

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.IfModifiedSince = lastWrite;

                try
                {
                    using var response = await client.SendAsync(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                    {
                        logger.LogInformation("File is up-to-date (304 Not Modified).");
                        return false;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogError("Server returned {StatusCode} -> downloading anyway.", response.StatusCode);
                    }

                    // If we got here -> either no 304 or server ignored conditional -> download
                }
                catch (Exception ex)
                {
                    // Network error / timeout -> try normal download below
                    logger.LogError(ex, "Error checking file freshness -> downloading anyway.");
                }
            }

            // ───────────────────────────────────────
            // Download (file missing or conditional check didn't skip)
            // ───────────────────────────────────────

            byte[] bytes = await client.GetByteArrayAsync(url);

            // Atomic save
            string temp = localPath + ".tmp";
            await File.WriteAllBytesAsync(temp, bytes);

            // Try to preserve server timestamp if available
            try
            {
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (response.Content.Headers.LastModified.HasValue)
                {
                    File.SetLastWriteTimeUtc(temp, response.Content.Headers.LastModified.Value.UtcDateTime);
                }
            }
            catch (Exception ex) {
                /* ignore - timestamp is nice-to-have */
                logger.LogError(ex, "Error preserving timestamp - ignoring.");
            }

            File.Move(temp, localPath, overwrite: true);

            logger.LogInformation($"Saved {bytes.Length:N0} bytes -> {localPath}");
            return true;
        }
    }
}