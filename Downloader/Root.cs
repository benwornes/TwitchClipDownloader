using System.Collections.Generic;

namespace Downloader
{
    public class Root
    {
        public List<ClipInfo> data { get; set; }
        public Pagination pagination { get; set; }
    }
}
