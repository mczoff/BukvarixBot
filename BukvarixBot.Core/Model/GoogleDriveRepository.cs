using BukvarixBot.Core.Abstractions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace BukvarixBot.Core.Model
{
    public class GoogleDriveRepository
        : IGoogleDriveRepository<string>
    {
        readonly string[] Scopes = { DriveService.Scope.Drive };
        readonly string ApplicationName = "Bukvarix Storage";
        readonly string _tokenPath = "token";

        readonly UserCredential _credential;
        readonly DriveService _driveService;

        string _folderId;

        public GoogleDriveRepository(string credentialsPath)
        {
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "User",
                        CancellationToken.None,
                        new FileDataStore(_tokenPath, true)).Result;
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = ApplicationName,
            });
        }

        public void CreateCSVFolder(string path)
        {
            var fileMetadata = new GoogleFile()
            {
                Name = path,
                MimeType = "application/vnd.google-apps.folder"
            };

            var request = _driveService.Files.Create(fileMetadata);
            request.Fields = "id";

            var file = request.Execute();
            _folderId = file.Id;

        }

        public void CreateCSVFile(string path, string name)
        {
            if (name == null)
                throw new ArgumentNullException("Google file name cant be null");

            GoogleFile googleFile = new GoogleFile
            {
                Name = name,
                Parents = new List<string> { _folderId },
                MimeType = "application/vnd.google-apps.spreadsheet"
            };

            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream(path, System.IO.FileMode.Open))
            {
                request = _driveService.Files.Create(
                    googleFile, stream, "text/csv");

                request.Fields = "id";
                request.Upload();
            }
        }
    }
}
