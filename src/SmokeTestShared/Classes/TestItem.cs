namespace SmokeTest.Shared
{
    public class TestItem
    {
        public TestItem()
        {

        }

        public TestItem(in long testId, in long queueStart, in long queueEnd, in double estimatedTime)
        {
            TestId = testId;
            Start = queueStart;
            End = queueEnd;
            EstimatedTime = estimatedTime;
        }

        public TestItem(in long id, in long testId, in long queueStart, in long queueEnd, in double estimatedTime)
            : this(testId, queueStart, queueEnd, estimatedTime)
        {
            UniqueId = id;
        }

        public long TestId { get; set; }

        public long Start { get; set; }

        public long End { get; set; }

        public int Position { get; set; }

        public long UniqueId { get; set; }

        public double EstimatedTime { get; set; }
    }
}
