namespace WebApplication1.Pages
{
    using Azure.Identity;
    using Azure.Storage;
    using Azure.Storage.Blobs;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string Message { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            ////Uri serviceUri = new Uri("https://gratefullazurestorage.blob.core.windows.net/");
            ////var credential = new DefaultAzureCredential();
            ////BlobServiceClient blobServiceClient = new BlobServiceClient(serviceUri, credential);
        }

        [HttpPost]
        public async Task<IActionResult> OnPostUploadFilesAsync(List<IFormFile> files)
        {
            //////////////////////////////////////////
            /// Shared Key Authentication
            /// 
            var containerName = "gratefull-taughts";

            /// Connect using connection string (in portal: Settings -> Access Keys -> Connection String)
            //var connectionString = "DefaultEndpointsProtocol=https;AccountName=gratefullazurestorage;AccountKey=lUTgcTR26ANILPW3dmQHt/zttkxhM+dlUZtb1PJdijLGZ1jx+JIetU6H/AJVbejrYAZp+XA/XdHyVEVnAkS/Ig==;EndpointSuffix=core.windows.net";
            //BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            /// Connect using Storage Account Key (in portal: Settings -> Access Keys -> Key)
            ////string accountName = "gratefullazurestorage";
            ////string accountKey = "lUTgcTR26ANILPW3dmQHt/zttkxhM+dlUZtb1PJdijLGZ1jx+JIetU6H/AJVbejrYAZp+XA/XdHyVEVnAkS/Ig==";
            /////// Blob service URL can be found in portal: Settings -> Properties -> Blob Service
            ////Uri serviceUri = new Uri("https://gratefullazurestorage.blob.core.windows.net/");
            ////StorageSharedKeyCredential credential = new StorageSharedKeyCredential(accountName, accountKey);
            ////BlobServiceClient blobServiceClient = new BlobServiceClient(serviceUri, credential);

            //////////////////////////////////////////
            /// Shared Access Signature (SAS) Authentication
            /// 
            ////var sasURL = "https://gratefullazurestorage.blob.core.windows.net/gratefull-taughts?sp=racwdl&st=2021-09-10T09:38:48Z&se=2021-09-10T17:38:48Z&spr=https&sv=2020-08-04&sr=c&sig=UzhaShsBPdB75%2Bc4TTcpU%2BPP8tb8mLAhuYb8sGZDVMY%3D";
            ////UriBuilder sasUri = new UriBuilder(sasURL);
            ////BlobServiceClient blobServiceClient = new BlobServiceClient(sasUri.Uri);

            /// Connect using Managed Identity for Azure Resources
            var credential = new DefaultAzureCredential();
            Uri serviceUri = new Uri("https://gratefullazurestorage.blob.core.windows.net/");
            BlobServiceClient blobServiceClient = new BlobServiceClient(serviceUri, credential);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            long size = files.Sum(f => f.Length);

            // full path to file in temp location (buffer file locally before uploading).
            // note: for larger files and workloads, consider streaming instead.
            var filePath = Path.GetTempFileName();

            int uploadFileCount = 0;

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    // create a local reference to a blob
                    BlobClient blobClient = containerClient.GetBlobClient(Path.GetFileName(formFile.FileName));

                    using (var stream = formFile.OpenReadStream())
                    {
                        // upload the blob to Azure Storage
                        await blobClient.UploadAsync(stream, true);
                        uploadFileCount++;
                    }
                }
            }

            Message = "Number of files uploaded to Azure Storage: " + uploadFileCount;
            return Page();

        }
    }
}
