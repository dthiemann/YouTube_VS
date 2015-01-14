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
    internal class MyUploads {
        [STAThread]
        static void Main(string[] args) {
            Console.WriteLine("YouTube Data API: My Uploads");
            Console.WriteLine("============================");

            try {
                //new MyUploads().Uploads().Wait();
                new MyUploads().Search().Wait();
            }
            catch (AggregateException ex) {
                foreach (var e in ex.InnerExceptions) {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task Uploads() {
            UserCredential credential;
            using (var stream = new FileStream("client_secret_test.json", FileMode.Open, FileAccess.Read)) {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                    // user's account, but not other types of account access.
                    new[] { YouTubeService.Scope.YoutubeReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer() {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });

            var channelsListRequest = youtubeService.Channels.List("contentDetails");
            channelsListRequest.Mine = true;

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var channelsListResponse = await channelsListRequest.ExecuteAsync();

            foreach (var channel in channelsListResponse.Items) {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.
                var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                Console.WriteLine("Videos in list {0}", uploadsListId);

                var nextPageToken = "";
                while (nextPageToken != null) {
                    var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                    playlistItemsListRequest.PlaylistId = uploadsListId;
                    playlistItemsListRequest.MaxResults = 50;
                    playlistItemsListRequest.PageToken = nextPageToken;

                    // Retrieve the list of videos uploaded to the authenticated user's channel.
                    var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                    foreach (var playlistItem in playlistItemsListResponse.Items) {
                        // Print information about each video.
                        Console.WriteLine("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
                    }

                    nextPageToken = playlistItemsListResponse.NextPageToken;
                }
            }
        }

        private async Task Search()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                // iOS ApiKey because I can't find how to do it with Window's Phones
                ApiKey = "AIzaSyAj45pQlT6IRJlTmt8ykUUbZPhONmnwvgo",
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = "Achievement Hunter"; // This is the search term
            searchListRequest.MaxResults = 50;

            //Call the search.list method to retrieve results matching specified query term
            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<string> videos = new List<string>();
            List<string> channels = new List<string>();
            List<string> playlists = new List<string>();

            // Add each result to the appropriate list and then display the list of
            // matching videos, channels, and playlists
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                        break;
                    case "youtube#channel":
                        channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                        break;
                    case "youtube#playlist":
                        playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                        break;
                }
            }
            Console.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos)));
            Console.WriteLine(String.Format("Channels:\n{0}\n", string.Join("\n", channels)));
            Console.WriteLine(String.Format("Playlists:\n{0}\n", string.Join("\n", playlists)));
        }
    }
}

