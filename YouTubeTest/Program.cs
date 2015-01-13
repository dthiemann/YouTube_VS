using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        private async Task Run()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] {YouTubeService.Scope.YoutubeReadonly},
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString()));
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });

            var channelsListRequest = youtubeService.Channels.List("contentDetails");
            channelsListRequest.Mine = true;

            // Retrieve the conetnetDetails part of the channel resource for the authenticated user's channel
            var channelsListResponse = await channelsListRequest.ExecuteAsync();

            foreach (var channel in channelsListResponse.Items)
            {
                // From the API response, extract the playlist ID that indentifies
                // the list of videos uploaded to the authenticated user's channel
                var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                Console.WriteLine("Videos in list {0}", uploadsListId);

                var nextPageToken = "";
                while (nextPageToken != null)
                {
                    var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                    playlistItemsListRequest.PlaylistId = uploadsListId;
                    playlistItemsListRequest.MaxResults = 50;
                    playlistItemsListRequest.PageToken = nextPageToken;
                }
            }
        }
    }
}
