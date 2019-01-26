using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
 
using sw.util;
using System.IO;
using System.Diagnostics;
using ProtoCmd;
using ProtoBuf;
using Cmd;
 
namespace Common
{
    public class GameSocket2 : IDisposable
    {
        private TcpClient m_lcClient;
        private string m_ip;
        private uint m_port;
        private Thread m_connectThread;
        private Thread m_recvThread;
        private Thread m_sendThread;
        private volatile bool stopFlag;
        private Queue<BaseCmd> m_sendQueue;
        private Queue<ByteArray2> m_sendRawQueue;
        private Queue<BaseCmd> m_recvFirstQueue;
        private Queue<BaseCmd> m_recvQueue;
        private CmdBuilder m_builder;
        private ByteArray2 m_sendBuff;
        int[] sendBytes = new int[4] { 0, 0, 0, 0 };
        int[] recvBytes = new int[4] { 0, 0, 0, 0 };
        int lastSecond=0;
 
        const int MAXRETRY = 10;
        static object lockobj = new object();
        public const string SHOW_CMD_FILTER = "socket.show.cmd.filter";
        public const string SHOW_SENDCMD_FILTER = "socket.showsend.cmd.filter";
        public const string SHOW_RECV_BITRATE = "socket.show.recv.bitrate";
        public const string UPDATE_RECV_BITRATE = "socket.update.recv.bitrate";
        public const string UPDATE_SEND_BITRATE = "socket.send.recv.bitrate";
        public const string UPDATE_CMD_COUNT = "update.cmd.count";
        public const string UPDATE_LAST_COUNT = "update.last.cmd.count";

        public GameSocket2(Dictionary<int, int> csCmdDict, Dictionary<int, int> csAndLuaCmdDict)
        {
            m_builder = new CmdBuilder();
            SetCsCmdDict(csCmdDict);
            SetCsAndLuaCmdDict(csAndLuaCmdDict);
            m_builder.recvCallback = (onRecvCmd);
        }

        private double _onShowCmdFilterIndex;
        public void start(string ip, uint port)
        {
            //sw.util.LoggerHelper.Debug("start connect:" + port + "," + ip);
            m_ip = ip;
            m_port = port;
            m_connectThread = new Thread(new ThreadStart(connectThread));

            m_connectThread.Start();
            m_sendQueue = new Queue<BaseCmd>();
            m_recvQueue = new Queue<BaseCmd>();
            m_recvFirstQueue = new Queue<BaseCmd>();
            m_sendRawQueue = new Queue<ByteArray2>();
            m_sendBuff = new ByteArray2(false);
            stopFlag = false;
            _onShowCmdFilterIndex = EventDispatcher.AddEventListener(SHOW_CMD_FILTER, onShowCmdFilter);
            EventDispatcher.AddEventListener(SHOW_SENDCMD_FILTER, onShowSendCmdFilter);

        }
        object onGetRecvBitrate()
        {
            long total = 0;
            for (int i = 0; i < 3;i++ )
                total += m_builder.recvBytes[i];
            return (int)(total/3);
        }
        private void SetCsCmdDict(Dictionary<int, int> dict)
        {
            m_builder.SetCsCmdDict(dict);
        }

        private void SetCsAndLuaCmdDict(Dictionary<int, int> dict)
        {
            m_builder.SetCsAndLuaCmdDict(dict);
        }

        Dictionary<int, Dictionary<int, int>> showCmdFilter;
        List<BaseCmd> toShowCmd;
        void onShowCmdFilter(params object[] arg)
        {
            int tp = (int)arg[0];
            int subtp = (int)arg[1];
            bool show = (bool)arg[2];
            if (show)
            {
                if (showCmdFilter == null)
                    showCmdFilter = new Dictionary<int, Dictionary<int, int>>();
                if (!showCmdFilter.ContainsKey(tp))
                    showCmdFilter[tp] = new Dictionary<int, int>();
                showCmdFilter[tp][subtp] = 1;
            }
            else if (showCmdFilter != null && showCmdFilter.ContainsKey(tp) && showCmdFilter[tp].ContainsKey(subtp))
            {
                showCmdFilter[tp].Remove(subtp);
                if (showCmdFilter[tp].Count == 0)
                    showCmdFilter.Remove(tp);
            }
        }
        Dictionary<int, Dictionary<int, int>> showSendCmdFilter;

