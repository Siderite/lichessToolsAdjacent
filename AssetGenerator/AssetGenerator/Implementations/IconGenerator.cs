using AssetGenerator.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetGenerator.Implementations
{
    /// <summary>
    /// Generates icons from the Lichess sfd file
    /// </summary>
    public class IconGenerator(
          ILogger<IconGenerator> logger
        ) : IIconGenerator
    {
        private const string _sfdFileUrl = "https://github.com/lichess-org/lila/raw/refs/heads/master/public/font/lichess.sfd";
        private const string jsIconsFile = "Output/lichess-icons.js";
        private const string cssIconsFile = "Output/lichess-icons.css";

        /// <summary>
        /// Generate the js and css files for the icons
        /// </summary>
        /// <returns></returns>
        public async Task Generate()
        {
            //StartChar: share-ios
            //Encoding: 57347 57347 4
            using (var client = new HttpClient())
            {
                logger.LogInformation("Generating icon files...");
                var text = await client.GetStringAsync(_sfdFileUrl);
                Directory.CreateDirectory("Output");

                File.Delete(jsIconsFile);
                File.Delete(cssIconsFile);
                File.AppendAllText(jsIconsFile, "// generate this with IconGenerator\r\nlichessIcons = {\r\n");
                File.AppendAllText(cssIconsFile, "/* generate this with IconGenerator */\r\n:root {\r\n");

                var count = 0;
                Regex.Matches(text, @"StartChar:\s+(?<name>[^\s]+)[\s\r\n]+Encoding:\s+(?<encoding1>\d+)\s+(?<encoding2>\d+)", RegexOptions.Singleline)
                    .Select(m => new
                    {
                        name = m.Groups["name"].Value,
                        encoding1 = m.Groups["encoding1"].Value,
                        encoding2 = m.Groups["encoding2"].Value
                    })
                    .Where(a=> a.encoding1 == a.encoding2) // Only consider icons where the two encoding values are the same
                    .OrderBy(a=>a.encoding1)
                    .ToList()
                    .ForEach(a =>
                    {
                        count++;
                        var unicode = int.Parse(a.encoding1);
                        var name = a.name.Split('-').Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)).Aggregate((s1, s2) => s1 + s2);
                        File.AppendAllText(jsIconsFile, $"  {name}: '\\u{unicode:X4}',\r\n");
                        File.AppendAllText(cssIconsFile, $"  --icon-{name}: \"\\{unicode:X4}\";\r\n");
                    });

                logger.LogInformation("{Count} icons generated successfully for JS and CSS.", count);
                File.AppendAllText(jsIconsFile, "\r\n};");
                File.AppendAllText(cssIconsFile, "\r\n}");

            }
        }
    }
}
