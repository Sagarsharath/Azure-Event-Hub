﻿using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using System;

namespace EventHub_receiver
{
    class Program
    {
		// keys have to be in configuration
        private const string ehubNamespaceConnectionString = "Endpoint=sb://event-hub-new.servicebus.windows.net/;SharedAccessKeyName=SAP;SharedAccessKey=hdt0i3dJZ0JxPW40+ht9JV+7O5NqeYNGX6aWGzndbFo=";
        private const string eventHubName = "hub-one";
        private const string blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storageccsharath;AccountKey=nCvk4LRtKwbRMBIf2xEJiWRqd5C3gXNeSdrRQ2PsurFXReXHGzV/Qmbxq+opsr7jpwkl2I4m5piTxOtELtCunQ==";
        private const string blobContainerName = "blob-container-one";
        static async Task Main()
        {
            // Read from the default consumer group: $Default
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            // Create a blob container client that the event processor will use 
            BlobContainerClient storageClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);

            // Create an event processor client to process events in the event hub
            EventProcessorClient processor = new EventProcessorClient(storageClient, consumerGroup, ehubNamespaceConnectionString, eventHubName);

            // Register handlers for processing events and handling errors
            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;

            // Start the processing
            await processor.StartProcessingAsync();

            // Wait for 10 seconds for the events to be processed
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Stop the processing
            await processor.StopProcessingAsync();
        }
        static async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            // Write the body of the event to the console window
            Console.WriteLine("\tRecevied event: {0} at {1}", Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()),DateTime.Now);

            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        static Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }
    }
}
