using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using File = Google.Apis.Drive.v2.Data.File;


namespace DriveQuickstart
{
    class Program
    {

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";

        public static List<File> retrieveAllFiles(DriveService service)
        {
            List<File> result = new List<File>();
            FilesResource.ListRequest request = service.Files.List();

            do
            {
                try
                {
                    FileList files = request.Execute();

                    result.AddRange(files.Items);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));
            return result;
        }

        public static Boolean downloadFile(DriveService _service, File _fileResource, string _saveTo)
        {

            if (!String.IsNullOrEmpty(_fileResource.DownloadUrl))
            {
                try
                {
                    var x = _service.HttpClient.GetByteArrayAsync(_fileResource.DownloadUrl);
                    byte[] arrBytes = x.Result;
                    System.IO.File.WriteAllBytes(_saveTo, arrBytes);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return false;
                }
            }
            else
            {
                // The file doesn't have any content stored on Drive.
                return false;
            }
        }

        public static async Task<File> GetFile(string fileID)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/drive/v3/files/fileId/" + fileID);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                HttpResponseMessage response = await client.GetAsync(fileID);
                if (response.IsSuccessStatusCode)
                {
                    File file= await response.Content.ReadAsAsync<File>();
                    return file;
                }
                return null;
            }

        }
        
        static void Main(string[] args)
        {
            UserCredential credential;

            string savePath = Environment.CurrentDirectory;
            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.ReadWrite))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }


            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var files = retrieveAllFiles(service);

            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    var it =downloadFile(service,file, savePath + file.Title);
                }

                Console.Write("Finished Migrations");
            }
            else
            {
                Console.WriteLine("No files found.");
            }

            
            Console.Read();

        }
    }
}