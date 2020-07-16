using System.Runtime.InteropServices;
using System;
using UnityWebSocket;


namespace UnityWebSocket.WebGL
{
    public class WebSocket : IWebSocket
    {
        public string Address { get; private set; }
        public WebSocketState ReadyState { get { return (WebSocketState)GetReadyStateJS(Address); } }

        public event EventHandler<OpenEventArgs> OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;

        /*------------- call jslib method --------*/
        [DllImport("__Internal")]
        private static extern void ConnectJS(string address);
        [DllImport("__Internal")]
        private static extern void SendJS(string address, byte[] data, int length);
        [DllImport("__Internal")]
        private static extern void SendStrJS(string address, string msg);
        [DllImport("__Internal")]
        private static extern void CloseJS(string address);
        [DllImport("__Internal")]
        private static extern int GetReadyStateJS(string address);

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

        private void HandleOnClose(ushort code, string reason, bool wasClean)
        {
            if (OnClose != null)
                OnClose.Invoke(this, new CloseEventArgs(code, reason, wasClean));
            WebSocketReceiver.Instance.RemoveListener(Address);
        }

        private void HandleOnError(string msg)
        {
            if (OnError != null)
                OnError.Invoke(this, new ErrorEventArgs(msg));
        }

        public void ConnectAsync(string address)
        {
            this.Address = address;
            WebSocketReceiver.Instance.AddListener(Address, HandleOnOpen, HandleOnClose, HandleOnMessage, HandleOnError);
            ConnectJS(Address);
        }

        public void CloseAsync()
        {
            CloseJS(Address);
        }

        public void SendAsync(string text, Action completed = null)
        {
            SendStrJS(Address, text);
            if (completed != null)
                completed.Invoke();
        }

        public void SendAsync(byte[] data, Action completed = null)
        {
            SendJS(Address, data, data.Length);
            if (completed != null)
                completed.Invoke();
        }
    }
}
