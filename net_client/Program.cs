using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using net_Protocol;
namespace net_client
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Client> list = new List<Client>();
            for(int i = 0; i < 1; i++)
            {
                Client client = new Client();
                list.Add(client);
            }

            for(int i = 0; i < list.Count; i++)
            {
                Console.WriteLine("启动第{0}个客户端", i + 1);
                list[i].Start();
            }

            while (true)
            {
                Thread.Sleep(1000);
            };
        }
    }
}
