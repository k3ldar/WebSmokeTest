using System;

using SharedPluginFeatures;

namespace SmokeTest.Scheduler.Models
{
    public sealed class QueueItemModel : BaseModel
    {
        public QueueItemModel()
        {

        }

        public QueueItemModel(in BaseModelData modelData, in long queueId, 
            in string testName, in string url, in int position,
            in DateTime queueStartTime)
            : base(modelData)
        {
            QueueId = queueId;
            TestName = testName;
            QueueTime = queueStartTime;
            Url = url;
            Position = position;
        }

        #region Properties

        public long QueueId { get; set; }

        public string TestName { get; private set; }

        public string Url { get; private set; }

        public DateTime QueueTime { get; private set; }

        public string Confirm { get; set; }

        public int Position { get; private set; }

        #endregion Properties
    }
}
