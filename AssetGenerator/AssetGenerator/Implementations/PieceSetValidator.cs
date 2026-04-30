using AssetGenerator.Interfaces;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Svg;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace AssetGenerator.Implementations
{
    /// <summary>
    /// Validates piece sets
    /// </summary>
    /// <param name="logger">The logger used to record diagnostic and validation information.</param>
    public class PieceSetValidator(ILogger<PieceSetValidator> logger)
        : IPieceSetValidator
    {
        /// <summary>
        /// Checks if piece sets have all pieces available or are duplicated
        /// </summary>
        /// <remarks>The pieceSets.json file will be read from the LiChessTools master repo, so make sure it's updated. 
        /// Only the piece sets with cap set will be validated.</remarks>
        public async Task Validate()
        {
            logger.LogInformation("Validating piece sets...");

            var sourceFile = "https://github.com/Siderite/lichessTools/raw/refs/heads/master/data/pieceSets.json";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "LiChessToolsAssetGenerator");
            var text = await client.GetStringAsync(sourceFile);
            var data = JsonConvert.DeserializeObject<PieceSetFile>(text);
            data.pieceSets.InsertRange(0, lichessPieces);
            data.pieceSets.RemoveAll(ps => ps.duplicate);

            var colors = new[] { "w", "b" };
            var pieces = new[] { "p", "n", "b", "r", "q", "k" };
            var hashList = new List<PieceHash>();
            var hashFilePath = "Output/pieceSetHashes.json";
            if (File.Exists(hashFilePath))
            {
                var serializedHashList = File.ReadAllText(hashFilePath);
                hashList = JsonConvert.DeserializeObject<List<PieceHash>>(serializedHashList);
            }
            var hasher = new DifferenceHash();
            var rnd = new Random();
            foreach (var pieceSet in data.pieceSets)
            {
                var similarities = new Dictionary<string, double>();
                logger.LogInformation(" ... validating {pieceSet}", pieceSet.key);
                foreach (var piece in pieces)
                {
                    foreach (var color in colors)
                    {
                        var url = GetPieceUrl(pieceSet, piece, color);
                        ulong imageHash = 0;
                        var same = hashList.Find(ph => ph.PieceSetKey == pieceSet.key && ph.Color == color && ph.Piece == piece);
                        string etag = null;
                        if (same != null)
                        {
                            imageHash = same.Hash;
                            if (rnd.Next(24) == 0) // occasionally check if the piece has changed by comparing ETags
                            {
                                try
                                {
                                    etag = await GetEtag(url);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error heading piece {piece} for color {color} for set {pieceSet}", piece, color, pieceSet.key);
                                }
                                if (etag != null)
                                {
                                    if (same.ETag == null)
                                    {
                                        same.ETag = etag;
                                    }
                                    else if (same.ETag != etag)
                                    {
                                        hashList.RemoveAll(ph => ph.PieceSetKey == pieceSet.key);
                                        same = null;
                                    }
                                }
                            }
                        }
                        if (same == null)
                        {
                            byte[] bytes = null;
                            try
                            {
                                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                                var response = await client.SendAsync(request);
                                response.EnsureSuccessStatusCode();

                                bytes = await response.Content.ReadAsByteArrayAsync();

                                etag = response.Headers.ETag?.Tag;

                                using var image = LoadImageBytes(bytes, pieceSet.type);
                                imageHash = hasher.Hash(image);
                                hashList.Add(new PieceHash { Hash = imageHash, PieceSetKey = pieceSet.key, Color = color, Piece = piece, ETag = etag });
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error getting piece {piece} for color {color} for set {pieceSet}", piece, color, pieceSet.key);
                            }
                        }
                        if (imageHash != 0)
                        {
                            foreach (var existing in hashList.Where(ph => ph.Color == color && ph.Piece == piece))
                            {
                                if (existing.PieceSetKey.StartsWith(pieceSet.category+"/")) break; // only compare with sets before it in the list
                                var similarity = CompareHash.Similarity(imageHash, existing.Hash);
                                if (!similarities.TryGetValue(existing.PieceSetKey, out var sim))
                                {
                                    sim = 0;
                                }
                                similarities[existing.PieceSetKey] = sim + (similarity / 12.0); // 6 pieces per color
                            }
                        }
                    }
                }
                var serializedHashList = JsonConvert.SerializeObject(hashList, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(hashFilePath, serializedHashList);
                var mostSimilar = similarities.OrderByDescending(kv => kv.Value).FirstOrDefault();
                var maxSimilarity = mostSimilar.Value;
                if (maxSimilarity > 90)
                {
                    logger.LogWarning("Piece set {pieceSet} is {similarity}% similar to {similarSet}", pieceSet.key, maxSimilarity, mostSimilar.Key);
                }
            }
        }

        private async Task<string> GetEtag(string rawUrl)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "LiChessToolsAssetGenerator");

            using var request = new HttpRequestMessage(HttpMethod.Head, rawUrl);

            var response = await client.SendAsync(request);

            return response.Headers.ETag?.Tag;
        }

        private Image<Rgba32> LoadImageBytes(byte[] bytes, string type)
        {
            MemoryStream ms;
            if (type == "svg")
            {
                var doc = new XmlDocument();
                doc.LoadXml(Encoding.UTF8.GetString(bytes).Replace("currentColor","#808080"));
                // Load and rasterize SVG
                var svgDocument = SvgDocument.Open(doc);

                // Render to bitmap at a fixed reasonable size for hashing
                // Chess pieces usually look good at 128-256 px
                using var bitmap = svgDocument.Draw(100, 100);

                // Convert System.Drawing.Bitmap -> ImageSharp
                ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
            }
            else
            {
                ms = new MemoryStream(bytes);
            }
            var image = Image.Load<Rgba32>(ms);
            image.Mutate(x => x.Resize(100, 100));
            ms.Dispose();
            return image;
        }

        private string? GetPieceUrl(PieceSet pieceSet, string piece, string color)
        {
            var fullPieceDict = new Dictionary<string, string>
            {
               {"p","pawn"},
               {"n","knight" },
               {"b","bishop" },
               {"r","rook" },
               {"q","queen" },
               {"k","king" }
            };
            var url = pieceSet.url;
            switch (pieceSet.cap ?? pieceSet.category)
            {
                case "wN":
                    url += $"{color}{piece.ToUpper()}.{pieceSet.type}";
                    break;
                case "wn":
                    url += $"{color}{piece}.{pieceSet.type}";
                    break;
                case "WN":
                    url += $"{color.ToUpper()}{piece.ToUpper()}.{pieceSet.type}";
                    break;
                case "nw":
                    url += $"{piece}{color}.{pieceSet.type}";
                    break;
                case "basedpolymer":
                    var key = color + piece;
                    if (pieceSet.name == "ichess")
                    {
                        var ring = new Dictionary<string, string> {
                                { "bp","j2WrNG" },
                                { "br", "fzAmF1" },
                                { "bn", "JAq5BZ" },
                                { "bb", "ZxcpUI" },
                                { "bq", "tgDj55" },
                                { "bk", "Eu0v0L" },
                                { "wp", "snAUn" },
                                { "wr", "ZB0EnP" },
                                { "wn", "AKcFJe" },
                                { "wb", "IzedLx" },
                                { "wq", "qfWM82" },
                                { "wk", "3H6DG9" }
                            };
                        key = ring[key];
                    }
                    url += key + "." + pieceSet.type;
                    break;
                case "comfysage":
                    url += color + "/" + color + piece + "." + pieceSet.type;
                    break;
                case "DragurKnight":
                    var fullPiece = fullPieceDict[piece];
                    url += color + "_" + fullPiece + "." + pieceSet.type;
                    break;
            }
            return url;
        }

        private class PieceSetFile
        {
            public List<PieceSet> pieceSets { get; set; }
        }

        private class PieceSet
        {
            public string category { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string type { get; set; }
            public string cap { get; set; }
            public bool duplicate { get; set; }
            public string key => $"{category}/{name}";
        }

        private class PieceHash
        {
            public ulong Hash { get; set; }
            public string PieceSetKey { get; set; }
            public string Color { get; set; }
            public string Piece { get; set; }
            public string ETag { get; set; }
        }

        private List<PieceSet> lichessPieces =
        [
            new PieceSet { category = "lichess", name = "alpha", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/alpha/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "anarcandy", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/anarcandy/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "caliente", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/caliente/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "california", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/california/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "cardinal", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/cardinal/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "cburnett", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/cburnett/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "celtic", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/celtic/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "chess7", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/chess7/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "chessnut", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/chessnut/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "companion", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/companion/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "cooke", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/cooke/", type = "svg", cap = "wN" },
            //new PieceSet { category = "lichess", name = "disguised", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/disguised/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "dubrovny", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/dubrovny/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "fantasy", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/fantasy/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "firi", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/firi/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "fresca", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/fresca/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "gioco", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/gioco/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "governor", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/governor/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "horsey", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/horsey/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "icpieces", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/icpieces/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "kiwen-suwi", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/kiwen-suwi/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "kosal", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/kosal/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "leipzig", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/leipzig/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "letter", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/letter/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "maestro", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/maestro/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "merida", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/merida/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "monarchy", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/monarchy/", type = "webp", cap = "wN" },
            new PieceSet { category = "lichess", name = "mpchess", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/mpchess/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "pirouetti", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/pirouetti/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "pixel", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/pixel/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "reillycraig", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/reillycraig/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "rhosgfx", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/rhosgfx/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "riohacha", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/riohacha/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "shahi-ivory-brown", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/shahi-ivory-brown/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "shapes", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/shapes/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "spatial", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/spatial/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "staunty", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/staunty/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "tatiana", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/tatiana/", type = "svg", cap = "wN" },
            new PieceSet { category = "lichess", name = "xkcd", url = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/piece/xkcd/", type = "svg", cap = "wN" },
        ];
    }
}
