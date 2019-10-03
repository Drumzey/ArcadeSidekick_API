using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared
{
    public class ObjectInformation
    {
        [DynamoDBHashKey]
        public string Key { get; set; }

        public List<string> ListValue { get; set; }

        public Dictionary<string, string> DictionaryValue { get; set; }
    }
}