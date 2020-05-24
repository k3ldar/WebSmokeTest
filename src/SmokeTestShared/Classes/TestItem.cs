namespace SmokeTest.Shared
{
    public class TestItem
    {
        public TestItem()
        {

        }

        public TestItem(in long testId, in long queueStart, in long queueEnd)
        {
            TestId = testId;
            Start = queueStart;
            End = queueEnd;
        }

        public TestItem(in long id, in long testId, in long queueStart, in long queueEnd)
            : this(testId, queueStart, queueEnd)
        {
            UniqueId = id;
        }

        public long TestId { get; set; }

        public long Start { get; set; }

        public long End { get; set; }

        public int Position { get; set; }

        public long UniqueId { get; set; }
    }
}
