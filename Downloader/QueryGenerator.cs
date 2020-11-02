using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Downloader
{
    public class QueryGenerator
    {
        private static string apiURL = "https://api.twitch.tv/helix/";
        private static Data data;
        private static string idQuery = null;
        private static string numClipsQuery = null;

        public async static Task Generate(Data clientData)
        {
            data = clientData;
            string response = null;


            // Loops until either G or C is chosen by user
            while (response != "G" && response != "C")
            {
                Console.Clear();
                Console.WriteLine("Would you like to find the top clips of:");
                Console.WriteLine("A Game - G");
                Console.WriteLine("A Channel - C");

                response = Console.ReadLine().ToUpper();
            }


            switch (response)
            {
                case "G":
                    await GetGameID();
                    break;
                case "C":
                    await GetChannelID();
                    break;
                default:
                    break;
            }

            GetNumClipsQuery();

            data.QueryURL = apiURL + "clips?" + idQuery + numClipsQuery;
        }
        /// <summary>
        /// Generates QueryGenerator.idQuery from gameName input via Console
        /// </summary>
        private async static Task GetGameID()
        {
            Console.Write("Please enter the game name: ");
            string gameName = Console.ReadLine();
            gameName = gameName.Replace(" ", "%20");


            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, apiURL + "games?name=" + gameName);
            requestMessage.Headers.Add("client-id", data.ClientID);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", data.Authentication);

            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);

            // Converts response into Root<GameInfo> object
            // which contains List<GameInfo> and Pagination
            string responseContnet = await responseMessage.Content.ReadAsStringAsync();
            Root<GameInfo> gameInfo = JsonConvert.DeserializeObject<Root<GameInfo>>(responseContnet);

            // gameInfo.Data is a List<GameInfo>
            // There is only ever going to be one item in
            // this list so we take the 0th index
            idQuery = "game_id=" + gameInfo.Data[0].Id;
        }

        /// <summary>
        /// Generates QueryGenerator.idQuery from channelName input via Console
        /// </summary>
        private async static Task GetChannelID()
        {
            Console.Write("Please enter the name of the streamer: ");
            string channelName = Console.ReadLine();

            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, apiURL + "users?login=" + channelName);
            requestMessage.Headers.Add("client-id", data.ClientID);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", data.Authentication);

            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);

            string responseContent = await responseMessage.Content.ReadAsStringAsync();
            Root<ChannelInfo> channelInfo = JsonConvert.DeserializeObject<Root<ChannelInfo>>(responseContent);

            // channelInfo.Data is a List<ChannelInfo>
            // There is only ever going to be one item in
            // this list so we take the 0th index
            idQuery = "broadcaster_id=" + channelInfo.Data[0].Id;
        }

        private static void GetNumClipsQuery()
        {
            int numClips = 0;

            Console.Clear();
            Console.WriteLine("How many clips would you like to download? (1-100 inclusive)");
            numClips = Convert.ToInt32(Console.ReadLine());

            if (numClips < 1 || numClips > 100)
            {
                GetNumClipsQuery();
                return;
            }

            numClipsQuery = "&first=" + numClips.ToString();
        }
    }
}
