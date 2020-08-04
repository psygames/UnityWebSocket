using System;
using UnityWebSocket;
using HybridWebSocket;

namespace UnityWebSocket.WebGL2
{
    public class WebSocket : IWebSocket
    {
        public string Address { get; private set; }
        public WebSocketState ReadyState { get { return rawSocket == null ? WebSocketState.Closed : (WebSocketState)rawSocket.GetState(); } }

        public event EventHandler<OpenEventArgs> OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;

        private HybridWebSocket.WebSocket rawSocket;

        private void HandleOnOpen()
        {
            if (OnOpen != null)
                OnOpen.Invoke(this, new OpenEventArgs());
        }

        private void HandleOnMessage(Opcode opcode, byte[] rawData)
        {
            if (OnMessage != null)
                OnMessage.Invoke(this, new MessageEventArgs(opcode, rawData));
        }

        private void HandleOnClose(ushort code, string reason)
        {
            if (OnClose != null)
                OnClose.Invoke(this, new CloseEventArgs(code, reason));
        }

        private void HandleOnError(string msg)
        {
            if (OnError != null)
                OnError.Invoke(this, new ErrorEventArgs(msg));
        }

        public void ConnectAsync(string address)
        {
            this.Address = address;
            rawSocket = WebSocketFactory.CreateInstance(address);
            rawSocket.OnOpen += RawSocket_OnOpen;
            rawSocket.OnMessage += RawSocket_OnMessage;
            rawSocket.OnMessageStr += RawSocket_OnMessageStr;
            rawSocket.OnClose += RawSocket_OnClose;
            rawSocket.OnError += RawSocket_OnError;
            rawSocket.Connect();
        }

        private void RawSocket_OnError(string errorMsg)
        {
            HandleOnError(errorMsg);
        }

        private void RawSocket_OnClose(WebSocketCloseCode closeCode)
        {
            HandleOnClose((ushort)closeCode, "");
        }

        private void RawSocket_OnMessage(byte[] data)
        {
            HandleOnMessage(Opcode.Binary, data);
        }

        private void RawSocket_OnMessageStr(string data)
        {
            var _bytes = System.Text.Encoding.UTF8.GetBytes(data);
            HandleOnMessage(Opcode.Text, _bytes);
        }

        private void RawSocket_OnOpen()
        {
            HandleOnOpen();
        }

        public void CloseAsync()
        {
            rawSocket.Close(WebSocketCloseCode.Normal, "Normal Closure");
        }

        public void SendAsync(string text, Action completed = null)
        {
            rawSocket.Send(text);
            if (completed != null)
                completed.Invoke();
        }

        public void SendAsync(byte[] data, Action completed = null)
        {
            rawSocket.Send(data);
            if (completed != null)
                completed.Invoke();
        }
    }
}
