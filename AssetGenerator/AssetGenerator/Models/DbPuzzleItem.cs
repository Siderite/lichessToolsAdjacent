using AssetGenerator.Extensions;
using LiteDB;
using Newtonsoft.Json;

namespace AssetGenerator.Models
{
    /// <summary>
    /// Used to store a PuzzleItem in the database, with compressed data to save space
    /// </summary>
    public class DbPuzzleItem
    {
        /// <summary>
        /// Unique case insensitive key for the puzzle item
        /// </summary>
        [BsonId]
        public string Key { get; set; }

        /// <summary>
        /// Compresses JSON data
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Get a DbPuzzleItem from a PuzzleItem, compressing the data to save space in the database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DbPuzzleItem FromPuzzleItem(string key, PuzzleItem obj)
        {
            var data = JsonConvert.SerializeObject(obj).Compress();
            return new DbPuzzleItem
            {
                Key = key,
                Data = data
            };
        }

        /// <summary>
        /// Get a PuzzleItem from the compressed data in the database, decompressing it and deserializing it from JSON
        /// </summary>
        /// <returns></returns>
        public PuzzleItem ToPuzzleItem()
        {
            var obj = JsonConvert.DeserializeObject<PuzzleItem>(Data.Decompress());
            return obj;
        }
    }
}
