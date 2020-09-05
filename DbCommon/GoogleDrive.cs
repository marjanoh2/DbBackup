using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentResults;

namespace DbCommon
{
   public class GoogleDrive
    {
        //private void Authorize()
        //{
        //    string[] scopes = new string[] { DriveService.Scope.Drive,
        //                       DriveService.Scope.DriveFile,};
        //    var clientId = "12345678-kiwwjelkrklsjdkljklaflkjsdjasdkhw.apps.googleusercontent.com";      // From https://console.developers.google.com  
        //    var clientSecret = "ksdklfklas2lskj_asdklfjaskla-";          // From https://console.developers.google.com  
        //                                                                 // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%  
        //    var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
        //    {
        //        ClientId = clientId,
        //        ClientSecret = clientSecret
        //    }, scopes,
        //    Environment.UserName, CancellationToken.None, new FileDataStore("MyAppsToken")).Result;
        //    //Once consent is recieved, your token will be stored locally on the AppData directory, so that next time you wont be prompted for consent.   

        //    DriveService service = new DriveService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential,
        //        ApplicationName = "MyAppName",

        //    });
        //    service.HttpClient.Timeout = TimeSpan.FromMinutes(100);
        //    //Long Operations like file uploads might timeout. 100 is just precautionary value, can be set to any reasonable value depending on what you use your service for  

        //    // team drive root https://drive.google.com/drive/folders/0AAE83zjNwK-GUk9PVA   

        //    var respocne = uploadFile(service, "textBox1.Text", "");
        //    // Third parameter is empty it means it would upload to root directory, if you want to upload under a folder, pass folder's id here.
        //    //MessageBox.Show("Process completed--- Response--" + respocne);
        //}
        public Result Upload(string _uploadFile)
        {
            string serviceAccount = "bekap-846@pelagic-bison-124219.iam.gserviceaccount.com";
            string saCredentialFile = @"C:\pelagic-bison-124219-f195de19548f.json";

            DriveService service = AuthenticateServiceAccount(serviceAccount, saCredentialFile);
            var response = uploadFile(service, _uploadFile, "1-ISVKGeCTNtZL4dvOXTiTOp2SaAXVy-q"); //1-ISVKGeCTNtZL4dvOXTiTOp2SaAXVy-a
            
            if (response!=null)
            { return Results.Ok(); }
            else
            { return Results.Fail("Error with uploading to Google drive"); }

            
        }
        public Google.Apis.Drive.v3.Data.File uploadFile(DriveService _service, string _uploadFile, string _parent, string _descrp = "Uploaded with .NET!")
        {
            if (System.IO.File.Exists(_uploadFile))
            {
                Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
                body.Name = System.IO.Path.GetFileName(_uploadFile);
                body.Description = _descrp;
                body.MimeType = GetMimeType(_uploadFile);
                body.Parents = new List<string> { _parent };// UN comment if you want to upload to a folder(ID of parent folder need to be send as paramter in above method)
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, GetMimeType(_uploadFile));
                    request.SupportsTeamDrives = true;
                    // You can bind event handler with progress changed event and response recieved(completed event)
                    request.ProgressChanged += Request_ProgressChanged;
                    request.ResponseReceived += Request_ResponseReceived;
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.Message, "Error Occured");
                    return null;
                }
            }
            else
            {
                //MessageBox.Show("The file does not exist.", "404");
                return null;
            }
        }
        private static string GetMimeType(string fileName) {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            System.Diagnostics.Debug.WriteLine(mimeType);
            return mimeType; }

        private void Request_ProgressChanged(Google.Apis.Upload.IUploadProgress obj)
        {
            //textBox2.Text += obj.Status + " " + obj.BytesSent;
        }

        private void Request_ResponseReceived(Google.Apis.Drive.v3.Data.File obj)
        {
            if (obj != null)
            {
                //MessageBox.Show("File was uploaded sucessfully--" + obj.Id);
            }
        }

        public static DriveService AuthenticateServiceAccount(string serviceAccountEmail, string serviceAccountCredentialFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceAccountCredentialFilePath))
                    throw new Exception("Path to the service account credentials file is required.");
                if (!File.Exists(serviceAccountCredentialFilePath))
                    throw new Exception("The service account credentials file does not exist at: " + serviceAccountCredentialFilePath);
                if (string.IsNullOrEmpty(serviceAccountEmail))
                    throw new Exception("ServiceAccountEmail is required.");

                // These are the scopes of permissions you need. It is best to request only what you need and not all of them
                string[] scopes = { DriveService.Scope.Drive }; ;// = new string[] { AnalyticsReportingService.Scope.Analytics };             // View your Google Analytics data

                // For Json file
                if (Path.GetExtension(serviceAccountCredentialFilePath).ToLower() == ".json")
                {
                    GoogleCredential credential;
                    using (var stream = new FileStream(serviceAccountCredentialFilePath, FileMode.Open, FileAccess.Read))
                    {
                        credential = GoogleCredential.FromStream(stream)
                             .CreateScoped(scopes);
                    }

                    // Create the  Analytics service.
                    return new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Drive Service account Authentication Sample",
                    });
                }
                else if (Path.GetExtension(serviceAccountCredentialFilePath).ToLower() == ".p12")
                {   // If its a P12 file

                    var certificate = new X509Certificate2(serviceAccountCredentialFilePath, "notasecret", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                    var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
                    {
                        Scopes = scopes
                    }.FromCertificate(certificate));

                    // Create the  Drive service.
                    return new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Drive Authentication Sample",
                    });
                }
                else
                {
                    throw new Exception("Unsupported Service accounts credentials.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Create service account DriveService failed" + ex.Message);
                throw new Exception("CreateServiceAccountDriveFailed", ex);
            }
        }
    }
}
