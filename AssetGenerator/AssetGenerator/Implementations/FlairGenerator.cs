using AssetGenerator.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AssetGenerator.Implementations
{
    /// <summary>
    /// Provides functionality to generate a JSON file containing a list of flairs retrieved from an external source.
    /// </summary>
    /// <remarks>The generated JSON file is written to the application's working directory as 'flairs.json'.
    /// Ensure the application has appropriate file system permissions before invoking flair generation. This class is
    /// not thread-safe.</remarks>
    /// <param name="logger">The logger used to record informational messages during the flair generation process.</param>
    public partial class FlairGenerator(ILogger<FlairGenerator> logger)
        : IFlairGenerator
    {
        /// <summary>
        /// Asynchronously retrieves a list of flairs from an external source and generates a JSON file containing the
        /// flairs.
        /// </summary>
        /// <remarks>The generated file is named 'flairs.json' and is created in the application's working
        /// directory. The method logs the number of flairs generated. Ensure the application has write permissions to
        /// the target directory before calling this method.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task Generate()
        {
            using (var client = new HttpClient())
            {
                logger.LogInformation("Generating flairs...");
                var text = await client.GetStringAsync("https://lichess1.org/assets/flair/list.txt");
                var obj = new
                {
                    flairs = newLinesRegex().Split(text).Where(s => !String.IsNullOrWhiteSpace(s)).ToList()
                };
                Directory.CreateDirectory("Output");
                File.WriteAllText("Output/flairs.json", JsonConvert.SerializeObject(obj, Formatting.Indented));
                logger.LogInformation("{Count} flairs generated successfully.", obj.flairs.Count);
            }
        }

        [GeneratedRegex(@"[\r\n]+", RegexOptions.Singleline)]
        private static partial Regex newLinesRegex();
    }
}