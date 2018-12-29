using System;
using System.Collections.Generic;
using System.Text;

namespace net_Protocol
{
    [Serializable]
    public class Message
    {
        public int Type { get; set; }

        public int Area { get; set; }

        public int Command { get; set; }

        public object Data { get; set; }

        public Message()
        {

        }
        public Message(int type, int area, int command, object data)
        {
            Type = type;
            Area = area;
            Command = command;
            Data = data;
        }

        public T GetData<T>()
        {
            return (T)Data;
        }
    }
}
