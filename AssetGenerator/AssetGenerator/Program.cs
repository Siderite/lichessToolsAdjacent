using AssetGenerator.Extensions;
using AssetGenerator.Implementations;
using AssetGenerator.Implementations.GeraChessWrap;
using AssetGenerator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AssetGenerator
{
    internal partial class Program
    {
        static async Task Main()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var services = new ServiceCollection();
            ConfigureServices(services);
            await services
                .AddSingleton<MainGenerator, MainGenerator>()
                .BuildServiceProvider()
                .GetService<MainGenerator>()
                .Generate();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<ICountryGenerator, CountryGenerator>()
                .AddSingleton<IFlairGenerator,FlairGenerator>()
                .AddSingleton<IGambitGenerator, GambitGenerator>()
                .AddSingleton<IWikiGenerator, WikiGenerator>()
                .AddSingleton<IOpeningGenerator, OpeningGenerator>()
                .AddSingleton<ITranslationGenerator, TranslationGenerator>()
                .AddSingleton<IPuzzleGenerator, PuzzleGenerator>()
                .AddSingleton<IIconGenerator, IconGenerator>()

                .AddSingleton<IChessManager,ChessManager>()
                .AddSingleton<ICrowdinDownloader, CrowdinDownloader>()
                .AddSingleton<ILowerCaseEncoder, LowerCaseEncoder>()

                .AddLogging(logging =>
                {
                    logging.AddCustomFormatter(_ => { });
                });
        }
    }
}