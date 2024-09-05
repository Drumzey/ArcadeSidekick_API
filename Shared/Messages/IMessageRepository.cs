using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Messages
{
    public interface IMessageRepository
    {
        void SetupTable();

        void Save(Messages messages);

        void SaveBatch(List<Messages> messages);

        Messages Load(string partitionKey);

        List<Messages> Scan(IEnumerable<ScanCondition> scanConditions);

        List<Messages> All();

        List<Messages> BatchGet(List<string> userNames);
    }
}
