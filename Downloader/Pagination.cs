using Newtonsoft.Json;

namespace Downloader
{
    public class Pagination
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }

}
