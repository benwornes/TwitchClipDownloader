using System;
using System.Threading.Tasks;
using Downloader;

namespace ClipDownloader
{
    class Program
    {
        private static string clientID = "728pqj9uirwa88hhh1caqtjaxltk29";
        private static string authentication = "j2v5p4ieenr81mt4hanhqsbtltvgws";
        async static Task Main(string[] args)
        {
            Console.WriteLine("Enter your output path");
            string outputPath = Console.ReadLine();

            Download download = new Download(clientID, authentication, outputPath);

            await download.StartDownload();
        }
    }
}
