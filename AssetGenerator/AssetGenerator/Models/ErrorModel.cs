namespace AssetGenerator.Models
{
    /// <summary>
    /// Used to deserialize error responses from the Wikibooks API, 
    /// containing an error code and additional information about the error.
    /// </summary>
    public class ErrorModel
    {
        /// <summary>
        /// Error code
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// Error info
        /// </summary>
        public string info { get; set; }
    }


}
