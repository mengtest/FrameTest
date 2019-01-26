using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


namespace Core.Net
{
    public class Sender
    {
        private Socket workSocket;
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private Thread thread;

        public void Start(Socket client)
        {
            workSocket = client;
            if (thread != null)
            {
                thread.Abort("New Start Abort this old!");
                thread = null;
            }

            thread = new Thread(Process);
        }

        private void Process()
        {
            try
            {
                while (true)
                {
                    Receive();
                    receiveDone.WaitOne();
                }
            }
            catch (ThreadAbortException ex)
            {
                SDebug.Debug(ex.Message);
                return;
            }
            catch (Exception e)
            {
                SDebug.Error(e.Message);
            }
        }

        public void Receive()
        {
            try
            {
                BaseCmd state = new BaseCmd();
                workSocket.BeginReceive(state.buffer, 0, BaseCmd.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), workSocket);
            }
            catch (Exception e)
            {
            }
        }



        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                BaseCmd state = new BaseCmd();
                Socket client = (Socket)ar.AsyncState;
                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, BaseCmd.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    if (state.sb.Length > 1)
                    {
                        //TODO 数据处理
                        //response = state.sb.ToString();
                    }
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
            }
        }

        public void Reset()
        {

        }
    }
}
