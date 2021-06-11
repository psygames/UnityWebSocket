#if UNITY_EDITOR || !UNTIY_WEBGL
using System;

namespace UnityWebSocket.NoWebGL.Sharp
{
    public class WebSocket : IWebSocket
    {
        public string Address { get { return socket.Url.AbsoluteUri; } }

        public WebSocketState ReadyState { get { return (WebSocketState)socket.ReadyState; } }

        public event EventHandler<OpenEventArgs> OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;

        private WebSocketSharp.WebSocket socket;

        public WebSocket(string address)
        {
            socket = new WebSocketSharp.WebSocket(address);
            socket.OnOpen += (o, e) =>
            {
                if (OnOpen != null) OnOpen(this, new OpenEventArgs());
            };
            socket.OnClose += (o, e) =>
            {
                if (OnClose != null) OnClose(this, new CloseEventArgs(e.Code, e.Reason));
            };
            socket.OnError += (o, e) =>
            {
                if (OnError != null) OnError(this, new ErrorEventArgs(e.Message, e.Exception));
            };
            socket.OnMessage += (o, e) =>
            {
                if (OnMessage != null) OnMessage(this, new MessageEventArgs((Opcode)e.Opcode, e.RawData));
            };
            if (socket.IsSecure)
            {
                socket.SslConfiguration.EnabledSslProtocols = 
                    (System.Security.Authentication.SslProtocols)
                    ((int)socket.SslConfiguration.EnabledSslProtocols | 192 | 768 | 3072);
            }
        }

        public void CloseAsync()
        {
            socket.CloseAsync();
        }

        public void ConnectAsync()
        {
            socket.ConnectAsync();
        }

        public void SendAsync(byte[] data)
        {
            socket.SendAsync(data, null);
        }

        public void SendAsync(string text)
        {
            socket.SendAsync(text, null);
        }
    }
}
#endif
