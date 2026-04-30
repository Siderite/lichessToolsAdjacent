namespace AssetGenerator.Interfaces
{
    /// <summary>
    /// Validate piece sets
    /// </summary>
    public interface IPieceSetValidator
    {
        /// <summary>
        /// Checks if piece sets have all pieces available or are duplicated
        /// </summary>
        /// <returns></returns>
        Task Validate();
    }
}