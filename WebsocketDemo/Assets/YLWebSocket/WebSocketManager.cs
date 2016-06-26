using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YLWebSocket
{
    /// <summary>
    /// <para>This component must add to a GameObject will not be destroyed. </para>
    /// <para>And make sure the GameObject Named with WebSocket.</para>
    /// <para>Cause of Unity will SendMessage to it when receive messages.</para>
    /// <para>需要挂在不会被销毁的GameObject上，</para>
    /// <para>并且保证GameObject的名字为WebSocket，</para>
    /// <para>因为接收消息时候Unity会SendMessage给它。</para>
    /// </summary>
    class WebSocketManager : MonoBehaviour
    {
        private static WebSocketManager s_instance;
        public static WebSocketManager instance { get { return s_instance; } }
        private Dictionary<string, WebSocket> m_socketDict = new Dictionary<string, WebSocket>();

        void Awake()
        {
            s_instance = this;
        }

        private WebSocket CreateSocket(string address)
        {
            WebSocket socket = new WebSocket(address);
            m_socketDict.Add(address, socket);
            return socket;
        }

        /// <summary>
        /// <para>Get or Create a WebSocket</para>
        /// <para>Address format: ws://ip:port/path</para>
        /// <para>返回或创建一个WebSocket</para>
        /// <para>地址格式: ws://ip:port/path</para>
        /// </summary>
        /// <param name="address">address<para>地址</para></param>
        /// <returns>WebScoket</returns>
        public WebSocket GetSocket(string address)
        {
            if (m_socketDict.ContainsKey(address))
                return m_socketDict[address];
            return CreateSocket(address);
        }

        /// <summary>
        /// <para>jslib will invoke when receive messages.</para>
        /// <para>Atteion: In jslib use method SendMessage("WebSocket","OnReceived",data);</para>
        /// <para>接收数据时jslib调用接口</para>
        /// <para>注：在jslib里面SendMessage("WebSocket","OnReceived",data);</para>
        /// </summary>
        /// <param name="msg">string类型消息</param>
        private void OnReceived(string msg)
        {
            WebMessage webMsg = MessageTranslator(msg);
            if (m_socketDict.ContainsKey(webMsg.address))
                m_socketDict[webMsg.address].ReceiveHandle(webMsg.msgType, webMsg.data);
        }

        /// <summary>
        /// <para>Message Translator</para>
        /// <para>消息解析</para>
        /// </summary>
        private WebMessage MessageTranslator(string msg)
        {
            string[] msgArray = SplitRawMsg(msg);
            WebMessage webMsg = new WebMessage();
            // 地址
            webMsg.address = msgArray[0];
            // 类型
            webMsg.msgType = TranslateWebMessageType(msgArray[1]);
            // 消息
            webMsg.data = TranslateWebMessage(msgArray[2]);
            return webMsg;
        }

        /// <summary>
        /// <para>Message split</para>
        /// <para>message contains three parts:address,message type,data</para>
        /// <para>split with ' '</para>
        /// <para>消息头体拆分</para>
        /// <para>消息的 地址、类型、数据 的拆分</para>
        /// <para>使用 ' ' 空格作为间隔 ，按上述顺序组合</para>
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private string[] SplitRawMsg(string msg)
        {
            string[] msgArray = new string[3];
            int bi = 0;
            int ei = -1;
            for (int i = 0; i < msgArray.Length && ei < msg.Length - 2; i++)
            {
                bi = ei + 1;
                ei = msg.IndexOf(' ', bi);
                if (ei < 0)
                    ei = msg.Length;
                msgArray[i] = msg.Substring(bi, ei - bi);
            }
            return msgArray;
        }

        /// <summary>
        /// <para>Message Type Translator</para>
        /// <para>Type the same message type name with jslib </para>
        /// <para>Cn --- Connected</para>
        /// <para>Cl --- Closed</para>
        /// <para>Rv --- Received</para>
        /// <para>消息类型解析</para>
        /// <para>与 jslib 协同 好的消息类型字段</para>
        /// </summary>
        /// <param name="sType"></param>
        /// <returns></returns>
        private WebMessageType TranslateWebMessageType(string sType)
        {
            if (sType == "Cn")
                return WebMessageType.Connected;
            else if (sType == "Cl")
                return WebMessageType.Closed;
            else if (sType == "Rv")
                return WebMessageType.Received;
            return WebMessageType.None;
        }

        /// <summary>
        /// <para>Message Data Translator</para>
        /// <para>Type the same split character with jslib. </para>
        /// <para>byte convert to int and use '-' connect the items.</para>
        /// <para>here for the split above.</para>
        /// <para>消息体解析</para>
        /// <para>与 jslib里面 协同好的 字节 分割方式。</para>
        /// <para>byte 转成 int 使用 - 连接成字符串</para>
        /// <para>这里做上述组合方式的 拆分</para>
        /// </summary>
        /// <param name="sMsg"></param>
        /// <returns></returns>
        private byte[] TranslateWebMessage(string sMsg)
        {
            if (string.IsNullOrEmpty(sMsg))
                return new byte[0];
            string[] msgArray = sMsg.Split('-');
            byte[] data = new byte[msgArray.Length];
            for (int i = 0; i < msgArray.Length; i++)
            {
                data[i] = (byte)int.Parse(msgArray[i]);
            }
            return data;
        }
    }

    public class WebMessage
    {
        public string address;
        public WebMessageType msgType;
        public byte[] data;
    }

    public enum WebMessageType
    {
        None,
        Connected,
        Closed,
        Received,
    }
}
