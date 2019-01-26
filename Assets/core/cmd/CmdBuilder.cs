using System.Net.Sockets;

using System.IO;
using UnityEngine;
using ProtoCmd;
using System;
using sw.util;
using ProtoBuf;
using System.Diagnostics;
using System.Collections.Generic;
using Common;

namespace Cmd
{
    public delegate void OnRecvCmd(BaseCmd cmd);

    public class CmdBuilder
    {
        public OnRecvCmd recvCallback;
        private ByteArray2 m_buffer;
        private uint m_contentLen;
        private bool m_isCompress;
        private ByteArray2 m_cmdTemp;

        internal int[] recvBytes = new int[4] { 0, 0, 0, 0 };
        int lastSecond = 0;
        internal long allBytes = 0;

        private long msLen = 0;
        public CmdBuilder()
        {
            m_buffer = new ByteArray2(true);
            m_cmdTemp = new ByteArray2(false);
            m_contentLen = 0;
            m_isCompress = false;
        }
        Dictionary<int, int> csCmdDict;
        public void SetCsCmdDict(Dictionary<int, int> dict)
        {
            csCmdDict = dict;
        }
        Dictionary<int, int> csAndLuaCmdDict;
        public void SetCsAndLuaCmdDict(Dictionary<int, int> dict)
        {
            csAndLuaCmdDict = dict;
        }
        public void setTime(float tm)
        {
            int curSecond = (int)tm;
            if (curSecond != lastSecond)
            {
                long total = 0;
                for (int i = 0; i < 3; i++)
                {
                    recvBytes[i] = recvBytes[i + 1];
                    total += recvBytes[i];
                }
                recvBytes[3] = 0;
                //EventDispatcher.DispatchEvent(GameSocket2.UPDATE_RECV_BITRATE, (int)(total / 3));
            }
        }

        private void SetLuaCmd(byte tp1,byte tp2, MemoryStream ms)
        {
            ByteCmd byteCmd = new ByteCmd() { cmdVal = tp1, paramVal = tp2, data = ms.ToArray() };
            //UnityEngine.Debug.Log("----MemoryStream--SetLuaCmd:"+ byteCmd.cmdVal+","+byteCmd.paramVal+":"+ ms.Length+"  sum:"+msLen);
            recvCallback(byteCmd);
        }

        //private Dictionary<byte, List<string>> _logDic = new Dictionary<byte, List<string>>();

        private void SetCsCmd(byte tp1, byte tp2,Type tp, MemoryStream ms)
        {
            if (tp != null)
            {
                try
                {
                    //string msg = tp1 + "_" + tp2;
                    //List<string> list = null;
                    //if(_logDic.ContainsKey(tp1) == false)
                    //{
                    //    list = new List<string>();
                    //    _logDic.Add(tp1, list);
                    //}else
                    //{
                    //    list = _logDic[tp1];
                    //}
                    //if(list.Contains(msg) == false)
                    //{
                    //    list.Add(msg);
                    //}
                    //string logStr = "";
                    //foreach(KeyValuePair<byte,List<string>> kv in _logDic)
                    //{
                    //    List<string> ll = kv.Value;
                    //    logStr += "\n";
                    //    foreach (string str in ll)
                    //    {
                    //        logStr += str + ",";
                    //    }
                    //}
                    //LoggerHelper.Error("_logStr:" + logStr);

                    BaseCmd cmd = Serializer.Deserialize(tp, ms) as BaseCmd;
                    //UnityEngine.Debug.Log("----MemoryStream--SetCsCmd:" + cmd.cmdVal + "," + cmd.paramVal + ":" + ms.Length+"   sum:"+msLen);
                    //sw.util.LoggerHelper.Debug("get cmd:" + cmd);
                    recvCallback(cmd);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error("Deserialize error:" + ex);
                }

                //BaseCmd cmd = new IncomeCmd() { cmdVal = tp1, paramVal = tp2, timestampVal = 0,data = obj };

            }
            else
                LoggerHelper.Info("unknown cmd type:" + tp1 + "," + tp2);
        }

        public bool read(NetworkStream stream)
        {
            if (recvCallback == null)
                return false;
            if (m_buffer.remains() < 65536)
                m_buffer.rearrange();
            //sw.util.LoggerHelper.Debug("begin recv");
            int len = m_buffer.recv(stream);
            //sw.util.LoggerHelper.Debug("recv data:" + m_buffer.position + "," + m_buffer.size() + "," + len + "," + m_buffer.remains());
            if (len == 0)
            {
                return false;
            }
                
         
            recvBytes[3] = recvBytes[3]+len;
            allBytes +=len;
            while (true)
            {
                //UnityEngine.Debug.Log("Cur leng " + len);
                if (m_contentLen == 0 && m_buffer.size() < 6)
                    break;
                if (m_contentLen == 0)
                {
                    uint head = m_buffer.readUnsignedInt();// BitConverter.ToUInt16(m_buffer.getBuffer(), m_buffer.position);
                    m_isCompress = (head & 0x40000000) > 0;
                    this.m_contentLen = head & 0xffff;
                    //sw.util.LoggerHelper.Debug("cotent len:" + this.m_contentLen + "," + m_buffer.size() + "," + m_isCompress);

                }
 
                if (m_buffer.size() >= m_contentLen)
                {


                    int nextPos = m_buffer.position + (int)m_contentLen;
                    byte tp2 = m_buffer.readUnsignedByte();
                    byte tp1 = m_buffer.readUnsignedByte();

                    Type tp = CommandFactory.getType(tp1, tp2);
                    //if(tp1 == 4 && tp2 == 35)
                    //sw.util.LoggerHelper.Debug("read cmd:" + tp + ",tp1:" + tp1 + ",tp2:" + tp2+","+m_contentLen   );
                    MemoryStream ms = new MemoryStream(m_buffer.getBuffer(), m_buffer.position, (int)m_contentLen-2);
                    msLen += ms.Length;
                    Stopwatch w = new Stopwatch();                    
                    int onlyKey = (tp1 << 8) + tp2;
                    bool iscsAndLua = csAndLuaCmdDict.ContainsKey(onlyKey);
                    if (iscsAndLua)//说明这个消息cs lua 都要侦听
                    {
                        SetLuaCmd(tp1, tp2, ms);
                        SetCsCmd(tp1, tp2, tp, ms);
                    }
                    else              //说明这个消息cs  lua  只能同时存在一个方式
                    {
                        if (csCmdDict.ContainsKey(onlyKey) == false)//lua专属消息
                        {
                            SetLuaCmd(tp1, tp2, ms);
                        }
                        else                                        //c#专属消息
                        {
                            SetCsCmd(tp1, tp2, tp, ms);
                        }
                    }




                    m_contentLen = 0;
                    m_buffer.position = nextPos;

                }
                else
                {
                    // sw.util.LoggerHelper.Debug("data too short:" + m_buffer.size() + "," + m_contentLen);
                    break;
                }
            }
            return true;

        }
    }
}
