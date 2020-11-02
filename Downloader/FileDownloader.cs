using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Downloader
{
    public class FileDownloader
    {
        private static Data data;
        private static List<Task> tasks;
        public async static Task DownloadAllFiles(Data clientData)
        {
            data = clientData;

            data.OutputFolderExists();


            // Deserialize .json file and get ClipInfo list
            List<ClipInfo> clips = JsonConvert.DeserializeObject<List<ClipInfo>>(File.ReadAllText(data.JsonFile));

            tasks = new List<Task>();

            foreach (ClipInfo clip in clips)
            {
                tasks.Add(DownloadFilesAsync(clip));
            }

            await Task.WhenAll(tasks);
        }

        private static void GetData()
        {
            if (data.ClientID == null)
            {
                data.GetClientID();
            }
            if (data.Authentication == null)
            {
                data.GetAuthentication();
            }
            if (data.QueryURL == null)
            {
                data.GetQuery();
            }
        }

        private static string GetClipURL(ClipInfo clip)
        {
            // Example thumbnail URL:
            // https://clips-media-assets2.twitch.tv/AT-cm%7C902106752-preview-480x272.jpg
            // You can get the URL of the location of clip.mp4
            // by removing the -preview.... from the thumbnail url */

            string url = clip.ThumbnailURL;
            url = url.Substring(0, url.IndexOf("-preview")) + ".mp4";
            return url;
        }
                
        private async static Task DownloadFilesAsync(ClipInfo clip)
        {
            WebClient client = new WebClient();
            string url = GetClipURL(clip);
            string filepath = data.OutputPath + clip.Id + ".mp4";

            await client.DownloadFileTaskAsync(new Uri(url), filepath);
        }

        private static void FileDownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            tasks.Remove((Task)sender);
        }
    }
}
