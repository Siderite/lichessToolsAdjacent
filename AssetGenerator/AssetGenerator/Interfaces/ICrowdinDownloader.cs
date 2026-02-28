namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Downloads Crowdin data
    /// </summary>
    public interface ICrowdinDownloader
    {
        /// <summary>
        /// Download LiChess Tools bundle and save it in provided path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task DownloadBundle(string path);
    }
}
