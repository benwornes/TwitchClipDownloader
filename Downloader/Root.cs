using System.Collections.Generic;
using Newtonsoft.Json;

namespace Downloader
{
    /// <summary>
    /// Generic Root class that handles json deserialization for any type
    /// </summary>
    public class Root<T>
    {
        [JsonProperty("data")]
        public List<T> Data { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}