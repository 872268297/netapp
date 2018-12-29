using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
namespace net_core
{
    class BufferManager
    {
        private int _totalSize;
        private byte[] _buffer;
        private int _currentIndex;
        private int _bufferSize;
        private Stack<int> _freeIndexPool = new Stack<int>();
        public BufferManager(int totalSize, int bufferSize)
        {
            _totalSize = totalSize;
            _bufferSize = bufferSize;
            _buffer = new byte[_totalSize];
            _currentIndex = 0;
        }

        public bool SetBuffer(SocketAsyncEventArgs e)
        {
            if (_freeIndexPool.Count > 0)
            {
                e.SetBuffer(_buffer, _freeIndexPool.Pop(), _bufferSize);
            }
            else
            {
                if ((_currentIndex + _bufferSize) > _totalSize)
                {
                    return false;
                }
                e.SetBuffer(_buffer, _currentIndex, _bufferSize);
                _currentIndex += _bufferSize;
            }
            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs e)
        {
            _freeIndexPool.Push(e.Offset);
            e.SetBuffer(null, 0, 0);
        }
    }
}
