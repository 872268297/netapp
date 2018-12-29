using net_Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace net_client
{
    class Client
    {

        private TcpClient client;
        List<byte> cache = new List<byte>();
        private byte[] buff = new byte[1024];
        private bool isReading = false;
        public void Start()
        {
            client = new TcpClient();
            IPEndPoint end = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7899);
            client.Connect(end);

            client.GetStream().BeginRead(buff, 0, 1024, ReceiveMessage, null);

            NetworkStream stream = client.GetStream();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    string str = @"
123qwelkasdjak;sjdaskjdkals;jdklasjd123qwelkasdjak;sjdaskjdkals;jdklasjd
123qwelkasdjak;sjdaskjdkals;jdklasjd
"; //Console.ReadLine();
                    Message msg = new Message
                    {
                        Type = 1,
                        Area = 2,
                        Command = 3,
                        Data = str
                    };
                    byte[] bt = MyEncoding.Encode(msg);
                    stream.Write(bt, 0, bt.Length);
                    Thread.Sleep(1);
                }
            });

            //while (true)
            //{


            //}
        }

        public void ReceiveMessage(IAsyncResult e)
        {
            NetworkStream stream = client.GetStream();

            int byteRead = stream.EndRead(e);
            if (byteRead < 1)
            {
                return;
            }
            byte[] b = new byte[byteRead];
            Buffer.BlockCopy(buff, 0, b, 0, byteRead);
            cache.AddRange(b);
            if (!isReading)
            {
                DoRead();
            }
            stream.BeginRead(buff, 0, 1024, ReceiveMessage, null);

        }

        private void DoRead()
        {
            isReading = true;
            Message msg = MyEncoding.Decode(cache) as Message;
            if (msg == null)
            {
                isReading = false;
                return;
            }
            ProcessMsg(msg);
            DoRead();
        }

        private void ProcessMsg(Message msg)
        {
            Console.WriteLine(msg.GetData<String>());
        }
    }
}
