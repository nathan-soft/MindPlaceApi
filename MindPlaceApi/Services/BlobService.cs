using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MindPlaceApi.Services
{
    public interface IBlobService
    {
        Task DeleteBlobAsync(string containerName, string blobName);
        Task<BlobDownloadInfo> GetBlobAsync(string containerName, string blobName);
        Task<IEnumerable<string>> ListBlobsAsync(string containerName);
        Task<string> UploadFileBlobAsync(string containerName, Stream fileStream, string fileName);
    }

    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<BlobDownloadInfo> GetBlobAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            BlobDownloadInfo blobDownloadInfo = null;

            if (await blobClient.ExistsAsync())
            {
                blobDownloadInfo = await blobClient.DownloadAsync();
            }

            return blobDownloadInfo;
        }

        public async Task<IEnumerable<string>> ListBlobsAsync(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var items = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                items.Add(blobItem.Name);
            }

            return items;
        }


        /// <summary>
        /// Uploads a stream to azure storage and returns the url to the stream.
        /// </summary>
        /// <param name="containerName">Azure storage container.</param>
        /// <param name="fileStream">The stream/file to be uploaded.</param>
        /// <param name="fileName">The name to save the file with.</param>
        /// <returns>The url to the stream.</returns>
        public async Task<string> UploadFileBlobAsync(string containerName, Stream fileStream, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream);
            return blobClient.Uri.AbsoluteUri;
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
