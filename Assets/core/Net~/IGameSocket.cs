using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;

namespace Core.Net
{
    public interface IConnection : IDisposable
    {
        void Connect(string ip, int port);
        void Connect(IPAddress ip, int port);
        void Connect(IPEndPoint romote);
        void DisConnect();
        void Close();
    }
}
