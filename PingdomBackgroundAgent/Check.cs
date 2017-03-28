using System;

namespace PingdomBackgroundAgent
{
    public class Check
    {
        public int Id { get; set; }
        public int CreatedTimeStamp { get; set; }
        public DateTime LastErrorTime { get; set; }
        public int LastResponseTime { get; set; }
        public string Status { get; set; }
    }
}
