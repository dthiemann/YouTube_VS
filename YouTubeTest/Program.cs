using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Google Api
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeTest {
    internal class Program {
        [STAThread]
        static void Main(string[] args) {
            Console.WriteLine("YouTubeData API: My Uploads");
            Console.WriteLine("===========================");

            try
            {
                new Program().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
            Console.WriteLine("Press any key to continue...");

            //Closes the window when I hit 'Enter'
            Console.ReadKey();
        }
    }
}
