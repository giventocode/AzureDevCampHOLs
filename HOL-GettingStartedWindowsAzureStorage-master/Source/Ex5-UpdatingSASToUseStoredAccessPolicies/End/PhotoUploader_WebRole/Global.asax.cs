using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PhotoUploader_WebRole
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = cloudTableClient.GetTableReference("Photos");
            table.CreateIfNotExists();

            TablePermissions tp = new TablePermissions();
            tp.SharedAccessPolicies.Add("readonly", new SharedAccessTablePolicy { Permissions = SharedAccessTablePermissions.Query, SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15) });
            tp.SharedAccessPolicies.Add("edit", new SharedAccessTablePolicy { Permissions = SharedAccessTablePermissions.Query | SharedAccessTablePermissions.Add | SharedAccessTablePermissions.Update, SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15) });
            tp.SharedAccessPolicies.Add("admin", new SharedAccessTablePolicy { Permissions = SharedAccessTablePermissions.Query | SharedAccessTablePermissions.Add | SharedAccessTablePermissions.Update | SharedAccessTablePermissions.Delete, SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15) });
            tp.SharedAccessPolicies.Add("none", new SharedAccessTablePolicy { Permissions = SharedAccessTablePermissions.None, SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15) });
            table.SetPermissions(tp);

            CloudQueue queue = storageAccount.CreateCloudQueueClient().GetQueueReference("messagequeue");
            queue.CreateIfNotExists();
            QueuePermissions qp = new QueuePermissions();
            qp.SharedAccessPolicies.Add("add", new SharedAccessQueuePolicy { Permissions = SharedAccessQueuePermissions.Add | SharedAccessQueuePermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15)});
            qp.SharedAccessPolicies.Add("process", new SharedAccessQueuePolicy { Permissions = SharedAccessQueuePermissions.ProcessMessages | SharedAccessQueuePermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15) });
            queue.SetPermissions(qp);

            queue.Metadata.Add("Resize", "28");
            queue.SetMetadata();

            CloudBlobContainer blob = storageAccount.CreateCloudBlobClient().GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
            blob.CreateIfNotExists();
            blob.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            BlobContainerPermissions bp = new BlobContainerPermissions();
            bp.SharedAccessPolicies.Add("read", new SharedAccessBlobPolicy { Permissions = SharedAccessBlobPermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15) });
            blob.SetPermissions(bp);
        }
    }
}