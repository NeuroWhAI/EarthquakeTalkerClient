using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthquakeTalkerClient
{
    public class Message
    {
        public enum Priority
        {
            Low,
            Normal,
            High,
            Critical,
        }

        public Guid Id
        { get; set; }

        public DateTime CreationTime
        { get; set; }

        public Priority Level
        { get; set; }

        public string Sender
        { get; set; }

        public string Text
        { get; set; }
    }
}
