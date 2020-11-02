using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;


namespace Downloader
{
    public static class JsonGenerator
    {
        // This class has no constructor.
        // You call the Generate methods, passing in all required data.
        // The file will then be generated.
        private static Data data;

        /// <summary>
        /// <para>Creates .json file in OutputPath directory.</para>
        /// <para>Populates .json file with details of clips to be downloaded.</para>
        /// </summary>
        /// <param name="clientData"><see cref="Data()"/> object that should contain an OutputPath, QueryURL,
        /// ClientID and Authenticaion.</param>
        /// <returns></returns>
        public static async Task Generate(Data clientData)
        {
            data = clientData;
            string responseContent = null;

            // Loop that runs until the api request goes through
            bool authError = true;
            while (authError)
            {
                authError = false;
                try
                {
                    responseContent = await GetHttpResponse();
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Invalid authentication, please enter client-ID and authentication again!");
                    data.GetClientID();
                    data.GetAuthentication();

                    authError = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    authError = true;
                }
            }

            data.OutputFolderExists();
            GenerateJson(responseContent);
        }

        /// <summary>
        /// Generates Http request to the query URL
        /// Returns the contents of the response as a string
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetHttpResponse()
        {
            // Creating client
            HttpClient client = new HttpClient();

            if (data.QueryURL == null)
            {
                await QueryGenerator.Generate(data);
            }


            // Setting up request
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, data.QueryURL);

            // Adding Headers to request
            requestMessage.Headers.Add("client-id", data.ClientID);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", data.Authentication);

            // Receiving response to the request
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);

            // Gets the content of the response as a string
            string responseContent = await responseMessage.Content.ReadAsStringAsync();

            return responseContent;
        }

        /// <summary>
        ///  Generates or adds to the .json file that contains data on each clip
        /// </summary>
        /// <param name="responseContent"></param>
        private static void GenerateJson(string responseContent)
        {
            // Parses the data from the response to the api request
            Root<ClipInfo> responseResult = JsonConvert.DeserializeObject<Root<ClipInfo>>(responseContent);


            // If the file doesn't exist, we need to create it and add a '[' at the start
            if (!File.Exists(data.JsonFile))
            {
                FileStream file = File.Create(data.JsonFile);
                file.Close();
                // The array of json objects needs to be wrapped inside []
                File.AppendAllText(data.JsonFile, "[\n");
            }
            else
            {
                // For a pre-existing .json file, The last object won't have a comma at the
                // end of it so we need to add it now, before we add more objects
                string[] jsonLines = File.ReadAllLines(data.JsonFile);
                File.WriteAllLines(data.JsonFile, jsonLines.Take(jsonLines.Length - 1).ToArray());
                File.AppendAllText(data.JsonFile, ",");
            }

            // If the file already exists, but there was no [ at the start for whatever reason,
            // we need to add it
            if (File.ReadAllText(data.JsonFile).Length == 0 || File.ReadAllText(data.JsonFile)[0] != '[')
            {
                File.WriteAllText(data.JsonFile, "[\n" + File.ReadAllText(data.JsonFile));
            }

            string json;

            // Loops through each ClipInfo object that the api returned
            for (int i = 0; i < responseResult.Data.Count; i++)
            {
                // Serializes the ClipInfo object into a json style string
                json = JsonConvert.SerializeObject(responseResult.Data[i]);

                // Adds the serialized contents of ClipInfo to the .json file
                File.AppendAllText(data.JsonFile, json);

                if (i != responseResult.Data.Count - 1)
                {
                    // All objects except the last require a comma at the end of the
                    // object in order to correctly format the array of json objects
                    File.AppendAllText(data.JsonFile, ",");
                }

                // Adds new line after object entry
                File.AppendAllText(data.JsonFile, "\n");
            }
            // Adds the ] at the end of the file to close off the json objects array
            File.AppendAllText(data.JsonFile, "]");
        }
    }
}
