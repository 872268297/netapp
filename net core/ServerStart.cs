using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace net_core
{
    public class ServerStart
    {
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly int _maxClient;
        private Socket _serverSocket;
        public Semaphore _maxAcceptedClients;
        private readonly UserTokenPool _pool = new UserTokenPool();
        private BufferManager _bufferManager;
        public int BuffSize { get; set; }
        public IHandler centerHandler;
        public ServerStart(string ip, int port, int maxClient)
        {
            this._ipAddress = ip;
            this._port = port;
            this._maxClient = maxClient;
            BuffSize = 2048;
        }
        public void Start()
        {
            Init();
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
            _serverSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(localEndPoint);
            _serverSocket.Listen(100);
            Console.WriteLine("正在侦听");
            StartAccpet(null);
        }
        private void Init()
        {
            _bufferManager = new BufferManager(_maxClient * BuffSize, BuffSize);

            for (int i = 0; i < _maxClient; i++)
            {
                UserToken token = new UserToken
                {
                    ReadSAEA = new SocketAsyncEventArgs(),
                    WriteSAEA = new SocketAsyncEventArgs()
                };
                token.ReadSAEA.UserToken = token;
                token.WriteSAEA.UserToken = token;
                _bufferManager.SetBuffer(token.ReadSAEA);
                token.ReadSAEA.Completed += OnIOCompleted;
                token.WriteSAEA.Completed += OnIOCompleted;
                token.ProcessSend = this.ProcessSend;
                token.close = this.CloseClient;
                token.centerHandler = centerHandler;
                _pool.Push(token);
            }
            _maxAcceptedClients = new Semaphore(_maxClient, _maxClient);
        }

        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessIOCompleted(e);
        }

        private void ProcessIOCompleted(SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {

            UserToken token = e.UserToken as UserToken;
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    byte[] data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, e.Offset, data, 0, data.Length);
                    //取得数据
                    token.Receive(data);
                    StartReceive(e);//继续下次接收
                }
                else
                {
                    CloseClient(token, "远程客户端主动关闭连接");
                }
            }
            else
            {
                CloseClient(token, e.SocketError.ToString());
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                UserToken token = e.UserToken as UserToken;
                token.Sended();
            }
            else
            {
                CloseClient(e.UserToken as UserToken, e.SocketError.ToString());
            }
        }

        private void StartAccpet(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += OnAcceptCompleted;
            }
            else
            {
                e.AcceptSocket = null;
            }
            _maxAcceptedClients.WaitOne();

            if (!_serverSocket.AcceptAsync(e))
            {
                ProcessAccept(e);
            }
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            UserToken token = e.UserToken as UserToken;
            try
            {
                if (!token.Socket.ReceiveAsync(e))
                {
                    ProcessIOCompleted(e);
                };
            }
            catch (Exception ex)
            {
                CloseClient(token, ex.ToString());
            }


        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket s = e.AcceptSocket;//和客户端关联的socket
                if (s.Connected)
                {
                    UserToken token = _pool.Pop();
                    try
                    {
                        token.Socket = s;

                        StartReceive(token.ReadSAEA);

                        centerHandler.OnClientConnect(token);
                        //if (!s.ReceiveAsync(token.ReadSAEA))
                        //{
                        //    ProcessIOCompleted(token.ReadSAEA);
                        //}

                    }
                    catch (SocketException ex)
                    {
                        CloseClient(token, string.Format("接受客户{0}数据出错,异常信息:{1}", s.RemoteEndPoint, ex.ToString()));
                    }
                }
                else
                {
                    _maxAcceptedClients.Release();
                }
            }
            else
            {
                _maxAcceptedClients.Release();
            }

            StartAccpet(e);
        }

        private void CloseClient(UserToken token, string error)
        {
            lock (token)
            {
                if (token.Socket != null)
                {
                    centerHandler.OnClientClose(token, error);
                    token.Close();
                    _pool.Push(token);
                    _maxAcceptedClients.Release();
                }
            }
        }
    }
}
