using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Net;
using System.Threading;

namespace Downloader
{
    public class Download
    {
        //private static string directory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName; // ..\ClipDownloader
        private static string directory = Directory.GetCurrentDirectory();

        private string clientID = null;
        private string authentication = null;

        private static string defaultJsonFile = directory + @"\clips.json"; // ..\TwitchDownloader\clips.json
        private string jsonFile;

        private static string twitchDownloaderExe = directory + @"\TwitchDownloader\TwitchDownloaderCLI.exe"; // ..\TwitchDownloader\TwitchDownloader\TwitchDownloaderCLI.exe

        //static string outputPath = "f:\\Users\\ben\\Documents\\Programming\\TwitchClipsDownloaderBot\\Clips";
        private static string defaultOutputPath = directory + @"\Clips\"; // ..\TwitchDownloader\Clips
        private string outputPath;

        private static string clipsLink = "https://api.twitch.tv/helix/clips?";

        private string url;

        public Download()
        {
            this.outputPath = defaultOutputPath;
            this.jsonFile = defaultJsonFile;
        }
        public Download(string clientID, string authentication)
        {
            this.clientID = clientID;
            this.outputPath = defaultOutputPath;
            this.jsonFile = defaultJsonFile;
        }
        public Download(string clientID, string authentication, string outputPath)
        {
            this.clientID = clientID;
            this.authentication = authentication;
            this.outputPath = directory + @"\" + outputPath + @"\";
            this.jsonFile = this.outputPath + @"\" + outputPath + ".json";
        }

        public async Task StartDownload()
        {

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            if (clientID == null)
            {
                GetClientID();
            }
            if (authentication == null)
            {
                GetAuthentication();
            }
            if (url == null)
            {
                GetQueries();
            }


            bool authError = true; // Has to execute at least once
            // Executes until the GetHttpResponse goes through
            while (authError)
            {
                authError = false;
                try
                {
                    string responseContent = await GetHttpResponse();
                    GenerateJson(responseContent);
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Invalid authentication, please enter client-ID and authentication again!");
                    GetClientID();
                    GetAuthentication();

                    authError = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    authError = true;
                }
            }


            Console.WriteLine(outputPath);


            DownloadClips();
        }

        public void GetQueries()
        {
            Console.Write("Please enter your query: ");
            string query = Console.ReadLine();

            url = clipsLink + query;
        }

        private async Task<string> GetHttpResponse()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            requestMessage.Headers.Add("client-id", clientID);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authentication);

            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);

            string responseContent = await responseMessage.Content.ReadAsStringAsync();

            return responseContent;
        }

        private void GenerateJson(string responseContent)
        {
            Root responseResult = JsonConvert.DeserializeObject<Root>(responseContent);

            if (!File.Exists(jsonFile)) // If the file doesn't exist, we need to create it and add a '[' at the start
            {
                FileStream file = File.Create(jsonFile);
                file.Close();
                File.AppendAllText(jsonFile, "[\n"); // The array of json objects needs to be wrapped inside []
            }
            else
            {
                string[] jsonLines = File.ReadAllLines(jsonFile);
                File.WriteAllLines(jsonFile, jsonLines.Take(jsonLines.Length - 1).ToArray());
                File.AppendAllText(jsonFile, ","); // The last json object won't have a comma at the end of it, so we need to add it now, before we add more objects
            }

            if (File.ReadAllText(jsonFile).Length == 0 || File.ReadAllText(jsonFile)[0] != '[')
            {
                File.WriteAllText(jsonFile, "[\n" + File.ReadAllText(jsonFile)); // If the file already exists, but there was no [ at the start for whatever reason, we need to add it
            }

            string json;

            for (int i = 0; i < responseResult.data.Count; i++)
            {
                json = JsonConvert.SerializeObject(responseResult.data[i]);
                if (i == responseResult.data.Count - 1)
                {
                    File.AppendAllText(jsonFile, json + "\n"); // No comma needed for the last object entry
                    continue;
                }
                else
                {
                    File.AppendAllText(jsonFile, json + ",\n");
                }
            }

            File.AppendAllText(jsonFile, "]"); // Adds the ] at the end of the file to close of the json objects array
        }

        private void GetClientID()
        {
            Console.WriteLine("Enter your client ID");
            clientID = Console.ReadLine();
        }

        private void GetAuthentication()
        {
            Console.WriteLine("Enter your Authentication");
            authentication = Console.ReadLine();
        }

        private void DownloadClips()
        {
            List<ClipInfo> clips = JsonConvert.DeserializeObject<List<ClipInfo>>(File.ReadAllText(jsonFile));

            WebClient client;

            for (int i = 0; i < clips.Count; i++)
            {
                client = new WebClient();
                string url = GetClipURL(clips[i]);
                client.DownloadFileAsync(new Uri(url), String.Format("{0}{1}.mp4", outputPath, clips[i].id)); // Downloads files Asynchronously
            }

            Thread.Sleep(150*1000);
        }

        private string GetClipURL(ClipInfo clip)
        {
            /*
             * Example thumbnail URL - https://clips-media-assets2.twitch.tv/AT-cm%7C902106752-preview-480x272.jpg
             * You can get the URL of the location of the .mp4 by removing the -preview.... from the thumbnail url */

            string url = clip.thumbnail_url;
            url = url.Substring(0, url.IndexOf("-preview")) + ".mp4";
            return url;
        }
    }
}