        void onShowSendCmdFilter(params object[] arg)
        {
            int tp = (int)arg[0];
            int subtp = (int)arg[1];
            bool show = (bool)arg[2];
            if (show)
            {
                if (showSendCmdFilter == null)
                    showSendCmdFilter = new Dictionary<int, Dictionary<int, int>>();
                if (!showSendCmdFilter.ContainsKey(tp))
                    showSendCmdFilter[tp] = new Dictionary<int, int>();
                showSendCmdFilter[tp][subtp] = 1;
            }
            else if (showSendCmdFilter != null && showSendCmdFilter.ContainsKey(tp) && showSendCmdFilter[tp].ContainsKey(subtp))
            {
                showSendCmdFilter[tp].Remove(subtp);
                if (showSendCmdFilter[tp].Count == 0)
                    showSendCmdFilter.Remove(tp);
            }
        }
        public void sendCmd(BaseCmd cmd)
        {
            if (stopFlag)
                return;
            lock (lockobj)
            {
               // LoggerHelper.Critical2("GameSocket2-onSendCmd:" + cmd.cmdVal + "," + cmd.paramVal + ",name:" + cmd.GetType().Name + ",queue len:" + m_sendQueue.Count);
                m_sendQueue.Enqueue(cmd);
               
            }
            if (showSendCmdFilter != null && showSendCmdFilter.ContainsKey((int)cmd.cmdVal) && showSendCmdFilter[(int)cmd.cmdVal].ContainsKey((int)cmd.paramVal))
            {
                stChannelChatUserCmd logcmd = new stChannelChatUserCmd();
                logcmd.pstrChat = "onSendCmd:" + cmd.cmdVal + "," + cmd.paramVal + ",name:" + cmd.GetType().Name + ",queue len:" + m_sendQueue.Count;
                logcmd.dwType = enumChatType.CHAT_TYPE_DRAGONZONES;
                //sw.util.LoggerHelper.Debug(logcmd.pstrChat);
                EventDispatcher.DispatchEvent("parse_chat", logcmd);
            }
        }
        public void setTime(float tm)
        {
            m_builder.setTime(tm);
        }
        public void sendRaw(ByteArray2 bts)
        {
            lock (lockobj)
            {
                m_sendRawQueue.Enqueue(bts);
            }
        }
        public int getCmdCount()
        {
            return m_recvQueue.Count;
        }

        public int getFirstCmdCount()
        {
            return m_recvFirstQueue.Count;
        }
        Dictionary<string, int> dicCmd = new Dictionary<string, int>();
        public Dictionary<string, int> getAllCmd()
        {
            if (getCmdCount()>10)
            {
                dicCmd.Clear();
                BaseCmd[] cmds = m_recvQueue.ToArray();
                for (int i = 0; i < cmds.Length; i++)
                {
                    BaseCmd cmd = cmds[i];
                    if (!dicCmd.ContainsKey(cmd.ToString()))
                        dicCmd.Add(cmd.ToString(), 0);
                    dicCmd[cmd.ToString()]++;
                }
                return dicCmd;
            }
            return null;

        }
        public int remainCmdCount()
        {
            return m_recvQueue.Count;
        }

        public BaseCmd GetFirstCmd()
        {
            if(m_recvFirstQueue.Count>0)
            {
                lock (lockobj)
                {
                    return m_recvFirstQueue.Dequeue();
                }
            }
            return null;
        }

