using System;

namespace UnityWebSocket.Uniform
{
    public class WebSocket : IWebSocket
    {
        #region Public Events
        public event EventHandler<OpenEventArgs> OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;
        #endregion

        public string Address { get { return _rawSocket.Address; } }

        public WebSocketState ReadyState { get { return _rawSocket.ReadyState; } }

        private readonly IWebSocket _rawSocket;

        public WebSocket(string address)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            _rawSocket = new WebGL.WebSocket(address);
#elif !NET_LEGACY
            _rawSocket = new NoWebGL.WebSocket(address);
#else
            throw new NotSupportedException("WebSocket not support .net3.5(legacy)");
#endif
            _rawSocket.OnOpen += (o, e) => OnOpen?.Invoke(this, e);
            _rawSocket.OnClose += (o, e) => OnClose?.Invoke(this, e);
            _rawSocket.OnError += (o, e) => OnError?.Invoke(this, e);
            _rawSocket.OnMessage += (o, e) => OnMessage?.Invoke(this, e);
        }


        public void SendAsync(string data)
        {
            _rawSocket.SendAsync(data);
        }

        public void SendAsync(byte[] data)
        {
            _rawSocket.SendAsync(data);
        }

        public void ConnectAsync()
        {
            _rawSocket.ConnectAsync();
        }

        public void CloseAsync()
        {
            _rawSocket.CloseAsync();
        }
    }
}
