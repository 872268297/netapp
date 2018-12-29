using System;
using System.Collections.Generic;
using net_core;
using net_Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace net_server
{
    public class CenterHandler : BaseHandler
    {
        private List<UserToken> userList = new List<UserToken>();

        public CenterHandler()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Console.WriteLine(userList.Count);
                    Thread.Sleep(1000);
                }
            });
        }

        public override void OnClientClose(UserToken token, string error)
        {
            lock(userList){
                userList.Remove(token);
            }
            Console.WriteLine("客户{0}断开连接,{1}", token.Socket.RemoteEndPoint.ToString(), error);
        }

        public override void OnClientConnect(UserToken token)
        {
            lock (userList)
            {
                userList.Add(token);
            }
            if (token.Socket != null && token.Socket.Connected)
            {
                Console.WriteLine("客户{0}连入", token.Socket.RemoteEndPoint.ToString());
            }

        }

        public override void OnMessageReceive(UserToken token, Message msg)
        {
            //Console.WriteLine(userList.Count + token.Socket.RemoteEndPoint.ToString());
            //Console.WriteLine(msg.GetData<string>());
            token.Send(msg);
        }
    }
}
