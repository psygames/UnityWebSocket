#if UNITY_EDITOR || UNITY_WEBGL
using System;

namespace UnityWebSocket.WebGL
{
    public class WebSocket : IWebSocket
    {
        public string Address { get; private set; }
        public WebSocketState ReadyState { get { return (WebSocketState)WebSocketManager.WebSocketGetState(instanceId); } }

        public event EventHandler<OpenEventArgs> OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;

        internal int instanceId;

        public WebSocket(string address)
        {
            this.Address = address;
        }

        internal void HandleOnOpen()
        {
            OnOpen?.Invoke(this, new OpenEventArgs());
        }

        internal void HandleOnMessage(byte[] rawData)
        {
            OnMessage?.Invoke(this, new MessageEventArgs(Opcode.Binary, rawData));
        }

        internal void HandleOnMessageStr(string data)
        {
            OnMessage?.Invoke(this, new MessageEventArgs(Opcode.Text, data));
        }

        internal void HandleOnClose(ushort code, string reason)
        {
            WebSocketManager.Remove(instanceId);
            OnClose?.Invoke(this, new CloseEventArgs(code, reason));
        }

        internal void HandleOnError(string msg)
        {
            OnError?.Invoke(this, new ErrorEventArgs(msg));
        }

        public void ConnectAsync()
        {
            instanceId = WebSocketManager.Add(this);
            int ret = WebSocketManager.WebSocketConnect(instanceId);
            if (ret < 0) HandleOnError(WebSocketManager.GetErrorMessageFromCode(ret));
        }

        public void CloseAsync()
        {
            int ret = WebSocketManager.WebSocketClose(instanceId, (int)CloseStatusCode.Normal, "Normal Closure");
            if (ret < 0) HandleOnError(WebSocketManager.GetErrorMessageFromCode(ret));
        }

        public void SendAsync(string text)
        {
            int ret = WebSocketManager.WebSocketSendStr(instanceId, text);
            if (ret < 0) HandleOnError(WebSocketManager.GetErrorMessageFromCode(ret));
        }

        public void SendAsync(byte[] data)
        {
            int ret = WebSocketManager.WebSocketSend(instanceId, data, data.Length);
            if (ret < 0) HandleOnError(WebSocketManager.GetErrorMessageFromCode(ret));
        }
    }
}
#endif
