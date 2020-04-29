namespace WebSmokeTest.Engine
{
    public class FormReport
    {
        public FormReport()
        {
            Status = FormStatus.New;
        }

        public string Id { get; set; }

        public string Method { get; set; }

        public string Action { get; set; }

        public FormStatus Status { get; set; }
    }

}
