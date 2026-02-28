using AssetGenerator.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AssetGenerator.Implementations
{
    /// <summary>
    /// Provides functionality to generate a JSON file containing a list of countries with their codes and names by
    /// retrieving and processing data from a remote source.
    /// </summary>
    /// <remarks>The generated JSON file is saved as 'countries.json' in the local directory. This class is
    /// intended for use in scenarios where an up-to-date list of countries is required, such as for populating
    /// dropdowns or validating country codes.</remarks>
    /// <param name="logger">The logger used to record informational messages during the country generation process.</param>
    public class CountryGenerator(ILogger<CountryGenerator> logger)
        :ICountryGenerator
    {
        /// <summary>
        /// Asynchronously generates a JSON file containing a list of countries with their codes and names by retrieving
        /// data from a remote source.
        /// </summary>
        /// <remarks>This method downloads country data from a predefined URL, processes it to extract
        /// country codes and names, and writes the result to a file named 'countries.json' in the application's working
        /// directory. The method logs the number of countries generated upon successful completion. Ensure that the
        /// application has permission to write to the file system before calling this method.</remarks>
        /// <returns></returns>
        public async Task Generate()
        {
            logger.LogInformation("Generating countries...");
            var url = "https://raw.githubusercontent.com/lichess-org/lila/refs/heads/master/modules/user/src/main/Flags.scala";
            using (var client = new HttpClient())
            {
                var text = await client.GetStringAsync(url);
                var matches = Regex.Matches(text, @"[CF]\(""(?<code>[^""]+)""\s*,\s*""(?<name>[^""]+)""(?:\s*,\s*""([^""]+)""\s*)?\)(?:,|$)");
                var list = matches
                    .Select(m => new[] { m.Groups["code"].Value, m.Groups["name"].Value })
                    .ToList();
                var result = JsonConvert.SerializeObject(new { countries = list }, Formatting.Indented);
                Directory.CreateDirectory("Output");
                File.WriteAllText("Output/countries.json", result);
                logger.LogInformation("{Count} countries generated successfully.", list.Count);
            }
        }
    }
}
