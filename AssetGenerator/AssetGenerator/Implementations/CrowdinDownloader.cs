using AssetGenerator.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AssetGenerator.Implementations
{
    /// <summary>
    /// Implements the ICrowdinDownloader interface to download Crowdin data using their API
    /// </summary>
    public class CrowdinDownloader(ILogger<CrowdinDownloader> logger)
        : ICrowdinDownloader
    {
        private readonly string _baseUrl = "https://api.crowdin.com/api/v2";

        /// <summary>
        /// Download LiChess Tools bundle and save it in provided path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task DownloadBundle(string path)
        {
            var personalAccessToken = Environment.GetEnvironmentVariable("CROWDIN_TOKEN") 
                ?? throw new Exception("CROWDIN_TOKEN environment variable is not set");
            await DownloadBundleAsync("595763", "2", personalAccessToken, path);
        }


        private async Task DownloadBundleAsync(
                                        string projectId,
                                        string bundleId,
                                        string personalAccessToken,
                                        string outputZipPath)
        {
            logger.LogInformation("Starting Crowdin bundle download for project {projectId} and bundle {bundleId}...", projectId, bundleId);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", personalAccessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var exportUrl = $"{_baseUrl}/projects/{projectId}/bundles/{bundleId}/exports";

                // 1. Start export (POST)
                using var postContent = new StringContent("{}", Encoding.UTF8, "application/json");
                var postResponse = await client.PostAsync(exportUrl, postContent);
                postResponse.EnsureSuccessStatusCode();
                var json = await postResponse.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<BundleExportResponse>(json);
                var exportId = result?.Data.Id ?? throw new Exception("Failed to start bundle export.");

                exportUrl = $"{_baseUrl}/projects/{projectId}/bundles/{bundleId}/exports/{exportId}";

                // 2. Poll until ready (usually takes a few seconds)
                while (true)
                {
                    var statusResponse = await client.GetAsync(exportUrl);
                    statusResponse.EnsureSuccessStatusCode();

                    json = await statusResponse.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<BundleExportResponse>(json);

                    if (result?.Data.Status == "finished")
                    {
                        var downloadUrl = $"{_baseUrl}/projects/{projectId}/bundles/{bundleId}/exports/{exportId}/download";
                        statusResponse = await client.GetAsync(downloadUrl);
                        statusResponse.EnsureSuccessStatusCode();

                        json = await statusResponse.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<BundleExportResponse>(json);
                        if (string.IsNullOrWhiteSpace(result?.Data.Url)) throw new Exception("Could not get the download URL");

                        // 3. Download the ZIP
                        using (var downloadClient = new HttpClient())
                        {
                            var zipBytes = await DownloadZipAsync(result.Data.Url);
                            await File.WriteAllBytesAsync(outputZipPath, zipBytes);
                        }
                        logger.LogInformation("Bundle downloaded: {outputZipPath}", outputZipPath);
                        return;
                    }

                    if (result?.Data.Status == "failed")
                        throw new Exception("Bundle export failed on Crowdin side.");

                    await Task.Delay(3000); // Poll every 3 seconds
                }
            }

        }

        private static async Task<byte[]> DownloadZipAsync(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var client = new HttpClient();
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        // Minimal models (Crowdin wraps responses in "data")
        private class BundleExportResponse
        {
            [JsonProperty("data")]
            public BundleExport Data { get; set; } = new();
        }

        private class BundleExport
        {
            [JsonProperty("identifier")]
            public string Id { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; } = string.Empty; // "inProgress", "finished", "failed"

            [JsonProperty("url")]
            public string? Url { get; set; }
        }
    }

}