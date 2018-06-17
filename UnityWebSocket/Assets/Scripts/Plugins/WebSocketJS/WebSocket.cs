using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System;

namespace WebSocketJS
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
        public State state { get; private set; }
        public Action onOpen { get; set; }
        public Action onClose { get; set; }
        public Action<string> onError { get; set; }
        public Action<byte[]> onReceive { get; set; }
        private WebSocket() { }

        public WebSocket(string address)
        {
            if (WebSocketReceiver.instance == null)
            {
                WebSocketReceiver.AutoCreateInstance();
            }
            this.address = address;
            this.state = State.Closed;
        }

        /*------------- call jslib method --------*/
        [DllImport("__Internal")]
        private static extern void ConnectJS(string address);
        [DllImport("__Internal")]
        private static extern void SendJS(string address, byte[] data, int length);
        [DllImport("__Internal")]
        private static extern void CloseJS(string address);

        public void Connect()
        {
            WebSocketReceiver.instance.AddListener(address, OnOpen, OnClose, OnReceive, OnError);
            ConnectJS(address);
            this.state = State.Connecting;
        }

        public void Send(byte[] data)
        {
            SendJS(address, data, data.Length);
        }

        public void Close()
        {
            CloseJS(address);
            this.state = State.Closing;
        }

        private void OnOpen()
        {
            if (onOpen != null)
                onOpen.Invoke();
            this.state = State.Connected;
        }

        private void OnReceive(byte[] msg)
        {
            if (onReceive != null)
                onReceive.Invoke(msg);
        }

        private void OnClose()
        {
            if (onClose != null)
                onClose.Invoke();
            this.state = State.Closed;
            WebSocketReceiver.instance.RemoveListener(address);
        }

        private void OnError(string msg)
        {
            if (onError != null)
                onError.Invoke(msg);
        }

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