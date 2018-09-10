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
            GameObject go = GameObject.Find("/WebSocketReceiver");
            if (go == null)
            {
                go = new GameObject("WebSocketReceiver");
            }

            if (go.GetComponent<WebSocketReceiver>() == null)
            {
                go.AddComponent<WebSocketReceiver>();
            }

            instance = go.GetComponent<WebSocketReceiver>();
        }

        public void AddListener(string address
            , Action onOpen
            , Action<CloseEventArgs> onClose
            , Action<MessageEventArgs> onReceive
            , Action<ErrorEventArgs> onError)
        {
            WebSocketHandle handle = new WebSocketHandle();
            handle.onOpen += onOpen;
            handle.onClose += onClose;
            handle.onReceive += onReceive;
            handle.onError += onError;

            if (!m_handles.ContainsKey(address))
                m_handles.Add(address, null);
            m_handles[address] = handle;
        }

        public void RemoveListener(string address)
        {
            if (m_handles.ContainsKey(address))
                m_handles.Remove(address);
        }

        private WebSocketHandle GetHandle(string address)
        {
            WebSocketHandle handle = null;
            m_handles.TryGetValue(address, out handle);
            return handle;
        }

        /// <summary>
        /// jslib will call this method on message received.
        /// </summary>
        /// <param name="address_data">address_opcode_data(hex string)</param>
        private void OnReceive(string address_opcode_data)
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

            handle.onReceive(this, new MessageEventArgs(opcode, rawData));
        }

        /// <summary>
        /// jslib will call this method on connection open.
        /// </summary>
        /// <param name="address">address</param>
        private void OnOpen(string address)
        {
            if (m_openActions.ContainsKey(address) && m_openActions[address] != null)
            {
                m_openActions[address].Invoke();
            }
        }

        /// <summary>
        /// jslib will call this method on connection closed.
        /// </summary>
        /// <param name="address">address</param>
        private void OnClose(string address)
        {
            if (m_closeActions.ContainsKey(address) && m_closeActions[address] != null)
            {
                m_closeActions[address].Invoke();
            }
        }

        /// <summary>
        /// jslib will call this method on error.
        /// </summary>
        /// <param name="address_data">address_data(text string)</param>
        private void OnError(string address_data)
        {
            string address;
            string errorMsg;
            SplitData(address_data, out address, out errorMsg);
            if (m_errorActions.ContainsKey(address) && m_errorActions[address] != null)
            {
                m_errorActions[address].Invoke(errorMsg);
            }
        }

        private void SplitData(string rawData, int length, out string[] retData)
        {
            retData = new string[length];
            int i = -1;
            int lastIndex = 0;
            while (++i < length)
            {
                var index = rawData.IndexOf('_', lastIndex);
                retData[i] = rawData.Substring(lastIndex, index - lastIndex);
                lastIndex = index + 1;
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


        private class WebSocketHandle
        {
            public event EventHandler onOpen;
            public event EventHandler<CloseEventArgs> onClose;
            public event EventHandler<ErrorEventArgs> onError;
            public event EventHandler<MessageEventArgs> onReceive;

            public void HandleOpen()
            {
                onOpen(this, EventArgs.Empty);
            }

            public void HandleClose(ushort code, string reason)
            {
                onClose(this, new CloseEventArgs(code, reason));
            }

            public void HandleReceive(Opcode code, byte[] rawData)
            {
                onReceive(this, new MessageEventArgs(code, rawData));
            }

            public void HandleError(string message)
            {
                onError(this, new ErrorEventArgs(message));
            }
        }
    }
}
