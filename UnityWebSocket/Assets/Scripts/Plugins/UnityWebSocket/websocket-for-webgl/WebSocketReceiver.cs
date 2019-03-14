#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWebSocket;

namespace WebSocketJslib
{
    public class WebSocketReceiver : MonoBehaviour
    {
        public static WebSocketReceiver instance { get; private set; }

        private Dictionary<string, WebSocketHandle> m_handles = new Dictionary<string, WebSocketHandle>();

        private WebSocketReceiver()
        { }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }

        public static void AutoCreateInstance()
        {
            GameObject go = GameObject.Find("/[WebSocketReceiver]");
            if (go == null)
            {
                go = new GameObject("[WebSocketReceiver]");
            }

            if (go.GetComponent<WebSocketReceiver>() == null)
            {
                go.AddComponent<WebSocketReceiver>();
            }

            instance = go.GetComponent<WebSocketReceiver>();
        }

        public void AddListener(string address
            , Action onOpen
            , Action<ushort, string, bool> onClose
            , Action<Opcode, byte[]> onReceive
            , Action<string> onError)
        {
            WebSocketHandle handle = new WebSocketHandle();
            handle.onOpen = onOpen;
            handle.onClose = onClose;
            handle.onReceive = onReceive;
            handle.onError = onError;

            if (!m_handles.ContainsKey(address))
                m_handles.Add(address, null);
            m_handles[address] = handle;
        }

        public void RemoveListener(string address)
        {
            if (m_handles.ContainsKey(address))
                m_handles.Remove(address);
        }

        /// <summary>
        /// jslib will call this method on message received.
        /// </summary>
        /// <param name="address_opcode_data">address_opcode_data(hex string)</param>
        private void OnMessage(string address_opcode_data)
        {
            string[] sp;
            SplitData(address_opcode_data, 3, out sp);
            var address = sp[0];
            var opcode_str = sp[1];
            var data_str = sp[2];
            var handle = GetHandle(address);
            if (handle == null)
                return;

            Opcode opcode = (Opcode)int.Parse(opcode_str);
            byte[] rawData = new byte[0];
            if (opcode == Opcode.Text)
                rawData = System.Text.Encoding.UTF8.GetBytes(data_str);
            else if (opcode == Opcode.Binary)
                rawData = HexStrToBytes(data_str);

            handle.onReceive(opcode, rawData);
        }

        /// <summary>
        /// jslib will call this method on connection open.
        /// </summary>
        /// <param name="address">address</param>
        private void OnOpen(string address)
        {
            var handle = GetHandle(address);
            if (handle == null)
                return;
            handle.HandleOpen();
        }

        /// <summary>
        /// jslib will call this method on connection closed.
        /// </summary>
        /// <param name="address_code_reason_wasClean">address_code_reason_wasClean</param>
        private void OnClose(string address_code_reason_wasClean)
        {
            string[] sp;
            SplitData(address_code_reason_wasClean, 4, out sp);
            var address = sp[0];
            var code_str = sp[1];
            var reason = sp[2];
            var wasClean_str = sp[3];
            var handle = GetHandle(address);
            if (handle == null)
                return;
            var code = ushort.Parse(code_str);
            var wasClean = bool.Parse(wasClean_str);
            handle.HandleClose(code, reason, wasClean);
        }

        /// <summary>
        /// jslib will call this method on error.
        /// </summary>
        /// <param name="address_errMsg">address_errMsg(text string)</param>
        private void OnError(string address_errMsg)
        {
            string[] sp;
            SplitData(address_errMsg, 2, out sp);
            string address = sp[0];
            string errorMsg = sp[1];
            var handle = GetHandle(address);
            if (handle == null)
                return;
            handle.HandleError(errorMsg);
        }

        private void SplitData(string rawData, int length, out string[] retData)
        {
            retData = new string[length];
            int i = -1;
            int lastIndex = 0;
            while (++i < length)
            {
                if (i == length - 1)
                {
                    retData[i] = rawData.Substring(lastIndex);
                }
                else
                {
                    var index = rawData.IndexOf('_', lastIndex);
                    retData[i] = rawData.Substring(lastIndex, index - lastIndex);
                    lastIndex = index + 1;
                }
            }
        }

        private byte[] HexStrToBytes(string hexStr)
        {
            byte[] data = new byte[hexStr.Length / 2];
            for (int i = 0; i < data.Length; i++)
            {
                string hex = hexStr.Substring(i * 2, 2);
                data[i] = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            }
            return data;
        }

        private WebSocketHandle GetHandle(string address)
        {
            WebSocketHandle handle = null;
            m_handles.TryGetValue(address, out handle);
            return handle;
        }

        private class WebSocketHandle
        {
            public Action onOpen;
            public Action<ushort, string, bool> onClose;
            public Action<string> onError;
            public Action<Opcode, byte[]> onReceive;

            public void HandleOpen()
            {
                onOpen();
            }

            public void HandleClose(ushort code, string reason, bool wasClean)
            {
                onClose(code, reason, wasClean);
            }

            public void HandleReceive(Opcode code, byte[] rawData)
            {
                onReceive(code, rawData);
            }

            public void HandleError(string message)
            {
                onError(message);
            }
        }
    }
}
#endif