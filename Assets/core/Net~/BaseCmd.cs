using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.Net
{
    public class BaseCmd
    {
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }
}
