using Confluent.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading.Tasks;

namespace Confluent.Helpers;

public static class AzureQueueStorageHelper
{
    public static IConfiguration Configuration { get; set; }

    public static CloudQueue InitializeQueueReference(string containerName)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
        }

        string storageConnectionString = Configuration["Values:AzureWebJobsStorage"] ?? Configuration["StorageConnectionString"];
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        CloudQueue queue = queueClient.GetQueueReference(queueName);
        return queue;
    }

    public static async Task<CloudQueue> CreateQueueAsync(string queueName)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
        }

        string storageConnectionString = Configuration["Values:AzureWebJobsStorage"] ?? Configuration["StorageConnectionString"];
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        CloudQueue queue = queueClient.GetQueueReference(queueName);
        await queue.CreateIfNotExistsAsync();
        return queue;
    }

    public static async Task<bool> CreateAsync(string containerName)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
        }

        CloudQueue queue = InitializeQueueReference(containerName);
        return await queue.CreateIfNotExistsAsync();
    }

    public static async Task AddAsync(string queueMessage, string containerName)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
        }

        CloudQueue queue = InitializeQueueReference(containerName);
        await queue.CreateIfNotExistsAsync();
        CloudQueueMessage message = new CloudQueueMessage(queueMessage);
        await queue.AddMessageAsync(message);
    }

    public static async Task<CloudQueueMessage> GetMostRecentMessageAsync(string containerName)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
        }
        switch (queue.ApproximateMessageCount)
        {
            case 0:
                return null; // No messages in the queue
            case 1:
                return await ReceiveMessageAsync(queue);
            case null:
                await queue.CreateIfNotExistsAsync();
                return null; // Queue does not exist
            detault:
                return await ReceiveMessageAsync(queue);
        }
        return await queue.GetMessageAsync();
    }

    public static async Task<CloudQueueMessage> ReceiveMessageAsync(CloudQueue queue)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
        }

        CloudQueueMessage message = await queue.GetMessageAsync();
        if (message != null)
        {
            await queue.DeleteMessageAsync(message);
        }
        return message;
    }

    public static async Task<bool> ClearQueueAsync(string containerName)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not set. Please call SetConfiguration first.");
        }
        CloudQueue queue = InitializeQueueReference(containerName);
        return await queue.ClearAsync();
    }
}
