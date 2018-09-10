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
        public event EventHandler<MessageEventArgs> onMessage;
        #endregion

#if UNITY_WEBGL && !UNITY_EDITOR
        public string address { get { return m_rawSocket.address; } }
        public WebSocketState readyState { get { return m_rawSocket.readyState; } }

        WebSocketJslib.WebSocket m_rawSocket = null;
        public WebSocket(string address)
        {
            m_rawSocket = new WebSocketJslib.WebSocket(address);
            m_rawSocket.onOpen += (o, e) =>
            {
                if (onOpen != null)
                    onOpen(this, EventArgs.Empty);
            };
            m_rawSocket.onClose += (o, e) =>
            {
                if (onClose != null)
                    onClose(this, new CloseEventArgs(e.Code, e.Reason, e.WasClean));
            };
            m_rawSocket.onError += (o, e) =>
            {
                if (onError != null)
                    onError(this, new ErrorEventArgs(e.Message, e.Exception));
            };
            m_rawSocket.onMessage += (o, e) =>
            {
                if (onMessage != null)
                    onMessage(this, new MessageEventArgs((Opcode)e.Opcode, e.RawData));
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
            throw new NotImplementedException("WebGL Platform Ping Has Not Implemented Yet!");
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
        public WebSocketState readyState { get { return (WebSocketState)m_rawSocket.ReadyState; } }
        WebSocketSharp.WebSocket m_rawSocket = null;

        public WebSocket(string address)
        {
            m_rawSocket = new WebSocketSharp.WebSocket(address);
            m_rawSocket.OnOpen += (o, e) =>
            {
                if (onOpen != null)
                    onOpen(this, EventArgs.Empty);
            };
            m_rawSocket.OnClose += (o, e) =>
            {
                if (onClose != null)
                    onClose(this, new CloseEventArgs(e.Code, e.Reason, e.WasClean));
            };
            m_rawSocket.OnError += (o, e) =>
            {
                if (onError != null)
                    onError(this, new ErrorEventArgs(e.Message, e.Exception));
            };
            m_rawSocket.OnMessage += (o, e) =>
            {
                if (onMessage != null)
                    onMessage(this, new MessageEventArgs((Opcode)e.Opcode, e.RawData));
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
