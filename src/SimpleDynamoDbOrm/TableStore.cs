using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace SimpleDynamoDbOrm
{
    class TableStore : ITableStore
    {
        public TableStore()
        {
            _client = new AmazonDynamoDBClient();

        }

        private readonly AmazonDynamoDBClient _client;

        /// <summary>
        /// CreateTableAsync will check to see if a given table exists and if not it will create a Table in DynamoDB
        /// <remarks>[CAUTION] This operation will return null if the table exists</remarks>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="readCapacityUnits"></param>
        /// <param name="writeCapacityUnits"></param>
        /// <param name="hashKeySchemaElement"></param>
        /// <param name="rangeKeySchemaElement"></param>
        /// <param name="hashAttributeDefinition"></param>
        /// <param name="rangeAttributeDefinition"></param>
        /// <returns>A Task of type CreateTableResponse</returns>
        public virtual async Task<CreateTableResponse> CreateTableAsync(TableDefinition tableDefinition)
        {
            if (!(await DoesTableExists(tableDefinition.TableName)))
            {
                var createTableRequest = new CreateTableRequest
                {
                    TableName = tableDefinition.TableName,
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = tableDefinition.ReadCapacityUnits,
                        WriteCapacityUnits = tableDefinition.WriteCapacityUnits
                    },
                    KeySchema = new List<KeySchemaElement>
                {
                    tableDefinition.HashKeySchemaElement,
                    tableDefinition.RangeKeySchemaElement ?? null
                },
                    AttributeDefinitions = new List<AttributeDefinition>()
                {
                    tableDefinition.HashAttributeDefinition,
                    tableDefinition.RangeAttributeDefinition ?? null
                }
                };

                return await _client.CreateTableAsync(createTableRequest).ConfigureAwait(false); 
            }

            return null;
        }

        /// <summary>
        /// DeleteTableAsyncwill check to see if a given table exists and if it does it will delete the Table from DynamoDB
        /// <remarks>[CAUTION] This operation will return null if the table does not exist</remarks>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>A Task of type DeleteTableResponse</returns>
        public virtual async Task<DeleteTableResponse> DeleteTableAsync(string tableName)
        {
            if (await DoesTableExists(tableName))
            {
                var deleteTableRequest = new DeleteTableRequest
                {
                    TableName = tableName
                };

                return await _client.DeleteTableAsync(deleteTableRequest).ConfigureAwait(false); 
            }

            return null;
        }

        /// <summary>
        /// GetTableDescriptionAsync will get a Table's description from DynamoDb
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>A Task of type DescribeTableResponse</returns>
        public virtual async Task<DescribeTableResponse> GetTableDescriptionAsync(string tableName)
        {
            var request = new DescribeTableRequest
            {
                TableName = tableName
            };
            return await _client.DescribeTableAsync(request).ConfigureAwait(false);
        }


        //Helper Methods
        private async Task<Boolean> DoesTableExists(string tableName)
        {
            if((await _client.ListTablesAsync()).TableNames.Contains(tableName))
            {
                return true;
            }

            return false;
        }
    }
}
