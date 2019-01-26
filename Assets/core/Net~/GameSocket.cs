
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Core.Net {
    public class GameSocket : IConnection
    {
        private Socket socket;
        private IPEndPoint ipEndPoint;

        public GameSocket()
        {
            if (socket != null)
            {
                if (socket.Connected)
                    socket.Disconnect(true);
            }else
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ip, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            Connect(ipAddress, port);
        }

        public void Connect(IPAddress ip, int port)
        {
           Connect(new IPEndPoint(ip, port));
        }


        ManualResetEvent connectDone = new ManualResetEvent(false);
        Reciver reciver;
        public void Connect(IPEndPoint romote)
        {
            try
            {
                ipEndPoint = romote;
                //回调声明 public delegate void AsyncCallback(IAsyncResult ar);
                socket.BeginConnect(romote, ConnectCallback, socket);
                connectDone.WaitOne();
                BeginRecive();
            }
            catch
            {

            }
        }

        #region AAAAAAAAAAAA
        private void BeginRecive()
        {
            try
            {
                if (reciver != null)
                    reciver.Close();
                else
                {
                    reciver = new Reciver();
                }
                reciver.Start(socket);
            }
            catch(Exception e)
            {

            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                SDebug.Debug("Socket connected to "+client.RemoteEndPoint.ToString());
                connectDone.Set();
            }
            catch (Exception e)
            {
                SDebug.Debug(e.ToString());
            }
        }

        #endregion

        public void Send(string msg)
        {
            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
            socket.Send(data, data.Length, SocketFlags.None);
        }

        public void Close()
        {
            socket.Close();
        }

        public void DisConnect()
        {
            socket.Disconnect(true);
        }

        public void Dispose()
        {
            if (socket != null)
            {
                if (socket.Connected)
                    socket.Disconnect(false);
                reciver.Close();
                socket.Close();
                socket.Dispose();
                socket = null;
                ipEndPoint = null;
            }
        }
    }
}