        public BaseCmd getCmd()
        {
            if (toShowCmd != null && toShowCmd.Count > 0)
            {
                lock (lockobj)
                {
                    for (int i = 0; i < toShowCmd.Count; i++)
                    {
                        stChannelChatUserCmd logcmd = new stChannelChatUserCmd();
                        logcmd.pstrChat = "onRecvCmd:" + toShowCmd[i].cmdVal + "," + toShowCmd[i].paramVal + ",name:" + toShowCmd[i].GetType().Name + ",queue len:" + m_sendQueue.Count;
                        logcmd.dwType = enumChatType.CHAT_TYPE_DRAGONZONES;
                        //sw.util.LoggerHelper.Debug(logcmd.pstrChat);
                        EventDispatcher.DispatchEvent("parse_chat", logcmd);
                    }
                    toShowCmd.Clear();
                }
            }

            if (m_recvQueue.Count > 0)
            {
                lock (lockobj)
                {


                    if (showCmdFilter != null)
                    {
                        BaseCmd cmd = m_recvQueue.Peek();
                        if (showCmdFilter.ContainsKey((int)cmd.cmdVal) && showCmdFilter[(int)cmd.cmdVal].ContainsKey((int)cmd.paramVal))
                        {
                            stChannelChatUserCmd logcmd = new stChannelChatUserCmd();
                            logcmd.pstrChat = "ongetcmd:" + cmd.cmdVal + "," + cmd.cmdVal + ",name:" + cmd.GetType().Name + ",queue len:" + m_sendQueue.Count;
                            logcmd.dwType = enumChatType.CHAT_TYPE_DRAGONZONES;
                            //sw.util.LoggerHelper.Debug(logcmd.pstrChat);
                            EventDispatcher.DispatchEvent("parse_chat", logcmd);
                        }
                    }
                    return m_recvQueue.Dequeue();
                }
            }
            return null;
        }
        private void connectThread()
        {
            try
            {
                if (m_lcClient != null)
                {
                    m_lcClient.Close();
                    m_lcClient = null;
                }
                m_lcClient = new TcpClient();
                IPAddress addr;
                if (!IPAddress.TryParse(m_ip, out addr))
                {
                    addr = Dns.GetHostEntry(m_ip).AddressList[0];

                }
                //LoggerHelper.Debug("addr before:" + addr.ToString() + ",m_ip:" + m_ip + ",family:" + addr.AddressFamily);
#if UNITY_IPHONE
                //IPV6
                if(addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    String newServerIp;
                    AddressFamily newAddressFamily = AddressFamily.InterNetwork;
                    IPv6SupportMidleware.getIPType(addr.ToString(), m_port.ToString(), out newServerIp, out newAddressFamily);
                //LoggerHelper.Debug("newServerIp:" + newServerIp+",family:"+newAddressFamily);
                    if (!string.IsNullOrEmpty(newServerIp)) {
                        addr = IPAddress.Parse(newServerIp);                 
                    }
                }
#endif
                //LoggerHelper.Debug("addr:" + addr.ToString() + ",family:" + addr.AddressFamily);
                m_lcClient.Connect(addr.ToString(),(int) m_port);
                //LoggerHelper.Debug("connected ");
            }
            catch (Exception e)
            {
                //LoggerHelper.Error("connect " + m_ip + ":" + m_port + "  error:" + e);
                this.m_lcClient.Close();
                m_lcClient = null;
                if (OnClosed != null)
                    OnClosed();
                return;
            }
            m_connectThread = null;
           
            m_recvThread = new Thread(new ThreadStart(recvThread));
            m_sendThread = new Thread(new ThreadStart(sendThread));

            m_recvThread.Start();
            m_sendThread.Start();
            if (OnConnected != null)
                OnConnected();

        }
        private void onRecvCmd(BaseCmd cmd)
        {
            //sw.util.LoggerHelper.Debug("onRecvCmd:"+cmd.byCmd+","+cmd.byParam);
            if (stopFlag)
                return;
            lock (lockobj)
            {
                if(cmd.cmdVal==2)
                {
                    m_recvFirstQueue.Enqueue(cmd);
                }
                else
                {
                    m_recvQueue.Enqueue(cmd);
                }
                
                //sw.util.LoggerHelper.Debug("onRecvCmd:UPDATE_CMD_COUNT:" + m_recvQueue.Count);
                //EventDispatcher.DispatchEvent(UPDATE_CMD_COUNT, m_recvQueue.Count);
            }

            //if (showCmdFilter != null && showCmdFilter.ContainsKey((int)cmd.cmdVal) && showCmdFilter[(int)cmd.cmdVal].ContainsKey((int)cmd.paramVal))
            //{
            //    lock (lockobj)
            //    {
            //        if (toShowCmd == null)
            //            toShowCmd = new List<BaseCmd>();
            //        toShowCmd.Add(cmd);
            //    }

            //}
        }
        static byte[] emptyLen = new byte[4] { 0, 0, 0, 0 };
        private void sendThread()
        {
            //LoggerHelper.Debug("sendThread start:" + m_lcClient + "," + m_ip + "," + m_port);

            if (m_lcClient != null && m_lcClient.Connected)
            {
                //LoggerHelper.Debug("start send loop");
                while (!stopFlag)
                {
                    try
                    {
                        NetworkStream stream = m_lcClient.GetStream();
                        if (stream.CanWrite)
                        {
                            if (m_sendRawQueue.Count > 0)
                            {

                                ByteArray2 raw;
                                lock (lockobj)
                                {
                                    raw = m_sendRawQueue.Dequeue();
                                }
                                if (raw != null)
                                {
                                    stream.Write(raw.getBuffer(), raw.position, raw.size());
                                    //sw.util.LoggerHelper.Debug("send raw bytes:" + raw.size());
                                    stream.Flush();
                                }
                            }
                            else if (m_sendQueue.Count > 0)
                            {
                                BaseCmd cmd;
                                lock (lockobj)
                                {
                                    cmd = m_sendQueue.Peek();
                                }
                                if (cmd != null)
                                {

                                    //ByteArray2 bt = new ByteArray2();
                                    MemoryStream ms = new MemoryStream();

                                    ms.Write(emptyLen, 0, emptyLen.Length);
                                    ms.WriteByte((byte)cmd.paramVal);
                                    ms.WriteByte((byte)cmd.cmdVal);

                                    if (cmd is ByteCmd)
                                    {
                                        ByteCmd bcmd = cmd as ByteCmd;
                                        ms.Write(bcmd.data, 0, bcmd.data.Length);
                                        //sw.util.LoggerHelper.Debug("send byte cmd:" + bcmd.data.Length);
                                    }
                                    else
                                        Serializer.Serialize(ms, cmd);
                                    ms.Position = 0;
                                    byte[] len = BitConverter.GetBytes((uint)(0x80000000 | (ms.Length - 4)));
                                    ms.Write(len, 0, 4);
                                    byte[] buff = ms.ToArray();



                                    //m_sendBuff.reset();
                                    //m_sendBuff.write((Int32)0);
                                    //cmd.write(m_sendBuff);
                                    //byte[] lenBuff = BitConverter.GetBytes(m_sendBuff.size() - 4);
                                    //for (int i = 0; i < 4; i++)
                                    //    m_sendBuff.getBuffer()[m_sendBuff.position + i] = lenBuff[i];
                                    try
                                    {
                                        stream.Write(buff, 0, buff.Length);
                                        //stream.Flush();
                                        //sw.util.LoggerHelper.Debug("send cmd bytes:" + m_sendBuff.size());
                                        m_sendQueue.Dequeue();
                                    }
                                    catch (IOException ex)
                                    {
                                        //sw.util.LoggerHelper.Debug("write error");
                                        if (OnClosed != null)
                                            OnClosed();
                                        return;
                                    }
                                    //if (cmd is stUserMoveMoveUserCmd)
                                    //{
                                    //    stUserMoveMoveUserCmd moveCmd = cmd as stUserMoveMoveUserCmd;
                                    //    sw.util.LoggerHelper.Debug("send move cmd2:" + moveCmd.x + "," + moveCmd.y + "," + moveCmd.fromx + "," + moveCmd.fromy + "," + moveCmd.toX + "," + moveCmd.toY);
                                    //    sw.util.LoggerHelper.Debug("write len:" + m_sendBuff.position + "," + m_sendBuff.size());
                                    //}
                                    //sw.util.LoggerHelper.Debug("begin send cmd:" + cmd.GetType().Name + "," + cmd.byParam + "," + m_sendBuff.position + "," + m_sendBuff.size());


                                }
                            }
                            else
                                Thread.Sleep(50);
                        }
                        else
                            Thread.Sleep(50);
                    }
                    catch (IOException)
                    {
                        //LoggerHelper.Error("socket closed");
                        if (OnClosed != null)
                            OnClosed();
                    }
                    catch (Exception e)
                    {
                        // LoggerHelper.Error("exception :" + e + "," + m_ip + "," + m_port, true);

                    }
                }
            }

            //LoggerHelper.Debug("sendThread end:" + m_lcClient + "," + m_ip + "," + m_port);
        }
        private void recvThread()
        {
            //LoggerHelper.Debug("recvThread start:" + m_lcClient + "," + m_port + "," + m_ip);
            try
            {
                if (m_lcClient != null && m_lcClient.Connected)
                {
                    NetworkStream stream = m_lcClient.GetStream();
                    uint cnt = 0;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    while (!stopFlag && stream.CanRead)
                    {

                        if (!m_builder.read(stream))
                        {
                            if (OnClosed != null)
                                OnClosed();
                            break;
                        }
                        //sw.util.LoggerHelper.Debug("stream can read");
                        cnt++;
                        if (cnt % 100 == 0)
                        {
                            //sw.util.LoggerHelper.Debug("100 times time:" +  sw.ElapsedMilliseconds);
                            sw.Reset();
                            sw.Start();
                        }
                        //if (stream.CanRead && stream.DataAvailable)
                        //{
                        //    //sw.util.LoggerHelper.Debug("stream can read");


                        //    //int len = stream.Read(buff, 0, 1);

                        //    m_builder.read(stream);


                        //}
                        //else
                        //    Thread.Sleep(50);
                    }

                    LoggerHelper.Warning("stream can read" + stream.CanRead + ",read thread end ");

                }
            }
            catch (IOException ex)
            {
                //LoggerHelper.Error("socket closed:" + ex);
                if (OnClosed != null)
                    OnClosed();
            }
            catch (Exception e)
            {
                //LoggerHelper.Error("recv error:" + e.Message + "," + m_lcClient + "," + m_port + "," + m_ip);
            }
            //LoggerHelper.Debug("recvThread end :" + m_port + "," + m_ip);
        }


