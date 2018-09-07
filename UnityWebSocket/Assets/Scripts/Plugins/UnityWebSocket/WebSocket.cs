using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityWebSocket
{
    /// <summary>
    /// For All Platform
    /// </summary>
    public class WebSocket : IWebSocket
    {
        #region events
        public event EventHandler onOpen;
        public event EventHandler<CloseEventArgs> onClose;
        public event EventHandler<ErrorEventArgs> onError;
        public event EventHandler<MessageEventArgs> onReceive;
        #endregion

#if UNITY_WEBGLs
        public string address { get { return m_rawSocket.address; } }
        public State state { get { return (State)m_rawSocket.state; } }

        WebSocketJslib.WebSocket m_rawSocket = null;
        public WebSocket(string address)
        {
            m_rawSocket = new WebSocketJslib.WebSocket(address);
            m_rawSocket.onOpen = () =>
            {
                if (onOpen != null)
                    onOpen(this, EventArgs.Empty);
            };
            m_rawSocket.onClose = () =>
            {
                if (onClose != null)
                    onClose(this, new CloseEventArgs(e.Code, e.Reason));
            };
            m_rawSocket.onError = (errMsg) =>
            {
                if (onError != null)
                    onError(this, new ErrorEventArgs(errMsg, new Exception(errMsg)));
            };
            m_rawSocket.onReceive = (a) => {
                if (onReceive != null) onReceive.Invoke(a);
            };
        }

        public void Connect()
        {
            m_rawSocket.Connect();
        }

        public void Send(byte[] data)
        {
            m_rawSocket.Send(data);
        }

        public void Send(string data)
        {
        }

        public void Ping()
        {
        }

        public void Close()
        {
            m_rawSocket.Close();
        }

        public void ConnectAsync()
        {
            Connect();
        }

        public void CloseAsync()
        {
            Close();
        }

        public void SendAsync(byte[] data, Action<bool> completed)
        {
            Send(data);
            completed(true);
        }
#else
        public string address { get { return m_rawSocket.Url.AbsoluteUri; } }
        public WebSocketState state { get { return (WebSocketState)m_rawSocket.ReadyState; } }
        WebSocketSharp.WebSocket m_rawSocket = null;

        public WebSocket(string address)
        {
            m_rawSocket = new WebSocketSharp.WebSocket(address);
            m_rawSocket.Close();
            m_rawSocket.OnOpen += (o, e) =>
            {
                if (onOpen != null)
                    onOpen(this, EventArgs.Empty);
            };
            m_rawSocket.OnClose += (o, e) =>
            {
                if (onClose != null)
                    onClose(this, new CloseEventArgs(e.Code, e.Reason));
            };
            m_rawSocket.OnError += (o, e) =>
            {
                if (onError != null)
                    onError(this, new ErrorEventArgs(e.Message, e.Exception));
            };
            m_rawSocket.OnMessage += (o, e) =>
            {
                if (onReceive != null)
                    onReceive(this, new MessageEventArgs((Opcode)e.Opcode, e.RawData));
            };
        }

        public void Connect()
        {
            m_rawSocket.Connect();
        }

        public void Send(byte[] data)
        {
            m_rawSocket.Send(data);
        }

        public void Send(string data)
        {
            m_rawSocket.Send(data);
        }

        public void Ping()
        {
            m_rawSocket.Ping();
        }

        public void Close()
        {
            m_rawSocket.Close();
        }

        public void ConnectAsync()
        {
            m_rawSocket.ConnectAsync();
        }

        public void CloseAsync()
        {
            m_rawSocket.CloseAsync();
        }

        public void SendAsync(byte[] data, Action<bool> completed)
        {
            m_rawSocket.SendAsync(data, completed);
        }
#endif

    }
}
