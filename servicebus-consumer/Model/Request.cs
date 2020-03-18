namespace ServiceBusDemo
{
    public class Request 
    {
        public string RequestId { get; set; }
        public string RequestApiStamp { get; set; }
        public string RequestStart { get; set; }
        public string RequestEnd { get; set; }
    }

    public class Api 
    {
        public string RequestId { get; set; }
        public string TimeStamp { get; set; }
        public string Version { get; set; }
    }
}