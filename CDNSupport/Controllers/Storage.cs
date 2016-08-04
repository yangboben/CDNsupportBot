using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CDNSupport
{
    [Serializable]
     class Storage
    {
        static public IEnumerable<tableitem> getAllOptions(string intent)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("support");

            TableQuery<CDNSupportEntity> query = new TableQuery<CDNSupportEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, intent));


            IEnumerable<tableitem> r = table.ExecuteQuery(query).Select<CDNSupportEntity,tableitem>(p => new tableitem(p)).ToList();

            //todo 将其转换为其他class。
            return r;
           
        }

   }

    [Serializable]
     public class CDNSupportEntity : TableEntity
    {

        public CDNSupportEntity(string pk, string rk)
        {
            this.PartitionKey = pk;
            this.RowKey = rk;
        }

        public CDNSupportEntity(string pk,string rk,string service,string answer) {
            this.PartitionKey = pk;
            this.RowKey = RowKey;
            this.service = service;
            this.answer = answer;
            
        }

        public CDNSupportEntity() { }

        public string service { get; set; }
       
        public string answer { get; set; }

        public string target { get; set; }

        public string item { get; set; }
        public string troubletype { get; set; }
    }

    [Serializable]
    public class tableitem
    {
        public string intent { get; set; }

        public string service { get; set; }

        public string target { get; set; }

        public string answer { get; set; }

        public string item { get; set; }

        public string troubletype { get; set; }
        public tableitem(CDNSupportEntity entity){

            intent = entity.PartitionKey;
            service = entity.service;
            target = entity.target;
            item = entity.item;
            answer = entity.answer;
            troubletype = entity.troubletype;
        }
    }
 
    
}