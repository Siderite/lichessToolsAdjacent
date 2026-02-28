using AssetGenerator.Interfaces;
using AssetGenerator.Models;
//using Chess;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AssetGenerator
{
    /// <summary>
    /// Generates the relations between chess opening theory pages on Wikibooks and their corresponding FEN positions.
    /// </summary>
    /// <param name="chessManager"></param>
    /// <param name="logger"></param>
    public partial class WikiGenerator(
        IChessManager chessManager,
        ILogger<WikiGenerator> logger
        ) 
        : IWikiGenerator
    {
        [GeneratedRegex(@"^Chess Opening Theory(?<move>/\d+\.+\s*[^\s/]+)*$")]
        private partial Regex regCotTitle();

        /// <summary>
        /// Asynchronously generates a collection of wiki URLs related to chess opening theory by querying the Wikibooks
        /// API and writes the resulting associations to a JSON file.
        /// </summary>
        /// <remarks>The method handles pagination by repeatedly querying the API until all relevant
        /// results are retrieved. The resulting associations are serialized and saved to a file named "wikiUrls.json".
        /// Logging is used to provide progress and error information throughout the operation.</remarks>
        /// <returns>This method does not return a value.</returns>
        /// <exception cref="HttpRequestException">Thrown if an error occurs while retrieving wiki URLs from the Wikibooks API, such as when the response
        /// contains an error code.</exception>
        public async Task Generate()
        {
            logger.LogInformation("Generating wiki URLs...");
            var baseUrl = "https://en.wikibooks.org/w/api.php?action=query&list=search&srnamespace=0&srprop=timestamp&srsearch=intitle:Chess%20Opening%20Theory&srlimit=max&format=json&stable=1";
            var url = baseUrl;
            using (var client = new HttpClient())
            {

                // Set up headers to match the fetch request
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                client.DefaultRequestHeaders.Add("Pragma", "no-cache");
                client.DefaultRequestHeaders.Add("Priority", "u=0, i");
                client.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\"Not;A=Brand\";v=\"99\", \"Brave\";v=\"139\", \"Chromium\";v=\"139\"");
                client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?0");
                client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                client.DefaultRequestHeaders.Add("Sec-Gpc", "1");
                client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");

                var list = new List<string>();
                //File.Delete("wiki.json");
                while (true)
                {
                    var json = await client.GetStringAsync(url);
                    //File.AppendAllText("wiki.json", json + "\r\n\r\n");
                    var search = JsonConvert.DeserializeObject<SearchModel>(json);
                    if (search.error != null)
                    {
                        throw new HttpRequestException("Error getting wiki URLs: " + search.error.code + " (" + search.error.info + ")");
                    }
                    if (search?.query?.search?.Count > 0)
                    {
                        list.AddRange(search.query.search.Select(s => s.title));
                    }
                    else
                    {
                        break;
                    }
                    if (search?.@continue == null) break;
                    url = baseUrl + "&sroffset=" + search?.@continue?.sroffset + "&continue=" + search?.@continue?.@continue;
                }
                var regex = regCotTitle();
                var associations = new Dictionary<string, HashSet<string>>();
                list.Sort();
                foreach (var title in list)
                {
                    try
                    {
                        var match = regex.Match(title);
                        if (!match.Success) continue;
                        var moves = match.Groups["move"].Captures.OfType<Capture>().Select(c => Regex.Replace(c.Value, @"^/\d+\.+\s*", "")).ToList();
                        var board = chessManager.NewBoard();
                        foreach (var moveText in moves)
                        {
                            board.MoveSan(moveText);
                        }
                        var fen = board.ToFen().Split(" ");
                        var fenText = string.Join("", fen.Take(2).Select(p => p.Replace("/", "")));
                        if (!associations.TryGetValue(fenText, out HashSet<string> titles))
                        {
                            titles = [];
                            associations[fenText] = titles;
                        }
                        titles.Add(title);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error for title {Title}", title);
                    }
                }
                var result = JsonConvert.SerializeObject(associations, Formatting.Indented);
                Directory.CreateDirectory("Output");
                File.WriteAllText("Output/wikiUrls.json", result);
                logger.LogInformation("{Count} wiki URLs generated successfully.", associations.Count);
            }
        }

    }
}
