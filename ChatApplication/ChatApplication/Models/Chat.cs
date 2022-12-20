using System;
using System.Collections.Generic;

namespace ChatApplication.Models
{
    public class Chat
    {
        public List<string> users { get; set; }
        public string partner { get; set; }
        public DateTime time { get; set; }
        public List<string> messages { get; set; }

    }
}
