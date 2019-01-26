using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Core.Net
{

    public class Connection : IConnection
    {
        public void Close()
        {
            throw new System.NotImplementedException();
        }

        public void Connect(string ip, int port)
        {
            throw new System.NotImplementedException();
        }

        public void Connect(IPAddress ip, int port)
        {
            throw new System.NotImplementedException();
        }

        public void Connect(IPEndPoint romote)
        {
            throw new System.NotImplementedException();
        }

        public void DisConnect()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}