namespace AssetGenerator.Models
{
    /// <summary>
    /// Data model representing the structure of the response from the Wikibooks API 
    /// when querying for chess opening theory pages. It contains a list of PageModel objects, 
    /// each representing a search result from the API query.
    /// </summary>
    public class QueryModel
    {
        /// <summary>
        /// Search results
        /// </summary>
        public List<PageModel> search { get; set; }
    }


}
