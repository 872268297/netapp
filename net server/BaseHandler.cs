using net_core;
using net_Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace net_server
{
    public abstract class BaseHandler : IHandler
    {
        public abstract void OnClientClose(UserToken token, string error);

        public abstract void OnClientConnect(UserToken token);

        public abstract void OnMessageReceive(UserToken token, Message msg);

        public void OnMessageReceive(UserToken token, object msg)
        {
            OnMessageReceive(token, msg as Message);
        }

        public void Send(UserToken token, int type, int area, int command, object data = null)
        {
            token.Send(new Message(type, area, command, data));
        }
    }
}
