using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


namespace Core.Net
{
    public class CmdCache:Singletion<CmdCache>
    {
        Queue<byte[]> reciveMegQueue = new Queue<byte[]>();
        Queue<byte[]> sendMegQueue = new Queue<byte[]>();
        public void AddMsg(byte[] rawData)
        {
            reciveMegQueue.Enqueue(rawData);
        }

        public void Update()
        {
            if (reciveMegQueue != null && reciveMegQueue.Count > 0)
            {
                do
                {
                    byte[] data = reciveMegQueue.Dequeue();
                    DisPatcher(data);
                }
                while (reciveMegQueue.Count > 0);
            }
        }

        private void DisPatcher(byte[] data)
        {
            //测试
            string content = System.Text.ASCIIEncoding.ASCII.GetString(data);
            SDebug.Debug("拍发消息： " + content);
        }
    }
}
