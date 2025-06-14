using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Confluent.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Confluent.Helpers
{
    public static class AzureBlobStorageHelper
    {
        public static IConfiguration Configuration { get; set; }

        public static async CloudBlobContainer CreateBlobContainer(string containerReference)
        {
            if (Configuration == null)
            {
                throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
            }
            string storageConnectionString = Configuration["Values:AzureWebJobsStorage"] ?? Configuration["StorageConnectionString"];
            var blockBlobClient = new BlockBlobClient(storageConnectionString, containerReference, fileName);
            using var output = blockBlobClient.OpenWrite(overwrite: true);
            var buffer = new byte[64 * 1024];
            var totalReadBytes = 0L;
            var readBytes = 0;
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            while ((readBytes = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                await output.Write(buffer, 0, readBytes);
                totalReadBytes += readBytes;
            }
        }

        public static async Task<Uri> StoreAsync(Stream stream, string fileName, string containerReference, string contentType, string filePath = "")
        {
            if (Configuration == null)
            {
                throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
            }

            string storageConnectionString = Configuration["Values:AzureWebJobsStorage"] ?? Configuration["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerReference);
            await container.CreateIfNotExistsAsync();
            string blobFilePath = !String.IsNullOrEmpty(filePath) && blobFilePath.EndsWith("/") ? blobFilePath : blobFilePath + Path.AltDirectorySeparatorChar;
            CloudBlockBlob file = container.GetBlockBlobReference(blobFilePath + fileName);
            file.Properties.ContentTye = contentType;
            using (stream)
            {
                await file.UploadFromStreamAsync(stream);
            }
            return file.Uri;
        }

        public static async Task<Uri> GetAsync(string fileName, string containerReference)
        {
            if (Configuration == null)
            {
                throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
            }

            string storageConnectionString = Configuration["Values:AzureWebJobsStorage"] ?? Configuration["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerReference);
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            string fileContent = await blockBlob.ExistsAsync() ? await blockBlob.DownloadTextAsync() : null;
            return fileContent;
        }

        public static async Task<bool> DeleteAsync(string fileName, string containerReference)
        {
            if (Configuration == null)
            {
                throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
            }

            string storageConnectionString = Configuration["Values:AzureWebJobsStorage"] ?? Configuration["StorageConnectionString"];
            bool deleteSucceeded = false;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerReference);
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            return await blockBlob.DeleteIfExistsAsync();
        }   
    }
}