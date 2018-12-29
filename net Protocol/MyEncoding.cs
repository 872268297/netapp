using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using net_Protocol;
namespace net_Protocol
{
    public class MyEncoding
    {
        private static readonly int maxLength = 8192;
        public static byte[] Encode(object msg)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    Message m = msg as Message;
                    bw.Write(m.Type);
                    bw.Write(m.Area);
                    bw.Write(m.Command);
                    if (m.Data != null)
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, m.Data);
                    }
                    byte[] newBt = new byte[4 + (int)ms.Length];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)ms.Length), 0, newBt, 0, 4);
                    Buffer.BlockCopy(ms.ToArray(), 0, newBt, 4, (int)ms.Length);
                    return newBt;
                }
            }
        }

        public static object Decode(List<byte> list)
        {
            if (list.Count < 4)
            {
                return null;
            }
            byte[] lentBt = list.GetRange(0, 4).ToArray();
            int length = BitConverter.ToInt32(lentBt, 0);

            if (length > maxLength)
            {
                throw new Exception("协议数据包长度错误");
            }

            if (length > list.Count - 4)
            {
                return null;
            }


            byte[] msgBuff = list.GetRange(4, length).ToArray();
            //反序列化
            Message msg = new Message();

            using (MemoryStream ms = new MemoryStream(msgBuff))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    msg.Type = br.ReadInt32();
                    msg.Area = br.ReadInt32();
                    msg.Command = br.ReadInt32();
                    if(ms.Position < length)
                    {
                        BinaryFormatter de = new BinaryFormatter();
                        msg.Data = de.Deserialize(ms);
                    }
                }
            }
            lock (list)
            {
                list.RemoveRange(0, 4 + length);
            }
            return msg;
        }
    }
}
