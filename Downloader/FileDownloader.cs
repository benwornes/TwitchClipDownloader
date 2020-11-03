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
            GetData();
            data.OutputFolderExists();

            // Deserialize .json file and get ClipInfo list
            List<ClipInfo> clips = JsonConvert.DeserializeObject<List<ClipInfo>>(File.ReadAllText(data.JsonFile));

            tasks = new List<Task>();

            for (int i = 0; i < clips.Count; i++)
            {
                tasks.Add(DownloadFilesAsync(clips[i], i + 1));
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
                
        private async static Task DownloadFilesAsync(ClipInfo clip, int clipNum)
        {
            WebClient client = new WebClient();
            string url = GetClipURL(clip);
            string filepath = String.Format("{0}{1}-{2}.mp4", data.OutputPath, clipNum.ToString("000"), clip.Id);

            await client.DownloadFileTaskAsync(new Uri(url), filepath);
        }

        private static void FileDownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            tasks.Remove((Task)sender);
        }
    }
}
