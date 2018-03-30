using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDynamoDbOrm
{
    public interface IDataStore<TItem, TKey>
        where TItem : class, IPersistentItem<TKey>
        where TKey : IEquatable<TKey>
    {
        Task AddItemAsync(TItem item);
        Task BatchStoreAsync(IEnumerable<TItem> items);
        Task<IList<TItem>> GetAllAsync();
        Task<TItem> GetItemAsync(TKey id);
        Task<IList<TItem>> BatchGetAsync(IEnumerable<TKey> ids);
        Task ModifyItemAsync(TItem item);
        Task DeleteItemAsync(TItem item);
        Task<IList<TItem>> SearchItemsAsync(IEnumerable<ScanCondition> conditions, DynamoDBOperationConfig operationConfig = null);
    }
}
