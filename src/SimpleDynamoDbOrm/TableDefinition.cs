using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDynamoDbOrm
{
    public class TableDefinition
    {
        public string TableName { get; set; }
        public int ReadCapacityUnits { get; set; }
        public int WriteCapacityUnits { get; set; }
        public KeySchemaElement HashKeySchemaElement { get; set; }
        public KeySchemaElement RangeKeySchemaElement { get; set; } = null;
        public AttributeDefinition HashAttributeDefinition { get; set; }
        public AttributeDefinition RangeAttributeDefinition { get; set; } = null;
    }
}
