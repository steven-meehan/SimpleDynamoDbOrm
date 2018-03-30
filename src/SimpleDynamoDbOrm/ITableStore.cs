using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDynamoDbOrm
{
    interface ITableStore
    {
        Task<CreateTableResponse> CreateTableAsync(TableDefinition tableDefinition);
        Task<DeleteTableResponse> DeleteTableAsync(string tableName);
        Task<DescribeTableResponse> GetTableDescriptionAsync(string tableName);
    }
}
