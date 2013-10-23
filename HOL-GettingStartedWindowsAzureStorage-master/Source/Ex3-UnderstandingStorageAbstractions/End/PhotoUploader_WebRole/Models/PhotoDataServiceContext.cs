using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

namespace PhotoUploader_WebRole.Models
{
    public class PhotoDataServiceContext : TableServiceContext
    {
        public PhotoDataServiceContext(CloudTableClient client)
            : base(client)
        {

        }

        public IEnumerable<PhotoEntity> GetPhotos()
        {
            return this.CreateQuery<PhotoEntity>("Photos");
        }

        public PhotoEntity GetById(string partitionKey, string rowKey)
        {
            CloudTable table = this.ServiceClient.GetTableReference("Photos");
            TableOperation retrieveOperation = TableOperation.Retrieve<PhotoEntity>(partitionKey, rowKey);

            TableResult retrievedResult = table.Execute(retrieveOperation);

            if (retrievedResult.Result != null)
                return (PhotoEntity)retrievedResult.Result;
            else
                return null;
        }

        public void AddPhoto(PhotoEntity photo)
        {
            TableOperation operation = TableOperation.Insert(photo);
            CloudTable table = this.ServiceClient.GetTableReference("Photos");
            table.Execute(operation);
        }

        public void UpdatePhoto(PhotoEntity photo)
        {
            CloudTable table = this.ServiceClient.GetTableReference("Photos");
            TableOperation retrieveOperation = TableOperation.Retrieve<PhotoEntity>(photo.PartitionKey, photo.RowKey);

            TableResult retrievedResult = table.Execute(retrieveOperation);
            PhotoEntity updateEntity = (PhotoEntity)retrievedResult.Result;

            if (updateEntity != null)
            {
                updateEntity.Description = photo.Description;
                updateEntity.Title = photo.Title;

                TableOperation replaceOperation = TableOperation.Replace(updateEntity);
                table.Execute(replaceOperation);
            }
        }

        public void DeletePhoto(PhotoEntity photo)
        {
            CloudTable table = this.ServiceClient.GetTableReference("Photos");
            TableOperation retrieveOperation = TableOperation.Retrieve<PhotoEntity>(photo.PartitionKey, photo.RowKey);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            PhotoEntity deleteEntity = (PhotoEntity)retrievedResult.Result;

            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                table.Execute(deleteOperation);
            }

        }
    }
}