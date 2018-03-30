using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDynamoDbOrm
{
    public interface IPersistentItem<TKey> where TKey : IEquatable<TKey>
    {
        [DynamoDBHashKey]
        TKey Id { get; set; }
    }
}
