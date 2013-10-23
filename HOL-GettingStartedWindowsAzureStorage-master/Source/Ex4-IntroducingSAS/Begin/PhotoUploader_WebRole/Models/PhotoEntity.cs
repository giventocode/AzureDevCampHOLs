using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace PhotoUploader_WebRole.Models
{
    public class PhotoEntity : TableEntity
    {
        public PhotoEntity() 
        {
            PartitionKey = "Photo";
            RowKey = Guid.NewGuid().ToString();
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public string BlobReference { get; set; }
    }
}