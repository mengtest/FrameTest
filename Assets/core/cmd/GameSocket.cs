using ProtoCmd;
using ProtoBuf;
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using sw.util;
 
 
 

namespace sw.net
{
    public class GameSocket:IDisposable
    {
        public delegate void Callback();
        private TcpClient m_lcClient;
        private string m_ip;
        private uint m_port;
         private bool stopFlag;
         private Queue<BaseCmd> m_sendQueue;
         private Queue<ByteArray> m_sendRawQueue;
        private Queue<BaseCmd> m_recvQueue;
        private ByteArray m_sendBuff;
        bool sending=false;
        NetworkStream stream;
       // Socket client;
        ByteArray recvBuff;

        public Callback OnConnected;
        public Callback OnClosed;
        Dictionary<int, int> luaDict;

        static object lockobj = new object();
        enum Stage
        {
            INIT,
            CONNECTING,
            CONNECTED,
            RUNNING,
            CLOSED,
            END
        };
        Stage stage = Stage.INIT;
        public void start(string ip, uint port)
        {
            //sw.util.LoggerHelper.Debug("start connect:" + port + "," + ip);
            m_ip = ip;
            m_port = port;
             if (m_lcClient != null)
            {
                m_lcClient.Close();
                m_lcClient = null;
            }
            stream = null;
            recvBuff = new ByteArray();
             m_lcClient = new TcpClient();
            Socket socket=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            IPAddress addr;
            if(!IPAddress.TryParse(m_ip, out addr))
            {
                addr = Dns.GetHostEntry(m_ip).AddressList[0];

            }
            m_sendQueue = new Queue<BaseCmd>();
            m_recvQueue = new Queue<BaseCmd>();
            m_sendRawQueue = new Queue<ByteArray>();
            m_sendBuff = new ByteArray(false);
            stopFlag = false;
            stage = Stage.CONNECTING;
            m_lcClient.BeginConnect(addr, (int)m_port,onConnected,socket);
          

        }
        public void SetLuaCmdDict(Dictionary<int, int> dict)
        {
            luaDict = dict;
        }
        void onConnected(IAsyncResult ar)
        {
            //sw.util.LoggerHelper.Debug("connected");
 
           
           
            try
            {
                
                m_lcClient.EndConnect(ar);
                stage = Stage.CONNECTED;
                stream = m_lcClient.GetStream();
                stream.BeginRead(recvBuff.getBuffer(), 0, 4, onReadHead, stream);
                //sw.util.LoggerHelper.Debug("connected 2:" + stage);
            }
            catch (Exception e)
            {
                //sw.util.LoggerHelper.Debug("end connect error:" + e);
 
                stage = Stage.CLOSED;
            }
            finally
            {

            }
        }
        void doSendRaw(ByteArray bt)
        {
            try
            {
             
               

                sending = true;
                stream.BeginWrite(bt.getBuffer(), 0, bt.size(), onSendEnd, stream);
                
                //sw.util.LoggerHelper.Debug("send raw:" + bt.size());
            }
            catch (Exception ex)
            {
                sending = false;
                LoggerHelper.Error("send error:" + ex);
            }
        }
       static  byte[] emptyLen = new byte[4]{0,0,0,0};
       void doSendCmd(BaseCmd cmd)
        {
          
            try
            {
                ByteArray bt = new ByteArray();
                MemoryStream ms = new MemoryStream();

                ms.Write(emptyLen, 0, emptyLen.Length);
                ms.WriteByte((byte)cmd.paramVal);
                ms.WriteByte((byte)cmd.cmdVal);
                
                if(cmd is ByteCmd)
                {
                    ByteCmd bcmd = cmd as ByteCmd;
                    ms.Write(bcmd.data, 0, bcmd.data.Length);
                    //sw.util.LoggerHelper.Debug("send byte cmd:" + bcmd.data.Length);
                }
                else
                    Serializer.Serialize(ms, cmd);
                ms.Position = 0;
                byte[] len = BitConverter.GetBytes((uint)(0x80000000|(ms.Length - 4)));
                ms.Write(len, 0, 4);
                byte[] buff = ms.ToArray();
                //sw.util.LoggerHelper.Debug("send cmd:" + cmd.cmdVal + "," + cmd.paramVal + ",len:" + buff.Length + ",type:" + cmd.GetType().Name + ",len2:" + ms.Length);
                MemoryStream ms2 = new MemoryStream(buff, 6, buff.Length - 6);
                

                sending = true;
                stream.BeginWrite(buff, 0, buff.Length, onSendEnd, stream);
                //object data = Serializer.Deserialize(CommandFactory.getType(cmd.cmdVal, cmd.paramVal), ms2);
                //sw.util.LoggerHelper.Debug("obj:" + data);
            }
            catch(Exception ex)
            {
                sending = false;
                LoggerHelper.Error("send error:" + ex);
            }
            //cmd.
            //client.BeginSend()
        }
        void trySendNext()
       {
           BaseCmd cmd = null;
           ByteArray bt = null;
           lock (lockobj)
           {
               if (sending)
                   return;
               if (m_sendRawQueue.Count>0)
               {
                   bt = m_sendRawQueue.Dequeue();
                   sending = true;
               }
               if (m_sendQueue.Count > 0)
               { 
                       cmd = m_sendQueue.Dequeue();
                       sending = true;
               }
           }
           if (bt != null)
               doSendRaw(bt);
           if (cmd != null)
               doSendCmd(cmd);
       }
        void onSendEnd(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            ns.EndWrite(ar);
            lock (lockobj)
            {
                sending = false;
            }
            trySendNext();
                
            
        }
        void onReadHead(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState; 
            int numberOfBytesRead = ns.EndRead(ar);
            if(numberOfBytesRead>=4)
            {
                recvBuff.position = 0;
                recvBuff.end = numberOfBytesRead;
                int len = (int)(BitConverter.ToUInt32(recvBuff.getBuffer(), 0) & 0xffff);
               //sw.util.LoggerHelper.Debug("begin to read len:" + len);
                stream.BeginRead(recvBuff.getBuffer(), 0, len, onReadBody, ns);
            }
            else
            {
                //sw.util.LoggerHelper.Debug("read head closed");
                stage = Stage.CLOSED;
            }
           
        }
        void onReadBody(IAsyncResult ar)
        {
             NetworkStream ns = (NetworkStream)ar.AsyncState; 
            int numberOfBytesRead = ns.EndRead(ar);
            if(numberOfBytesRead>0)
            {

           
                recvBuff.position = 0;
                recvBuff.end = numberOfBytesRead;
                byte tp2 = recvBuff.readUnsignedByte();
                byte tp1 = recvBuff.readUnsignedByte();
                
                Type tp =  CommandFactory.getType(tp1,tp2);
               // if(tp1 == 5)
                //sw.util.LoggerHelper.Debug("read cmd:" + tp + ",tp1:" + tp1 + ",tp2:" + tp2 + ",len:" + numberOfBytesRead);
               MemoryStream ms =   new MemoryStream(recvBuff.getBuffer(),2,recvBuff.size());

               if (tp != null)
               {
                   BaseCmd cmd = Serializer.Deserialize(tp, ms) as BaseCmd;

                   //BaseCmd cmd = new IncomeCmd() { cmdVal = tp1, paramVal = tp2, timestampVal = 0,data = obj };
                   lock (lockobj)
                   {
                       m_recvQueue.Enqueue(cmd);
                   }
               }
               else
                   LoggerHelper.Info("unknown cmd type:" + tp1 + "," + tp2);
                ByteCmd byteCmd = new ByteCmd(){cmdVal = tp1, paramVal = tp2,  data = ms.ToArray()};
                lock(lockobj)
                {
                    m_recvQueue.Enqueue(byteCmd);
                }
                //sw.util.LoggerHelper.Debug("begin to read head");
                stream.BeginRead(recvBuff.getBuffer(), 0, 4, onReadHead, ns);
            }
            else
            {
                LoggerHelper.Info("socket closed");
                stage = Stage.CLOSED;
            }
        }
        public void sendCmd(BaseCmd cmd)
        {
            
                 lock(lockobj)
                 {
                     
                        m_sendQueue.Enqueue(cmd);
                          
                 }
                 trySendNext(); 

                
           
        }
        public void sendRaw(ByteArray bt)
        {

            lock (lockobj)
            {
                m_sendRawQueue.Enqueue(bt);
                

            }
            trySendNext();



        }
        
        public BaseCmd getCmd()
        {
           

            switch(stage)
            {
                case Stage.CONNECTED:
                    //sw.util.LoggerHelper.Debug("connected..........."+OnConnected);
                    if (OnConnected != null)
                        OnConnected.Invoke();
                    stage = Stage.RUNNING;
                    break;
                case Stage.CLOSED:
                    if (OnClosed != null)
                        OnClosed.Invoke();
                    stage = Stage.END;
                    break;
            }
            if (m_recvQueue.Count > 0)
            {
                lock (lockobj)
                {
                    return m_recvQueue.Dequeue();
                }
            }
                
            return null;
        }


        public void Dispose()
        {
            if (m_lcClient != null)
                m_lcClient.Close();
        }
    }
}
