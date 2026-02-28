using AssetGenerator.Interfaces;

namespace AssetGenerator
{
    /// <summary>
    /// Main execution class
    /// </summary>
    /// <param name="countryGenerator"></param>
    /// <param name="gambitGenerator"></param>
    /// <param name="flairGenerator"></param>
    /// <param name="iconGenerator"></param>
    /// <param name="wikiGenerator"></param>
    /// <param name="openingGenerator"></param>
    /// <param name="translationGenerator"></param>
    /// <param name="puzzleGenerator"></param>
    public class MainGenerator(
#pragma warning disable CS9113 // Parameter is unread.
        ICountryGenerator countryGenerator,
        IGambitGenerator gambitGenerator,
        IFlairGenerator flairGenerator,
        IIconGenerator iconGenerator,
        IWikiGenerator wikiGenerator,
        IOpeningGenerator openingGenerator,
        ITranslationGenerator translationGenerator,
        IPuzzleGenerator puzzleGenerator
#pragma warning restore CS9113 // Parameter is unread.
            )
    {
        /// <summary>
        /// Generates the various files needed for the chess tools, including country data, gambit information, 
        /// flair details, wiki URLs, opening data, and translations. 
        /// Comment / uncomment the lines you need.
        /// </summary>
        /// <returns></returns>
        public async Task Generate()
        {
            var tasks = new List<Task>
            {
                countryGenerator.Generate(),
                gambitGenerator.Generate(),
                flairGenerator.Generate(),
                iconGenerator.Generate(),
                wikiGenerator.Generate(),
                openingGenerator.Generate(),
                translationGenerator.Generate(),
                puzzleGenerator.RefreshPuzzleFile().ContinueWith(async t =>
                    {
                        if ( t.Result) {
                            await puzzleGenerator.GenerateNif();
                        }
                    }
                ),
                // Miscellaneous one shots
                //gambitGenerator.RankGambits()
            };
            await Task.WhenAll(tasks);
        }

    }
}