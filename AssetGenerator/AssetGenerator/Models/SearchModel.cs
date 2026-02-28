namespace AssetGenerator.Models
{
    /// <summary>
    /// Data model representing the response from a search query to the Wikibooks API
    /// </summary>
    public class SearchModel
    {
        /// <summary>
        /// Information about the continuation of the search query
        /// </summary>
        public ContinueModel @continue { get; set; }

        /// <summary>
        /// Query information, including the search results and metadata about the search
        /// </summary>
        public QueryModel query { get; set; }

        /// <summary>
        /// Error information, if any error occurred during the search query
        /// </summary>
        public ErrorModel error { get; set; }
    }


}
