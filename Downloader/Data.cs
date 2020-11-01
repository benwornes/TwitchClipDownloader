using System;
using System.IO;

namespace Downloader
{
    public class Data
    {
        private static string directory = Directory.GetCurrentDirectory();
        private readonly static string defaultJsonFile = directory + @"\clips.json";
        private readonly static string defaultOutputPath = directory + @"\Clips\";
        private readonly static string clipsLink = "https://api.twitch.tv/helix/clips?";

        public string OutputPath { get; set; }
        public string JsonFile { get; set; }
        public string ClientID { get; private set; }
        public string Authentication { get; private set; }
        public string QueryURL { get; private set; }
        

        public Data()
        {
            OutputPath = defaultOutputPath;
            JsonFile = defaultJsonFile;
        }
        public Data(string clientID, string authentication)
        {
            ClientID = clientID;
            Authentication = authentication;
            OutputPath = defaultOutputPath;
            JsonFile = defaultJsonFile;
        }
        public Data(string clientID, string authentication, string outputPath)
        {
            ClientID = clientID;
            Authentication = authentication;
            OutputPath = directory + @"\" + outputPath + @"\";
            JsonFile = OutputPath + outputPath + ".json";
        }

        public void GetQuery()
        {
            Console.Write("Please enter your query: ");
            QueryURL = clipsLink + Console.ReadLine();
        }

        public void GetClientID()
        {
            Console.WriteLine("Enter your client ID");
            ClientID = Console.ReadLine();
        }

        public void GetAuthentication()
        {
            Console.WriteLine("Enter your Authentication");
            Authentication = Console.ReadLine();
        }

        public void OutputFolderExists()
        {
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }
        }

    }
}