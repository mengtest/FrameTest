using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Core.Net
{
    public class NetException : Exception
    {
        public NetException()
        {

        }
        public NetException(string message):base(message)
        {

        }
        public NetException(string message, Exception innerException) : base(message, innerException)
        {

        }
        protected NetException(SerializationInfo info, StreamingContext context): base(info, context)
        {

        }
    }
}
