using System.Runtime.InteropServices;
using System;
using UnityWebSocket;

namespace WebSocketJslib
{
    /// <summary>
    /// <para>WebSocket is a network connection.</para>
    /// <para>It can be connecting,connected,closing or closed state. </para>
    /// <para>You can send and receive messages by using it.</para>
    /// <para>Regist receive callback for handling received messages.</para>
    /// <para>WebSocket 表示一个网络连接，</para>
    /// <para>它可以是 connecting connected closing closed 状态，</para>
    /// <para>可以发送和接收消息，</para>
    /// <para>接收消息处理的地方注册消息回调即可。</para>
    /// </summary>
    public class WebSocket
    {
        public string address { get; private set; }
        public WebSocketState readyState { get { return (WebSocketState)GetReadyStateJS(); } }

        public event EventHandler onOpen;
        public event EventHandler<CloseEventArgs> onClose;
        public event EventHandler<ErrorEventArgs> onError;
        public event EventHandler<MessageEventArgs> onReceive;

        private WebSocket() { }

        public WebSocket(string address)
        {
            if (WebSocketReceiver.instance == null)
            {
                WebSocketReceiver.AutoCreateInstance();
            }
            this.address = address;
        }

        /*------------- call jslib method --------*/
        [DllImport("__Internal")]
        private static extern void ConnectJS(string address);
        [DllImport("__Internal")]
        private static extern void SendJS(string address, byte[] data, int length);
        [DllImport("__Internal")]
        private static extern void CloseJS(string address);
        [DllImport("__Internal")]
        private static extern int GetReadyStateJS();


        public void Connect()
        {
            WebSocketReceiver.instance.AddListener(address, onOpen, onClose, onReceive, onError);
            ConnectJS(address);
        }

        public void Send(byte[] data)
        {
            SendJS(address, data, data.Length);
        }

        public void Close()
        {
            CloseJS(address);
        }

        private void OnOpen()
        {
            if (onOpen != null)
                onOpen(this, EventArgs.Empty);
        }

        private void OnReceive(byte[] msg)
        {
            if (onReceive != null)
                onReceive(this,new MessageEventArgs())
        }

        private void OnClose()
        {
            if (onClose != null)
                onClose.Invoke();
            WebSocketReceiver.instance.RemoveListener(address);
        }

        private void OnError(string msg)
        {
            if (onError != null)
                onError.Invoke(msg);
        }
    }
}