using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace net_core
{
    class UserTokenPool
    {
        private readonly Stack<UserToken> _pool = new Stack<UserToken>();
        
        public UserToken Pop()
        {
            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        public void Push(UserToken e)
        {
            if (e == null)
            {
                return;
            }
            lock (_pool)
            {
                _pool.Push(e);
            }
        }

        public int Count()
        {
            lock(_pool){
                return _pool.Count;
            }
        }
    }
}
