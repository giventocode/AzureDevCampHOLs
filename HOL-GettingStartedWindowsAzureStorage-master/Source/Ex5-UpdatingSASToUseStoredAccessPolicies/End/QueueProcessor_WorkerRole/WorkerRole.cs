using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace QueueProcessor_WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private DateTime serviceQueueSasExpiryTime;
        private CloudBlobContainer container;
        private Uri uri = new Uri("http://127.0.0.1:10001/devstoreaccount1");

        public override void Run()
        {
            Trace.TraceInformation("QueueProcessor_WorkerRole entry point called", "Information");
            var queueClient = new CloudQueueClient(this.uri, new StorageCredentials(this.GetQueueSas()));

            var queue = queueClient.GetQueueReference("messagequeue");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working", "Information");

                if (DateTime.UtcNow.AddMinutes(1) >= this.serviceQueueSasExpiryTime)
                {
                    queueClient = new CloudQueueClient(this.uri, new StorageCredentials(this.GetQueueSas()));
                    queue = queueClient.GetQueueReference("messagequeue");
                }

                var msg = queue.GetMessage();

                if (msg != null)
                {
                    queue.FetchAttributes();

                    var messageParts = msg.AsString.Split(new char[] { ',' });
                    var message = messageParts[0];
                    var blobReference = messageParts[1];

                    if (queue.Metadata.ContainsKey("Resize") && string.Equals(message, "Photo Uploaded"))
                    {
                        var maxSize = queue.Metadata["Resize"];

                        Trace.TraceInformation("Resize is configured");

                        CloudBlockBlob outputBlob = this.container.GetBlockBlobReference(blobReference);

                        outputBlob.FetchAttributes();

                        Trace.TraceInformation(string.Format("Image ContentType: {0}", outputBlob.Properties.ContentType)); 
                        Trace.TraceInformation(string.Format("Image width: {0}", outputBlob.Metadata["Width"]));
                        Trace.TraceInformation(string.Format("Image hieght: {0}", outputBlob.Metadata["Height"])); 
                    }

                    Trace.TraceInformation(string.Format("Message '{0}' processed.", message));
                    queue.DeleteMessage(msg);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            this.CreateCloudBlobClient();

            return base.OnStart();
        }

        private string GetQueueSas()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference("messagequeue");
            queue.CreateIfNotExists();

            QueuePermissions qp = new QueuePermissions();
            qp.SharedAccessPolicies.Add("process", new SharedAccessQueuePolicy { Permissions = SharedAccessQueuePermissions.ProcessMessages | SharedAccessQueuePermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15) });
            queue.SetPermissions(qp);

            var token = queue.GetSharedAccessSignature(
                         new SharedAccessQueuePolicy(),
                            "process");
            this.serviceQueueSasExpiryTime = DateTime.UtcNow.AddMinutes(15);
            return token;
        }

        private void CreateCloudBlobClient()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
            this.container = blobStorage.GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
        }
    }
}