        public Action OnConnected;
        public Action OnClosed;

        public void Dispose()
        {
            //LoggerHelper.Debug("begin to dispose:"+m_ip);
            EventDispatcher.RemoveEventListener(SHOW_CMD_FILTER, _onShowCmdFilterIndex);
            OnClosed = null;
            OnConnected = null;
            if (m_lcClient != null && m_lcClient.Connected)
            {
                m_lcClient.GetStream().Close();
                m_lcClient.Close();
            }

            stopFlag = true;

            if (m_connectThread != null)
            {
                //sw.util.LoggerHelper.Debug("begin join connect thread");
                //m_connectThread.Join(2000);
                //sw.util.LoggerHelper.Debug("end join connect thread");
                m_connectThread.Abort();
            }
            m_connectThread = null;
            if (m_sendThread != null)
            {
                //sw.util.LoggerHelper.Debug("begin join send thread");
                //m_sendThread.Join(2000);
                //sw.util.LoggerHelper.Debug("end join send thread");
                m_sendThread.Abort();
            }

            m_sendThread = null;
            if (m_recvThread != null)
            {
                //sw.util.LoggerHelper.Debug("begin join send thread");
                //m_recvThread.Join(2000);
                //sw.util.LoggerHelper.Debug("end join send thread");
                m_recvThread.Abort();
            }
            m_recvThread = null;
             
            m_recvQueue.Clear();
            //sw.util.LoggerHelper.Debug("end to dispose");
        }
    }
}
