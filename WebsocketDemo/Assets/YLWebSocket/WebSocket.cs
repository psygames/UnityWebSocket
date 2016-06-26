using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System;

namespace YLWebSocket
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
        private string m_address;
        private State m_state;

        public string address { get { return m_address; } }
        public State state { get { return m_state; } }
        public Action onConnected;
        public Action onClosed;
        public Action<byte[]> onReceived;

        public WebSocket(string address)
        {
            m_address = address;
            m_state = State.Closed;
        }
        /*------------- 调用jslib里面的方法 --------*/
        [DllImport("__Internal")]
        private static extern void ConnectJS(string str);
        [DllImport("__Internal")]
        private static extern void SendJS(byte[] data, int length);
        [DllImport("__Internal")]
        private static extern void CloseJS();
        [DllImport("__Internal")]
        private static extern void AlertJS(string str);

        public void Connect()
        {
            ConnectJS(m_address);
            m_state = State.Connecting;
        }

        public void Send(byte[] data, int length)
        {
            SendJS(data, length);
        }

        public void Send(byte[] data)
        {
            Send(data, data.Length);
        }

        public void Close()
        {
            CloseJS();
            m_state = State.Closing;
        }

        public void Alert(string str)
        {
            AlertJS(str);
        }

        private void OnConnected()
        {
            if (onConnected != null)
                onConnected.Invoke();
            m_state = State.Connected;
        }

        private void OnReceived(byte[] msg)
        {
            if (onReceived != null)
                onReceived.Invoke(msg);
        }

        private void OnClosed()
        {
            if (onClosed != null)
                onClosed.Invoke();
            m_state = State.Closed;
        }
        /// <summary>
        /// this will invoke by WebSocketManager, you don't need care this.
        /// 这个方法供WebSocketManager调用，你不需要关注他。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public void ReceiveHandle(WebMessageType type, byte[] data = null)
        {
            if (type == WebMessageType.Connected)
                OnConnected();
            else if (type == WebMessageType.Closed)
                OnClosed();
            else if (type == WebMessageType.Received)
                OnReceived(data);
            else
                Alert("Message Type is None!");
        }

        public enum State
        {
            Closed,
            Connecting,
            Connected,
            Closing,
        }
    }
}