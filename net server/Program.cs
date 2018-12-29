using net_core;
using System;
using System.Threading;

namespace net_server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerStart server = new ServerStart("127.0.0.1", 7899, 60000);
            server.BuffSize = 4096;
            CenterHandler center = new CenterHandler();
            server.centerHandler = center;
            server.Start();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
