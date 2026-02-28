namespace AssetGenerator.Models
{
    /// <summary>
    /// Used for JSON deserialization of the "continue" field in the Wikibooks API response, 
    /// which indicates the offset for the next set of results when paginating through search results. 
    /// </summary>
    public class ContinueModel
    {
        /// <summary>
        /// Indicates the offset for the next set of results when paginating through search 
        /// results in the Wikibooks API response.
        /// </summary>
        public int sroffset { get; set; }

        /// <summary>
        /// Indicates that there are more results to be fetched from the Wikibooks API 
        /// and provides the necessary information to continue the pagination process.
        /// </summary>
        public string @continue { get; set; }
    }
}
