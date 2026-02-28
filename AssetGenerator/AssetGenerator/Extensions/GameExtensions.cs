using Neondactyl.PgnParser.Net;
using System.Text.RegularExpressions;

namespace AssetGenerator.Extensions
{
    /// <summary>
    /// Extension methods for the Game class from the Neondactyl.PgnParser.Net library, 
    /// providing additional functionality to extract opening and variation information from PGN data.
    /// </summary>
    public static partial class GameExtensions
    {

        [GeneratedRegex(@"\[\s*Opening\s*""(?<opening>[^""]*)""\s*\]")]
        private static partial Regex openingTagRegex();
        [GeneratedRegex(@"\[\s*Variation\s*""(?<variation>[^""]*)""\s*\]")]
        private static partial Regex variationTagRegex();

        extension(Game game)
        {
            /// <summary>
            /// Retrieves the chess opening and variation names from the game's PGN (Portable Game Notation) data.
            /// </summary>
            /// <remarks>The method extracts the opening and variation information by searching for
            /// corresponding tags within the PGN string. Only non-empty values are included in the result, separated by
            /// a comma if both are present.</remarks>
            /// <returns>A comma-separated string containing the opening and variation names. Returns an empty string if neither
            /// is found.</returns>
            public string GetOpeningAndVariation()
            {
                var pgn = game.GetPgn();
                var opening = openingTagRegex().Match(pgn)?.Groups["opening"].Value ?? "";
                var variation = variationTagRegex().Match(pgn)?.Groups["variation"].Value ?? "";
                return new[] { opening, variation }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Aggregate((a, b) => a + ", " + b);
            }
        }
    }
}