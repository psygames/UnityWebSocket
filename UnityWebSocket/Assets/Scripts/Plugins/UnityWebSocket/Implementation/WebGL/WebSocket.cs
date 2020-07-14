using System.Runtime.InteropServices;
using System;
using UnityWebSocket;


namespace UnityWebSocket.WebGL
{
    /// <summary>
    /// <para>WebSocket indicate a network connection.</para>
    /// <para>It can be connecting, connected, closing or closed state. </para>
    /// <para>You can send and receive messages by using it.</para>
    /// <para>Register receive callback for handling received messages.</para>
    /// <para>WebSocket 表示一个网络连接，</para>
    /// <para>它可以是 connecting connected closing closed 状态，</para>
    /// <para>可以发送和接收消息，</para>
    /// <para>接收消息处理的地方注册消息回调即可。</para>
    /// </summary>
    public class WebSocket : IWebSocket
    {
        public string Address { get; private set; }
        public WebSocketState ReadyState { get { return (WebSocketState)GetReadyStateJS(Address); } }

        public event EventHandler OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageEventArgs> OnMessage;

        public WebSocket()
        {
            if (WebSocketReceiver.Instance == null)
            {
                WebSocketReceiver.AutoCreateInstance();
            }
        }

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
                OnOpen.Invoke(this, EventArgs.Empty);
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

        public void SendAsync(string text, Action<bool> completed)
        {
            SendStrJS(Address, text);
            if (completed != null)
                completed.Invoke(true);
        }

        public void SendAsync(byte[] data, Action<bool> completed)
        {
            SendJS(Address, data, data.Length);
            if (completed != null)
                completed.Invoke(true);
        }
    }
}
