using AssetGenerator.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace AssetGenerator.Implementations
{
    /// <summary>
    /// Generates translation JSON files from the crowdin.zip file, which contains translations for different languages.
    /// The generated JSON file is saved as 'crowdin.json' in the local directory. 
    /// </summary>
    /// <param name="crowdinDownloader"></param>
    /// <param name="logger"></param>
    public class TranslationGenerator(
        ICrowdinDownloader crowdinDownloader,
        ILogger<TranslationGenerator> logger
        ): ITranslationGenerator
    {
        /// <summary>
        /// Generates a consolidated translation file from language-specific JSON files contained within a zip archive.
        /// </summary>
        /// <remarks>The method processes each language JSON file found in the 'Data/crowdin.zip' archive,
        /// aggregates their contents, and writes the combined result to a 'crowdin.json' file. The operation logs
        /// progress and the total number of translations generated. Ensure that the input zip file exists and contains
        /// valid JSON files named according to the expected pattern.</remarks>
        /// <returns>This method does not return a value.</returns>
        public async Task Generate()
        {
            logger.LogInformation("Generating translations...");
            var total = 0;
            await crowdinDownloader.DownloadBundle("Data/crowdin.zip");
            using (var zipFile = ZipFile.OpenRead("Data/crowdin.zip"))
            {
                var result = new ExpandoObject();
                foreach (var entry in zipFile.Entries)
                {
                    logger.LogInformation(" {entry}...", entry.Name);
                    var match = Regex.Match(entry.Name, @"LiChessTools\.(?<lang>[^\.]+)\.json");
                    if (match.Success)
                    {
                        var language = match.Groups["lang"].Value.Replace("_", "-");
                        var content = new StreamReader(entry.Open()).ReadToEnd();
                        JObject pack = JsonConvert.DeserializeObject<JObject>(content);
                        total += pack.Count;
                        result.TryAdd(language, pack);
                    }
                }
                total/=zipFile.Entries.Count;
                Directory.CreateDirectory("Output");
                File.WriteAllText("Output/crowdin.json", JsonConvert.SerializeObject(result, Formatting.Indented));
                logger.LogInformation("{total} translations for {languages} languages generated successfully.", total, zipFile.Entries.Count);
            }
        }
    }
}
