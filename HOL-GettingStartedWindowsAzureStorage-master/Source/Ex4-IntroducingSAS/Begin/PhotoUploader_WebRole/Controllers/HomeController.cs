using PhotoUploader_WebRole.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace PhotoUploader_WebRole.Controllers
{
    public class HomeController : Controller
    {
        private CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
        //
        // GET: /

        public ActionResult Index()
        {
            CloudTableClient cloudTableClient = this.StorageAccount.CreateCloudTableClient();
            var photoContext = new PhotoDataServiceContext(cloudTableClient);

            return this.View(photoContext.GetPhotos().Select(x => this.ToViewModel(x)).ToList());
        }

        //
        // GET: /Home/Details/5

        public ActionResult Details(string partitionKey, string rowKey)
        {
            CloudTableClient cloudTableClient = this.StorageAccount.CreateCloudTableClient();
            var photoContext = new PhotoDataServiceContext(cloudTableClient);
            PhotoEntity photo = photoContext.GetById(partitionKey, rowKey);

            if (photo == null)
            {
                return HttpNotFound();
            }

            var viewModel = this.ToViewModel(photo);
            if (!string.IsNullOrEmpty(photo.BlobReference))
            {
                viewModel.Uri = this.GetBlobContainer().GetBlockBlobReference(photo.BlobReference).Uri.ToString();
            }

            return this.View(viewModel);
        }

        //
        // GET: /Home/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Home/Create

        [HttpPost]
        public ActionResult Create(PhotoViewModel photoViewModel, HttpPostedFileBase file, FormCollection collection)
        {
            if (this.ModelState.IsValid)
            {
                photoViewModel.PartitionKey = this.User.Identity.IsAuthenticated ? this.User.Identity.Name : "Public";
                var photo = this.FromViewModel(photoViewModel);

                if (file != null)
                {
                    //Save file stream to Blob Storage
                    var blob = this.GetBlobContainer().GetBlockBlobReference(file.FileName);
                    blob.Properties.ContentType = file.ContentType;
                    blob.UploadFromStream(file.InputStream);
                    photo.BlobReference = file.FileName;
                }
                else
                {
                    this.ModelState.AddModelError("File", new ArgumentNullException("file"));
                    return this.View(photoViewModel);
                }

                //Save information to Table Storage
                CloudTableClient cloudTableClient = this.StorageAccount.CreateCloudTableClient();
                var photoContext = new PhotoDataServiceContext(cloudTableClient);
                photoContext.AddPhoto(photo);

                try
                {
                    //Send create notification
                    var msg = new CloudQueueMessage("Photo Uploaded");
                    this.GetCloudQueue().AddMessage(msg);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceInformation("Error", "Couldn't send notification");
                }

                return this.RedirectToAction("Index");
            }

            return this.View();
        }

        //
        // GET: /Home/Edit/5

        public ActionResult Edit(string partitionKey, string rowKey)
        {
            CloudTableClient cloudTableClient = this.StorageAccount.CreateCloudTableClient();
            var photoContext = new PhotoDataServiceContext(cloudTableClient);
            PhotoEntity photo = photoContext.GetById(partitionKey, rowKey);

            if (photo == null)
            {
                return this.HttpNotFound();
            }

            var viewModel = this.ToViewModel(photo);
            if (!string.IsNullOrEmpty(photo.BlobReference))
            {
                viewModel.Uri = this.GetBlobContainer().GetBlockBlobReference(photo.BlobReference).Uri.ToString();
            }

            return this.View(viewModel);
        }

        //
        // POST: /Home/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PhotoViewModel photoViewModel, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var photo = this.FromViewModel(photoViewModel);

                //Update information in Table Storage
                CloudTableClient cloudTableClient = this.StorageAccount.CreateCloudTableClient();
                var photoContext = new PhotoDataServiceContext(cloudTableClient);
                photoContext.UpdatePhoto(photo);

                return this.RedirectToAction("Index");
            }

            return this.View();
        }

        //
        // GET: /Home/Delete/5

        public ActionResult Delete(string partitionKey, string rowKey)
        {
            CloudTableClient cloudTableClient = this.StorageAccount.CreateCloudTableClient();
            var photoContext = new PhotoDataServiceContext(cloudTableClient);
            PhotoEntity photo = photoContext.GetById(partitionKey, rowKey);

            if (photo == null)
            {
                return this.HttpNotFound();
            }

            var viewModel = this.ToViewModel(photo);
            if (!string.IsNullOrEmpty(photo.BlobReference))
            {
                viewModel.Uri = this.GetBlobContainer().GetBlockBlobReference(photo.BlobReference).Uri.ToString();
            }

            return this.View(viewModel);
        }

        //
        // POST: /Home/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string partitionKey, string rowKey)
        {
            CloudTableClient cloudTableClient = this.StorageAccount.CreateCloudTableClient();
            var photoContext = new PhotoDataServiceContext(cloudTableClient);
            PhotoEntity photo = photoContext.GetById(partitionKey, rowKey);
            photoContext.DeletePhoto(photo);

            //Deletes the Image from Blob Storage
            if (!string.IsNullOrEmpty(photo.BlobReference))
            {
                var blob = this.GetBlobContainer().GetBlockBlobReference(photo.BlobReference);
                blob.DeleteIfExists();
            }

            try
            {
                //Send delete notification
                var msg = new CloudQueueMessage("Photo Deleted");
                this.GetCloudQueue().AddMessage(msg);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceInformation("Error", "Couldn't send notification");
            }

            return this.RedirectToAction("Index");
        }

        private PhotoViewModel ToViewModel(PhotoEntity photo)
        {
            return new PhotoViewModel
            {
                PartitionKey = photo.PartitionKey,
                RowKey = photo.RowKey,
                Title = photo.Title,
                Description = photo.Description
            };
        }

        private PhotoEntity FromViewModel(PhotoViewModel photoViewModel)
        {
            var photo = new PhotoEntity
                {
                    Title = photoViewModel.Title,
                    Description = photoViewModel.Description
                };

            photo.PartitionKey = photoViewModel.PartitionKey ?? photo.PartitionKey;
            photo.RowKey = photoViewModel.RowKey ?? photo.RowKey;
            return photo;
        }

        private CloudBlobContainer GetBlobContainer()
        {
            var client = this.StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(CloudConfigurationManager.GetSetting("ContainerName"));
            if (container.CreateIfNotExists())
            {
                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }

            return container;
        }

        private CloudQueue GetCloudQueue()
        {
            var queueClient = this.StorageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("messagequeue");
            queue.CreateIfNotExists();
            return queue;
        }
    }
}
