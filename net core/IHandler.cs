using System;
using System.Collections.Generic;
using System.Text;
using net_Protocol;
namespace net_core
{
    public interface IHandler
    {
        void OnClientClose(UserToken token,string error);

        void OnClientConnect(UserToken token);

        void OnMessageReceive(UserToken token, object msg);
    }
}
