using System;
using System.Collections.Generic;
using System.Net.Sockets;
using net_Protocol;
namespace net_core
{
    public class UserToken
    {
        public SocketAsyncEventArgs ReadSAEA { get; set; }
        public SocketAsyncEventArgs WriteSAEA { get; set; }
        public Socket Socket { get; set; }
        private List<byte> ReceiveData = new List<byte>();//接收的字节
        private Queue<byte[]> MessageList = new Queue<byte[]>();//待发送信息
        private bool isSending = false;
        private bool isReading = false;
        public Action<UserToken, string> close;
        public Action<SocketAsyncEventArgs> ProcessSend;
        public IHandler centerHandler;
        public void Send(byte[] data)
        {
            MessageList.Enqueue(data);
            if (!isSending)
            {
                DoSend();
            }
        }
        public void Send(Message msg)
        {
            Send(MyEncoding.Encode(msg));
        }

        private void DoSend()
        {
            if (Socket == null)
            {
                close(this, "调用已断开的连接");
            }
            isSending = true;
            if (MessageList.Count <= 0)
            {
                isSending = false;
                return;
            }
            byte[] buf = MessageList.Dequeue();

            WriteSAEA.SetBuffer(buf, 0, buf.Length);

            if (!Socket.SendAsync(WriteSAEA))
            {
                ProcessSend(WriteSAEA);
            }

            //DoSend();
        }

        public void Sended()
        {
            DoSend();
        }

        public void Receive(byte[] data)
        {
            ReceiveData.AddRange(data);
            if (!isReading)
            {
                DoRead();
            }
        }

        private void DoRead()
        {
            isReading = true;
            try
            {
                object msg = net_Protocol.MyEncoding.Decode(ReceiveData);
                if (msg == null)
                {
                    isReading = false;
                    return;
                }
                //通知上层
                centerHandler.OnMessageReceive(this, msg);
                DoRead();
            }
            catch (Exception ex)
            {
                close(this, ex.ToString());
            }
        }

        public void Close()
        {
            if (Socket != null && Socket.Connected)
            {
                try
                {
                    Socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {

                }
                finally
                {
                    Socket.Close();
                }
            }
            Socket = null;
            ReceiveData.Clear();
            MessageList.Clear();
            isReading = false;
            isSending = false;
        }
    }
}
