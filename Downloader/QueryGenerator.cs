﻿using Newtonsoft.Json;
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

        public async static Task Generate(Data clientData)
        {
            data = clientData;
            string response = null;
            bool validResponse = false;
            string idQuery = null;

            while (!validResponse)
            {
                validResponse = true;

                Console.Clear();
                Console.WriteLine("Would you like to find the top clips of:");
                Console.WriteLine("A Game - G");
                Console.WriteLine("A Channel - C");

                response = Console.ReadLine().ToUpper();

                switch (response)
                {
                    case "G":
                        idQuery = "game_id=" + await GetGameID();
                        break;
                    case "C":
                        break;
                    default:
                        validResponse = false;
                        break;
                }
            }

            data.QueryURL = apiURL + "clips?" + idQuery;
        }

        private async static Task<string> GetGameID()
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

            // Only one item is ever goign to be in the List<GameInfo>
            // so we take the 0th index
            return gameInfo.Data[0].id;
        }
    }
}
