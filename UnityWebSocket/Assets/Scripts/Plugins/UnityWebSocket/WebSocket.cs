using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityWebSocket
{
    public class WebSocket
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        public string address { get { return m_rawSocket.address; } }
        public State state { get { return (State)m_rawSocket.state; } }
        public Action onOpen { get; set; }
        public Action onClose { get; set; }
        public Action<string> onError { get; set; }
        public Action<byte[]> onReceive { get; set; }

        WebSocketJslib.WebSocket m_rawSocket = null;
        public WebSocket(string address)
        {
            m_rawSocket = new WebSocketJslib.WebSocket(address);
            m_rawSocket.onOpen = () => { if (onOpen != null) onOpen.Invoke(); };
            m_rawSocket.onClose = () => { if (onClose != null) onClose.Invoke(); }; 
            m_rawSocket.onError = (a) => { if (onError != null) onError.Invoke(a); };
            m_rawSocket.onReceive = (a) => { if (onReceive != null) onReceive.Invoke(a); };
        }

        public void Connect()
        {
            m_rawSocket.Connect();
        }

        public void Send(byte[] data)
        {
            m_rawSocket.Send(data);
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
        public State state { get { return (State)m_rawSocket.ReadyState; } }
        public Action onOpen { get; set; }
        public Action onClose { get; set; }
        public Action<string> onError { get; set; }
        public Action<byte[]> onReceive { get; set; }

        WebSocketSharp.WebSocket m_rawSocket = null;
        public WebSocket(string address)
        {
            m_rawSocket = new WebSocketSharp.WebSocket(address);
            m_rawSocket.Close();
            m_rawSocket.OnOpen += (o, e) => { if (onOpen != null) onOpen.Invoke(); };
            m_rawSocket.OnClose += (o, e) => { if (onClose != null) onClose.Invoke(); };
            m_rawSocket.OnError += (o, e) => { if (onError != null) onError.Invoke(e.Message); };
            m_rawSocket.OnMessage += (o, e) => { if (onReceive != null) onReceive.Invoke(e.RawData); };
        }

        public void Connect()
        {
            m_rawSocket.Connect();
        }

        public void Send(byte[] data)
        {
            m_rawSocket.Send(data);
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

        /// <summary>
        /// 参考 html5 WebSocket ReadyState 属性
        /// </summary>
        public enum State
        {
            Connecting,
            Connected,
            Closing,
            Closed,
        }
    }
}
