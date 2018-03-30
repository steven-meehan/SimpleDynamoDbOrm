using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleDynamoDbOrm
{
    public abstract class DataStore<TItem, TKey> : IDataStore<TItem, TKey>
        where TItem : class, IPersistentItem<TKey>
        where TKey : IEquatable<TKey>
    {
        public DataStore()
        {
            _client = new AmazonDynamoDBClient();

            _dbContext = new DynamoDBContext(_client, new DynamoDBContextConfig
            {
                //Setting the Consistent property to true ensures that you'll always get the latest 
                ConsistentRead = true,
                SkipVersionCheck = true
            });
        }

        private readonly DynamoDBContext _dbContext;
        private readonly AmazonDynamoDBClient _client;

        //Create Methods
        /// <summary>
        ///  AddItemAsync will accept an instance of a class that inherits from IPersistantItem and use that to create a POCO object in DynamoDB
        /// </summary>
        /// <param name="item"></param>
        public virtual async Task AddItemAsync(TItem item)
        {
            await _dbContext.SaveAsync(item).ConfigureAwait(false);
        }

        /// <summary>
        /// The BatchStoreAsync Method will accept an IEnumerable of an Object that inherits from IPersistentItem to store multiple POCO objects in DynamoDB
        /// </summary>
        /// <param name="items"></param>
        public virtual async Task BatchStoreAsync(IEnumerable<TItem> items)
        {
            var itemBatch = _dbContext.CreateBatchWrite<TItem>();

            foreach (var item in items)
            {
                itemBatch.AddPutItem(item);
            }

            await itemBatch.ExecuteAsync().ConfigureAwait(false);
        }

        //Read Methods
        /// <summary>
        /// GetAllAsync uses the ScanAsync operator to retrieve all items from the table for IPersistentItem
        /// <remarks>[CAUTION] This operation can be very expensive if your table is large</remarks>
        /// </summary>
        /// <returns>A Task of IList of type TItem</returns>
        public virtual async Task<IList<TItem>> GetAllAsync()
        {
            var conditions = new List<ScanCondition>();
            conditions.Add(new ScanCondition("Id", ScanOperator.IsNotNull, true));

            return await _dbContext
                .ScanAsync<TItem>(conditions)
                .GetNextSetAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// GetItemAsync retrieves a single IPersistent object by it's id from DynamoDB
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A Task of type TItem</returns>
        public virtual async Task<TItem> GetItemAsync(TKey id)
        {
            return await _dbContext.LoadAsync<TItem>(id).ConfigureAwait(false);
        }

        /// <summary>
        /// BatchGetAsync retrieves an IList of IPersistent objects by their ids from DynamoDB
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>A Task of IList of type TItem</returns>
        public virtual async Task<IList<TItem>> BatchGetAsync(IEnumerable<TKey> ids)
        {
            var batchGet = _dbContext.CreateBatchGet<TItem>();
            foreach (var id in ids)
            {
                batchGet.AddKey(id);
            }
            await batchGet.ExecuteAsync().ConfigureAwait(false);
            return batchGet.Results;
        }

        //Modify Methods
        /// <summary>
        /// ModifyItemAsync checks to see if the IPersistentItem exists and if it exists then it will update the IPersistentItem
        /// <remarks>[CAUTION] If the item doesn’t exist, it raises an AmazonDynamoDBException</remarks>
        /// </summary>
        /// <param name="item"></param>
        public virtual async Task ModifyItemAsync(TItem item)
        {
            if (await CheckForItemNotInDynamoDb(item))
            {
                throw new AmazonDynamoDBException("The item does not exist in the Table");
            }

            await _dbContext.SaveAsync(item);
        }

        //Delete Methods
        /// <summary>
        /// DeleteItemAsync checks to see if the IPersistentItem exists and if it exists then it will delete the IPersistentItem
        /// <remarks>[CAUTION] If the item doesn’t exist, it raises an AmazonDynamoDBException</remarks>
        /// </summary>
        /// <param name="item"></param>
        public virtual async Task DeleteItemAsync(TItem item)
        {
            if (await CheckForItemNotInDynamoDb(item))
            {
                throw new AmazonDynamoDBException("The item does not exist in the Table");
            }

            await _dbContext.DeleteAsync(item).ConfigureAwait(false);
        }

        //Search Methods
        /// <summary>
        /// SearchItemsAsync uses the ScanAsync operator to retrieve items based on the conditions and options passed into it to retrieve a list of items from the table for IPersistentItem
        /// <remarks>[CAUTION] This operation can be very expensive if your table is large</remarks>
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="operationConfig"></param>
        /// <returns>A Task of IList of type TItem</returns>
        public virtual async Task<IList<TItem>> SearchItemsAsync(IEnumerable<ScanCondition> conditions, DynamoDBOperationConfig operationConfig = null)
        {
            return await _dbContext
                .ScanAsync<TItem>(conditions, operationConfig)
                .GetNextSetAsync()
                .ConfigureAwait(false);
        }


        //Helper Methods
        private async Task<Boolean> CheckForItemNotInDynamoDb(TItem item)
        {
            var savedItem = await GetItemAsync(item.Id).ConfigureAwait(false);
            if (savedItem == null)
            {
                return false;
            }
            return true;
        }
    }
}